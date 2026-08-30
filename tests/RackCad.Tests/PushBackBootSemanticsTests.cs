using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 (S1B, contrato final del dueño) — LOS PROTECTORES DE BOTA:
    /// <b>Ninguno · Entrada/Salida · Posterior · Ambas</b>, tambien POR POSTE.
    ///
    /// <para>
    /// <b>Lo que la ronda S1 se equivoco en suponer.</b> Que la bota protege solo la cara desde la que se carga. No:
    /// la bota protege el POSTE de un impacto, y detras de un rack que no esta contra muro puede haber un pasillo de
    /// transito. La cara POSTERIOR es configurable aunque nunca se opere producto desde ella, asi que la restriccion
    /// «posterior solo si es cara de carga» queda RETIRADA.
    /// </para>
    ///
    /// <para>
    /// <b>El contrato.</b> La eleccion nombra UBICACIONES FISICAS: <c>Entrada/Salida</c> la cara del frente
    /// operativo, <c>Posterior</c> la opuesta, <c>Ambas</c> las dos —una vez cada una—, <c>Ninguno</c> ninguna. La
    /// ORIENTACION la decide la ubicacion, nunca la eleccion. Y «por defecto» no es una quinta ubicacion: es la
    /// ausencia de decision propia, que hereda la general.
    /// </para>
    ///
    /// <para>
    /// <b>Contenido.</b> Es una semantica PROPIA de esta familia (<see cref="BootPlacement"/>). El PROTECTOR LATERAL
    /// sigue leyendo Izquierda/Derecha como orientacion en su sitio (I-32) y el DESVIADOR sigue en el extremo bajo
    /// (R1): ninguno cambia.
    /// </para>
    /// </summary>
    public class PushBackBootSemanticsTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static string BootId => Catalog.SafetyElements
            .First(entry => SelectiveSafetyDefaults.IsType(entry.Type, SelectiveSafetyDefaults.BotaType)).Id;

        private static PushBackEditorInputs Inputs()
        {
            var inputs = PushBackEditorInputs.NewDesign();
            inputs.Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg");
            return inputs;
        }

        private static PushBackCompositeEditorState State(int slots, bool composite)
        {
            var state = new PushBackCompositeEditorState();
            state.SideA.LoadNew();
            state.SetSlotCount(slots);
            if (composite)
            {
                state.SetSideBPresent(true);
                state.SideB.LoadNew();
                state.SetSlotCount(slots);
                for (var slot = 0; slot < slots; slot++)
                {
                    state.SetSlotPresent(PushBackSide.B, slot, true);
                }
            }

            return state;
        }

        /// <summary>Resuelve el rack con una seleccion de bota: general y, si se pide, overrides por poste.</summary>
        private static PushBackSystem Resolve(
            PushBackCompositeEditorState state,
            BootPlacement? general,
            params (int Post, BootPlacement Placement)[] perPost)
        {
            var design = new PushBackCompositeEditorAssembler(Catalog).Build(state, Inputs(), Catalog).Design;
            design.Structure.SafetySelections.Clear();

            var selection = new SelectiveSafetySelection { ElementId = BootId, Quantity = 1 };
            if (general.HasValue)
            {
                selection.Bota.Placement = general.Value;
                selection.Side = BootPlacements.To(general.Value);
            }

            foreach (var (post, placement) in perPost)
            {
                selection.Bota.Posts.Add(new BootPostPlacement { PostIndex = post, Placement = placement });
            }

            design.Structure.SafetySelections.Add(selection);
            return new PushBackResolver(Catalog).Resolve(design);
        }

        private static IReadOnlyList<HeaderBlockInstance> Boots(PushBackSystem system)
        {
            var id = BootId;
            return new PushBackSystemPlantaBuilder().BuildPlan(system, Catalog).Flatten().Instances
                .Where(instance => string.Equals(instance.PieceId, id, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        private static IReadOnlyList<string> Locations(PushBackSystem system)
            => Boots(system)
                .Select(instance => FormattableString.Invariant(
                    $"{instance.Insertion.X:0.##}|{instance.Insertion.Y:0.##}"))
                .Distinct()
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToList();

        private static int BootsInBom(PushBackSystem system)
        {
            var id = BootId;
            return PushBackBomBuilder.Build(system, Catalog).Lines
                .Where(line => string.Equals(line.ProfileId, id, StringComparison.OrdinalIgnoreCase))
                .Sum(line => line.Quantity);
        }

        /// <summary>Las botas de una linea concreta (su Y), en orden de X.</summary>
        private static IReadOnlyList<double> LineBoots(PushBackSystem system, double y)
            => Boots(system)
                .Where(instance => Math.Abs(instance.Insertion.Y - y) < 0.01)
                .Select(instance => Math.Round(instance.Insertion.X, 2))
                .OrderBy(value => value)
                .ToList();

        private static IReadOnlyList<double> Lines(PushBackSystem system)
            => Boots(system).Select(instance => Math.Round(instance.Insertion.Y, 2)).Distinct().OrderBy(y => y).ToList();

        // ==================================================================== semantica

        [Fact]
        public void BootMode_None_ProducesNone()
        {
            foreach (var composite in new[] { false, true })
            {
                var system = Resolve(State(2, composite), BootPlacement.None);
                Assert.Empty(Boots(system));
                Assert.Equal(0, BootsInBom(system));
            }
        }

        [Fact]
        public void BootMode_EntryExit_ProducesEntryExitLocation()
        {
            var system = Resolve(State(2, composite: false), BootPlacement.EntryExit);
            var middle = system.Structure.TotalLength / 2.0;

            Assert.NotEmpty(Boots(system));
            Assert.All(Boots(system), boot => Assert.True(boot.Insertion.X < middle));
        }

        /// <summary>
        /// La POSTERIOR se materializa aunque no sea cara de carga: detras puede haber un pasillo de transito. Es
        /// exactamente lo que la ronda S1 prohibia y el dueño rechazo.
        /// </summary>
        [Fact]
        public void BootMode_Rear_ProducesRearLocation()
        {
            var system = Resolve(State(2, composite: false), BootPlacement.Rear);
            var middle = system.Structure.TotalLength / 2.0;

            Assert.NotEmpty(Boots(system));
            Assert.All(Boots(system), boot => Assert.True(boot.Insertion.X > middle));
        }

        [Fact]
        public void BootMode_Both_ProducesTwoDistinctLocations()
        {
            var state = State(2, composite: false);
            var entry = Locations(Resolve(state, BootPlacement.EntryExit));
            var rear = Locations(Resolve(state, BootPlacement.Rear));
            var both = Locations(Resolve(state, BootPlacement.Both));

            Assert.NotEmpty(entry);
            Assert.NotEmpty(rear);
            Assert.Empty(entry.Intersect(rear));
            Assert.Equal(entry.Concat(rear).OrderBy(value => value, StringComparer.Ordinal), both);
        }

        [Theory]
        [InlineData(BootPlacement.EntryExit)]
        [InlineData(BootPlacement.Rear)]
        [InlineData(BootPlacement.Both)]
        public void BootMode_Both_DoesNotDuplicateSameLocation(BootPlacement placement)
        {
            foreach (var composite in new[] { false, true })
            {
                var system = Resolve(State(2, composite), placement);
                Assert.Equal(Locations(system).Count, Boots(system).Count);
            }
        }

        /// <summary>La orientacion se DERIVA de la ubicacion: la posterior es la imagen espejo de la de entrada.</summary>
        [Fact]
        public void BootMode_OrientationIsDerivedFromPhysicalLocation()
        {
            Assert.False(SelectiveSafetyEnds.Mirror(farEnd: false));
            Assert.True(SelectiveSafetyEnds.Mirror(farEnd: true));

            var system = Resolve(State(2, composite: false), BootPlacement.Both);
            var middle = system.Structure.TotalLength / 2.0;
            Assert.All(Boots(system), boot => Assert.Equal(boot.Insertion.X > middle, boot.MirroredX));
        }

        /// <summary>
        /// Y NO depende de si esa cara carga producto: en un Push Back de un solo sentido —cuyo extremo lejano no
        /// es cara de carga— «Posterior» sigue colocando su bota.
        /// </summary>
        [Fact]
        public void BootMode_DoesNotDependOnLoadFaceApplicability()
        {
            var selection = new SelectiveSafetySelection { ElementId = BootId, LowEndOnly = true };
            selection.Bota.Placement = BootPlacement.Rear;

            var copy = Assert.Single(SelectiveSafetyEnds.BootCopiesForPost(selection, 0));
            Assert.True(copy.AtHighEnd);

            var system = Resolve(State(2, composite: false), BootPlacement.Rear);
            Assert.NotEmpty(Boots(system));
        }

        // ==================================================================== por poste

        [Fact]
        public void BootPost_Default_InheritsGeneralMode()
        {
            var selection = new SelectiveSafetySelection { ElementId = BootId };
            selection.Bota.Placement = BootPlacement.Rear;

            Assert.Equal(BootPlacement.Rear, selection.BootPlacementAt(0));
            Assert.False(selection.HasOwnBootPlacement(0));
        }

        [Theory]
        [InlineData(BootPlacement.None)]
        [InlineData(BootPlacement.EntryExit)]
        [InlineData(BootPlacement.Rear)]
        [InlineData(BootPlacement.Both)]
        public void BootPost_Override_BeatsTheGeneralMode(BootPlacement own)
        {
            var selection = new SelectiveSafetySelection { ElementId = BootId };
            selection.Bota.Placement = BootPlacement.EntryExit;
            selection.Bota.Posts.Add(new BootPostPlacement { PostIndex = 1, Placement = own });

            Assert.Equal(own, selection.BootPlacementAt(1));
            Assert.Equal(BootPlacement.EntryExit, selection.BootPlacementAt(0));
            Assert.True(selection.HasOwnBootPlacement(1));
        }

        /// <summary>El patron exacto del contrato del dueño, materializado.</summary>
        [Fact]
        public void BootPost_TheOwnerPattern_MaterializesExactly()
        {
            var system = Resolve(
                State(3, composite: false),
                BootPlacement.EntryExit,
                (1, BootPlacement.Rear),
                (2, BootPlacement.Both),
                (3, BootPlacement.None));

            var middle = system.Structure.TotalLength / 2.0;
            var lines = Lines(system);
            var all = Boots(system);

            // Poste 1 hereda entrada/salida; 2 posterior; 3 las dos; 4 ninguna.
            Assert.Equal(3, lines.Count);   // la linea 4 se queda sin ninguna
            Assert.Single(all.Where(b => Math.Abs(b.Insertion.Y - lines[0]) < 0.01));
            Assert.All(all.Where(b => Math.Abs(b.Insertion.Y - lines[0]) < 0.01),
                b => Assert.True(b.Insertion.X < middle));
            Assert.Single(all.Where(b => Math.Abs(b.Insertion.Y - lines[1]) < 0.01));
            Assert.All(all.Where(b => Math.Abs(b.Insertion.Y - lines[1]) < 0.01),
                b => Assert.True(b.Insertion.X > middle));
            Assert.Equal(2, all.Count(b => Math.Abs(b.Insertion.Y - lines[2]) < 0.01));
        }

        /// <summary>Cambiar la general mueve SOLO los postes «por defecto»; los explicitos no se enteran.</summary>
        [Fact]
        public void ChangingGeneral_OnlyChangesDefaultPosts()
        {
            var selection = new SelectiveSafetySelection { ElementId = BootId };
            selection.Bota.Placement = BootPlacement.EntryExit;
            selection.Bota.Posts.Add(new BootPostPlacement { PostIndex = 1, Placement = BootPlacement.Rear });
            selection.Bota.Posts.Add(new BootPostPlacement { PostIndex = 2, Placement = BootPlacement.Both });
            selection.Bota.Posts.Add(new BootPostPlacement { PostIndex = 3, Placement = BootPlacement.None });

            Assert.Equal(BootPlacement.EntryExit, selection.BootPlacementAt(0));

            selection.Bota.Placement = BootPlacement.Rear;

            Assert.Equal(BootPlacement.Rear, selection.BootPlacementAt(0));      // el «por defecto» sigue la general
            Assert.Equal(BootPlacement.Rear, selection.BootPlacementAt(1));      // explicito, coincide por casualidad
            Assert.Equal(BootPlacement.Both, selection.BootPlacementAt(2));      // explicito, intacto
            Assert.Equal(BootPlacement.None, selection.BootPlacementAt(3));      // explicito, intacto

            selection.Bota.Placement = BootPlacement.EntryExit;
            Assert.Equal(BootPlacement.EntryExit, selection.BootPlacementAt(0));
            Assert.Equal(BootPlacement.Both, selection.BootPlacementAt(2));
        }

        [Fact]
        public void BootPost_OverridesRemainStable_AcrossDeepCopy()
        {
            var selection = new SelectiveSafetySelection { ElementId = BootId };
            selection.Bota.Placement = BootPlacement.Both;
            selection.Bota.Posts.Add(new BootPostPlacement { PostIndex = 2, Placement = BootPlacement.Rear });

            var copy = selection.DeepCopy();

            Assert.Equal(BootPlacement.Both, copy.Bota.Placement);
            Assert.Equal(BootPlacement.Rear, copy.BootPlacementAt(2));
            Assert.NotSame(selection.Bota, copy.Bota);
        }

        // ==================================================================== blanks

        /// <summary>Un rack con dos frentes en blanco, como la columna de nave del dueño.</summary>
        private static PushBackCompositeEditorState Blanked()
        {
            var state = State(4, composite: true);
            state.SetSlotPresent(PushBackSide.A, 1, false);
            state.SetSlotPresent(PushBackSide.A, 2, false);
            return state;
        }

        /// <summary>Por DEFECTO un blanco no inventa botas: es el automatico, y no hay razon para protegerlo.</summary>
        [Fact]
        public void Blank_DefaultBoot_CanResolveNone()
        {
            var full = Resolve(State(4, composite: true), general: null);
            var blanked = Resolve(Blanked(), general: null);

            Assert.True(Boots(blanked).Count < Boots(full).Count);
            Assert.All(Locations(blanked), location => Assert.Contains(location, Locations(full)));
        }

        /// <summary>
        /// Pero NO deshabilita la configuracion manual. RETARGETEADO EN S1C: el automatico de esa linea ya no deja
        /// nada —antes conservaba la posterior, que era la mitad que el filtro de caras no retiraba—, y una eleccion
        /// EXPLICITA sigue colocando exactamente lo que pide, incluida la de entrada sobre el poste expuesto.
        /// </summary>
        [Theory]
        [InlineData(BootPlacement.EntryExit, 1)]
        [InlineData(BootPlacement.Rear, 1)]
        [InlineData(BootPlacement.Both, 2)]
        [InlineData(BootPlacement.None, 0)]
        public void Blank_DoesNotDisableBootOverride(BootPlacement placement, int expected)
        {
            var line = BlankedLine();
            Assert.Empty(LineBoots(Resolve(Blanked(), general: null), line));

            var overridden = Resolve(Blanked(), general: null, (2, placement));
            Assert.Equal(expected, LineBoots(overridden, line).Count);
            Assert.Equal(Boots(overridden).Count, BootsInBom(overridden));
        }

        /// <summary>
        /// La LINEA que el blanco dejo sin pasillo propio. Se identifica por diferencia con el rack COMPLETO —es la
        /// unica que el automatico ya no protege—, no por cuantas botas le quedan: desde S1C no le queda ninguna.
        /// </summary>
        private static double BlankedLine()
            => Lines(Resolve(State(4, composite: true), general: null))
                .Except(Lines(Resolve(Blanked(), general: null)))
                .Single();

        /// <summary>
        /// Y una eleccion explicita de entrada/salida ahi coloca su bota. RETARGETEADO EN S1C: la comparacion ya no
        /// es «cambio de ubicacion» sino «aparece donde no habia nada», porque el automatico de esa linea es vacio.
        /// </summary>
        [Fact]
        public void Blank_ExplicitEntryExit_CanMaterialize()
        {
            var line = BlankedLine();
            var overridden = Resolve(Blanked(), general: null, (2, BootPlacement.EntryExit));

            Assert.Empty(LineBoots(Resolve(Blanked(), general: null), line));
            Assert.Single(LineBoots(overridden, line));
            Assert.True(LineBoots(overridden, line)[0] < overridden.Structure.TotalLength / 2.0);
        }

        [Fact]
        public void Blank_ExplicitNone_MaterializesNone()
            => Assert.Empty(LineBoots(
                Resolve(Blanked(), general: null, (2, BootPlacement.None)), BlankedLine()));

        /// <summary>Y la bota configurada a mano se queda en SU linea: ninguna otra se mueve ni desaparece.</summary>
        [Fact]
        public void Blank_BootDoesNotRelocate()
        {
            var automatic = Resolve(Blanked(), general: null);
            var line = BlankedLine();
            var overridden = Resolve(Blanked(), general: null, (2, BootPlacement.Both));

            foreach (var other in Lines(automatic).Where(y => Math.Abs(y - line) > 0.01))
            {
                Assert.Equal(LineBoots(automatic, other), LineBoots(overridden, other));
            }

            Assert.Equal(2, LineBoots(overridden, line).Count);
        }

        /// <summary>
        /// Ni cruza al otro lado: el lado B conserva exactamente sus botas. RETARGETEADO EN S1C: pedir la de entrada
        /// en la linea en blanco AÑADE esa —antes ademas retiraba la posterior que el automatico dejaba ahi—.
        /// </summary>
        [Fact]
        public void Blank_BootDoesNotCrossSide()
        {
            var automatic = Resolve(Blanked(), general: null);
            var overridden = Resolve(Blanked(), general: null, (2, BootPlacement.EntryExit));
            var middle = automatic.Structure.TotalLength / 2.0;

            Assert.Equal(
                Boots(automatic).Count(boot => boot.Insertion.X > middle),
                Boots(overridden).Count(boot => boot.Insertion.X > middle));
            Assert.Equal(
                Boots(automatic).Count(boot => boot.Insertion.X < middle) + 1,
                Boots(overridden).Count(boot => boot.Insertion.X < middle));
        }

        // ==================================================================== S1C: el blanco resuelve a NINGUNO

        /// <summary>
        /// I-42 (S1C, contrato del dueño) — EL DEFECTO DE UNA LINEA EN BLANCO ES «NINGUNA».
        ///
        /// <para>
        /// S1B dejaba ahi una bota posterior. No era una decision: era el residuo del filtro de caras, que retiraba
        /// la cara que el blanco se llevo y conservaba la otra. Un blanco quita la razon de proteger esa linea
        /// ENTERA, asi que el automatico no coloca nada — y sigue sin bloquear nada: una eleccion propia manda.
        /// </para>
        /// </summary>
        [Fact]
        public void Blank_DefaultBoot_ResolvesNone()
            => Assert.Empty(LineBoots(Resolve(Blanked(), general: null), BlankedLine()));

        [Fact]
        public void Blank_DefaultBoot_IsNone_ForGeneralEntryExit()
            => Assert.Empty(LineBoots(Resolve(Blanked(), BootPlacement.EntryExit), BlankedLine()));

        /// <summary>La general POSTERIOR tampoco se cuela: el blanco decide el defecto de esa linea, sea cual sea.</summary>
        [Fact]
        public void Blank_DefaultBoot_IsNone_ForGeneralRear()
            => Assert.Empty(LineBoots(Resolve(Blanked(), BootPlacement.Rear), BlankedLine()));

        [Fact]
        public void Blank_DefaultBoot_IsNone_ForGeneralBoth()
            => Assert.Empty(LineBoots(Resolve(Blanked(), BootPlacement.Both), BlankedLine()));

        [Fact]
        public void Blank_DefaultBoot_IsNone_ForGeneralNone()
            => Assert.Empty(LineBoots(Resolve(Blanked(), BootPlacement.None), BlankedLine()));

        /// <summary>Y las cuatro elecciones explicitas materializan EXACTAMENTE lo de S1B, que el dueño no reabrio.</summary>
        [Fact]
        public void Blank_ExplicitEntryExit_StillMaterializes()
        {
            var system = Resolve(Blanked(), BootPlacement.EntryExit, (2, BootPlacement.EntryExit));
            var boots = LineBoots(system, BlankedLine());

            Assert.Single(boots);
            Assert.True(boots[0] < system.Structure.TotalLength / 2.0);
        }

        [Fact]
        public void Blank_ExplicitRear_StillMaterializes()
        {
            var system = Resolve(Blanked(), BootPlacement.EntryExit, (2, BootPlacement.Rear));
            var boots = LineBoots(system, BlankedLine());

            Assert.Single(boots);
            Assert.True(boots[0] > system.Structure.TotalLength / 2.0);
        }

        [Fact]
        public void Blank_ExplicitBoth_StillMaterializesTwo()
        {
            var system = Resolve(Blanked(), BootPlacement.EntryExit, (2, BootPlacement.Both));
            var boots = LineBoots(system, BlankedLine());
            var middle = system.Structure.TotalLength / 2.0;

            Assert.Equal(2, boots.Count);
            Assert.Single(boots, x => x < middle);
            Assert.Single(boots, x => x > middle);
        }

        [Fact]
        public void Blank_ExplicitNone_StillMaterializesNone()
            => Assert.Empty(LineBoots(
                Resolve(Blanked(), BootPlacement.EntryExit, (2, BootPlacement.None)), BlankedLine()));

        /// <summary>
        /// El blanco afecta la RESOLUCION, no la intencion: el poste sigue en «por defecto» —sin entrada propia y
        /// sin general reescrita—, que es lo que le permite volver solo cuando el blanco se quita.
        /// </summary>
        [Fact]
        public void Blank_DefaultIntent_IsNotRewrittenToExplicitNone()
        {
            var system = Resolve(Blanked(), BootPlacement.EntryExit);
            var selection = system.Structure.SafetySelections.Single(IsBootSelection);

            Assert.False(selection.HasOwnBootPlacement(2));
            Assert.Empty(selection.Bota.Posts);
            Assert.Equal(BootPlacement.EntryExit, selection.Bota.Placement);
            Assert.Equal(BootPlacement.None, selection.BootPlacementAt(2));   // efectivo, mientras siga en blanco
        }

        /// <summary>Y al quitar el blanco esa linea recupera SOLA lo que la general diga.</summary>
        [Fact]
        public void LeavingBlank_RestoresInheritedGeneralMode()
        {
            var line = BlankedLine();

            Assert.Empty(LineBoots(Resolve(Blanked(), BootPlacement.EntryExit), line));

            var full = Resolve(State(4, composite: true), BootPlacement.EntryExit);
            Assert.Single(LineBoots(full, line));
            Assert.True(LineBoots(full, line)[0] < full.Structure.TotalLength / 2.0);
        }

        /// <summary>Un override explicito no se pierde ni se degrada a defecto al entrar y salir del blanco.</summary>
        [Fact]
        public void Blank_ExplicitOverride_SurvivesLeavingBlank()
        {
            var line = BlankedLine();
            var blanked = Resolve(Blanked(), BootPlacement.EntryExit, (2, BootPlacement.Rear));
            var full = Resolve(State(4, composite: true), BootPlacement.EntryExit, (2, BootPlacement.Rear));

            Assert.Equal(LineBoots(blanked, line), LineBoots(full, line));
            Assert.Single(LineBoots(full, line));
            Assert.True(LineBoots(full, line)[0] > full.Structure.TotalLength / 2.0);
        }

        /// <summary>El defecto del blanco no muda nada: las demas lineas conservan sus botas exactamente.</summary>
        [Fact]
        public void Blank_Default_DoesNotRelocate()
        {
            var blanked = Resolve(Blanked(), general: null);
            var full = Resolve(State(4, composite: true), general: null);
            var line = BlankedLine();

            foreach (var other in Lines(full).Where(y => Math.Abs(y - line) > 0.01))
            {
                Assert.Equal(LineBoots(full, other), LineBoots(blanked, other));
            }
        }

        /// <summary>Ni cruza al otro lado: cada mitad pierde SU bota de esa linea y ninguna otra se mueve.</summary>
        [Fact]
        public void Blank_Default_DoesNotCrossSide()
        {
            var blanked = Resolve(Blanked(), general: null);
            var full = Resolve(State(4, composite: true), general: null);
            var middle = full.Structure.TotalLength / 2.0;

            Assert.Equal(
                Boots(full).Count(boot => boot.Insertion.X > middle) - 1,
                Boots(blanked).Count(boot => boot.Insertion.X > middle));
            Assert.Equal(
                Boots(full).Count(boot => boot.Insertion.X < middle) - 1,
                Boots(blanked).Count(boot => boot.Insertion.X < middle));
        }

        [Fact]
        public void Blank_Default_DrawEqualsBom()
        {
            foreach (var general in new BootPlacement?[]
                     { null, BootPlacement.EntryExit, BootPlacement.Rear, BootPlacement.Both, BootPlacement.None })
            {
                var system = Resolve(Blanked(), general);
                Assert.Equal(Boots(system).Count, BootsInBom(system));
            }
        }

        /// <summary>
        /// RACKEDITAR: «por defecto» y «Ninguno» son cosas distintas y el documento las distingue. Nada de lo que
        /// S1C resuelve se escribe: el blanco no deja rastro persistido.
        /// </summary>
        [Fact]
        public void RackEditar_PreservesDefaultVsExplicitNone()
        {
            var selection = new SelectiveSafetySelection { ElementId = BootId, Quantity = 1 };
            selection.Bota.Placement = BootPlacement.EntryExit;
            selection.Bota.Posts.Add(new BootPostPlacement { PostIndex = 4, Placement = BootPlacement.None });
            selection.PostsWithoutAutomaticBoot.Add(2);   // lo que el sistema resolvio en esta sesion

            var restored = SafetySelectionDocument.From(selection).ToDomain();

            Assert.Equal(BootPlacement.EntryExit, restored.Bota.Placement);
            Assert.False(restored.HasOwnBootPlacement(2));                  // P2 sigue en «por defecto»
            Assert.Equal(BootPlacement.None, restored.Bota.At(4));          // P4 sigue en «Ninguno» explicito
            Assert.Empty(restored.PostsWithoutAutomaticBoot);               // derivado: no se persiste
            Assert.Equal(BootPlacement.EntryExit, restored.BootPlacementAt(2));
        }

        private static bool IsBootSelection(SelectiveSafetySelection selection)
            => string.Equals(selection?.ElementId, BootId, StringComparison.OrdinalIgnoreCase);

        // ==================================================================== simple / compuesto

        /// <summary>Sin eleccion, un rack de UN pasillo protege su frente operativo. Es el comportamiento historico.</summary>
        [Fact]
        public void Simple_DefaultIsEntryExit()
        {
            var system = Resolve(State(2, composite: false), general: null);
            var middle = system.Structure.TotalLength / 2.0;

            Assert.NotEmpty(Boots(system));
            Assert.All(Boots(system), boot => Assert.True(boot.Insertion.X < middle));
        }

        /// <summary>Y uno de DOS pasillos protege los dos, sin pedirlo (R6).</summary>
        [Fact]
        public void Composite_DefaultProtectsBothAisles()
        {
            var system = Resolve(State(2, composite: true), general: null);
            var middle = system.Structure.TotalLength / 2.0;

            Assert.Contains(Boots(system), boot => boot.Insertion.X < middle);
            Assert.Contains(Boots(system), boot => boot.Insertion.X > middle);
        }

        [Fact]
        public void Simple_RearAndBoth_AreAllowed()
        {
            var state = State(2, composite: false);
            Assert.NotEmpty(Boots(Resolve(state, BootPlacement.Rear)));
            Assert.Equal(
                Boots(Resolve(state, BootPlacement.EntryExit)).Count + Boots(Resolve(state, BootPlacement.Rear)).Count,
                Boots(Resolve(state, BootPlacement.Both)).Count);
        }

        /// <summary>La colocacion NO codifica el lado: es la cara longitudinal, y vale igual en simple y compuesto.</summary>
        [Fact]
        public void Composite_BootModeDoesNotEncodeSide()
        {
            var simple = Resolve(State(2, composite: false), BootPlacement.EntryExit);
            var composite = Resolve(State(2, composite: true), BootPlacement.EntryExit);
            var simpleMiddle = simple.Structure.TotalLength / 2.0;
            var compositeMiddle = composite.Structure.TotalLength / 2.0;

            Assert.All(Boots(simple), boot => Assert.True(boot.Insertion.X < simpleMiddle));
            Assert.All(Boots(composite), boot => Assert.True(boot.Insertion.X < compositeMiddle));
        }

        // ==================================================================== dibujo == BOM

        [Theory]
        [InlineData(null)]
        [InlineData(BootPlacement.None)]
        [InlineData(BootPlacement.EntryExit)]
        [InlineData(BootPlacement.Rear)]
        [InlineData(BootPlacement.Both)]
        public void BootDraw_EqualsBom(BootPlacement? placement)
        {
            foreach (var composite in new[] { false, true })
            {
                var system = Resolve(State(2, composite), placement);
                Assert.Equal(Boots(system).Count, BootsInBom(system));
            }
        }

        // ==================================================================== legacy

        [Theory]
        [InlineData(SafetySide.None, BootPlacement.None)]
        [InlineData(SafetySide.Left, BootPlacement.EntryExit)]
        [InlineData(SafetySide.Right, BootPlacement.Rear)]
        [InlineData(SafetySide.Both, BootPlacement.Both)]
        public void LegacyBoot_MapsByIntention(SafetySide side, BootPlacement expected)
        {
            Assert.Equal(expected, BootPlacements.From(side));
            Assert.Equal(side, BootPlacements.To(expected));
        }

        /// <summary>
        /// Un documento anterior no trae colocacion propia: se lee por su LADO historico, con la intencion que esas
        /// etiquetas tenian. Y una entrada por poste historica sigue mandando sobre la general.
        /// </summary>
        [Fact]
        public void LegacySelection_ReadsItsHistoricSide()
        {
            var selection = new SelectiveSafetySelection { ElementId = BootId, Side = SafetySide.Right };
            selection.PostSides.Add(new SafetyPostSide { PostIndex = 1, Side = SafetySide.Both });

            Assert.Null(selection.Bota.Placement);
            Assert.Equal(BootPlacement.Rear, selection.BootPlacementAt(0));
            Assert.Equal(BootPlacement.Both, selection.BootPlacementAt(1));
            Assert.True(selection.HasOwnBootPlacement(1));
        }

        /// <summary>
        /// Y el AUTOMATICO solo entra cuando nadie ha elegido nada: un lado historico explicito lo desplaza.
        /// </summary>
        /// <summary>
        /// El AUTOMATICO solo lo fija la autoridad del SISTEMA —y solo Push Back lo hace—, y cede ante una eleccion
        /// EXPLICITA. Un Selectivo o un Dinamico nunca lo tienen, asi que su lado historico sigue mandando.
        /// </summary>
        [Fact]
        public void AutomaticPlacement_YieldsToAnExplicitChoice()
        {
            var chosen = new SelectiveSafetySelection { ElementId = BootId };
            chosen.Bota.Placement = BootPlacement.Rear;
            chosen.AutomaticBootPlacement = BootPlacement.EntryExit;
            Assert.Equal(BootPlacement.Rear, chosen.BootPlacementAt(0));

            var perPost = new SelectiveSafetySelection { ElementId = BootId };
            perPost.Bota.Posts.Add(new BootPostPlacement { PostIndex = 0, Placement = BootPlacement.Both });
            perPost.AutomaticBootPlacement = BootPlacement.EntryExit;
            Assert.Equal(BootPlacement.Both, perPost.BootPlacementAt(0));

            var untouched = new SelectiveSafetySelection { ElementId = BootId, Side = SafetySide.Both };
            untouched.AutomaticBootPlacement = BootPlacement.EntryExit;
            Assert.Equal(BootPlacement.EntryExit, untouched.BootPlacementAt(0));

            // Sin automatico —Selectivo, Dinamico— manda el lado historico.
            var selective = new SelectiveSafetySelection { ElementId = BootId, Side = SafetySide.Right };
            Assert.Equal(BootPlacement.Rear, selective.BootPlacementAt(0));
        }

        // ==================================================================== otras familias

        /// <summary>El PROTECTOR LATERAL conserva su contrato de I-32: esta ronda no lo toca.</summary>
        [Fact]
        public void LateralGuardRegression_KeepsItsOwnContract()
        {
            var selection = new SelectiveSafetySelection { ElementId = "GUARD", Side = SafetySide.Right };
            selection.LowEndOnly = true;

            var copy = Assert.Single(SelectiveSafetyEnds.CopiesForPost(selection, 0));

            Assert.False(copy.AtHighEnd);   // delante
            Assert.True(copy.Mirrored);     // y espejado, que es su orientacion elegida
        }

        // ==================================================================== bite tests

        /// <summary>BITE — el MAPPING de ubicaciones. Romperlo cambia que caras se ocupan, y nada mas.</summary>
        [Fact]
        public void Bite_Mapping_EntryExitAndRearAreDifferentFaces()
        {
            var entry = new SelectiveSafetySelection { ElementId = BootId };
            entry.Bota.Placement = BootPlacement.EntryExit;
            var rear = new SelectiveSafetySelection { ElementId = BootId };
            rear.Bota.Placement = BootPlacement.Rear;

            Assert.False(Assert.Single(SelectiveSafetyEnds.BootCopiesForPost(entry, 0)).AtHighEnd);
            Assert.True(Assert.Single(SelectiveSafetyEnds.BootCopiesForPost(rear, 0)).AtHighEnd);
        }

        /// <summary>BITE — la ORIENTACION. Es funcion SOLO de la cara: romperla no mueve ninguna ubicacion.</summary>
        [Fact]
        public void Bite_Orientation_IsAFunctionOfTheFaceAlone()
        {
            Assert.NotEqual(SelectiveSafetyEnds.Mirror(farEnd: false), SelectiveSafetyEnds.Mirror(farEnd: true));

            foreach (var placement in new[] { BootPlacement.EntryExit, BootPlacement.Rear, BootPlacement.Both })
            {
                var selection = new SelectiveSafetySelection { ElementId = BootId };
                selection.Bota.Placement = placement;
                foreach (var copy in SelectiveSafetyEnds.BootCopiesForPost(selection, 0))
                {
                    Assert.Equal(SelectiveSafetyEnds.Mirror(copy.AtHighEnd), copy.Mirrored);
                }
            }
        }

        /// <summary>BITE — el DEFAULT del blanco. Solo gobierna lo que nadie eligio.</summary>
        [Fact]
        public void Bite_BlankDefault_OnlyGovernsWhatNobodyChose()
        {
            var selection = new SelectiveSafetySelection { ElementId = BootId };
            Assert.False(selection.HasOwnBootPlacement(2));

            selection.Bota.Posts.Add(new BootPostPlacement { PostIndex = 2, Placement = BootPlacement.Rear });
            Assert.True(selection.HasOwnBootPlacement(2));
            Assert.False(selection.HasOwnBootPlacement(1));
        }

        /// <summary>BITE — el OVERRIDE en blanco. Es lo unico que coloca una bota en una linea en blanco.</summary>
        [Fact]
        public void Bite_BlankOverride_IsWhatBypassesTheAutomaticFilter()
        {
            var line = BlankedLine();

            Assert.Empty(LineBoots(Resolve(Blanked(), general: null), line));
            Assert.Equal(2, LineBoots(Resolve(Blanked(), general: null, (2, BootPlacement.Both)), line).Count);
        }

        /// <summary>
        /// BITE (S1C) — LA DECLARACION DEL BLANCO es lo unico que silencia el automatico, y silencia SOLO eso.
        /// Anularla devuelve la linea a lo que herede; no toca ninguna de las cuatro elecciones explicitas ni
        /// ninguna otra linea.
        /// </summary>
        [Fact]
        public void Bite_BlankDeclaration_OnlySilencesTheAutomatic()
        {
            var selection = new SelectiveSafetySelection { ElementId = BootId };
            selection.Bota.Placement = BootPlacement.Both;

            Assert.Equal(BootPlacement.Both, selection.BootPlacementAt(2));

            selection.PostsWithoutAutomaticBoot.Add(2);
            Assert.Equal(BootPlacement.None, selection.BootPlacementAt(2));
            Assert.Equal(BootPlacement.Both, selection.BootPlacementAt(1));   // otra linea: intacta

            foreach (var chosen in new[]
                     { BootPlacement.None, BootPlacement.EntryExit, BootPlacement.Rear, BootPlacement.Both })
            {
                selection.Bota.Posts.Clear();
                selection.Bota.Posts.Add(new BootPostPlacement { PostIndex = 2, Placement = chosen });
                Assert.Equal(chosen, selection.BootPlacementAt(2));           // la decision propia manda
            }
        }
    }
}
