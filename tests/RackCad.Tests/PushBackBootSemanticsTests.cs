using System;
using System.Collections.Generic;
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
        /// Pero NO deshabilita la configuracion manual. En la linea que el blanco dejo sin cara de entrada, el
        /// automatico solo pone la posterior; una eleccion EXPLICITA coloca exactamente lo que pide — incluida la
        /// de entrada, sobre el poste que el blanco dejo expuesto.
        /// </summary>
        [Theory]
        [InlineData(BootPlacement.EntryExit, 1)]
        [InlineData(BootPlacement.Rear, 1)]
        [InlineData(BootPlacement.Both, 2)]
        [InlineData(BootPlacement.None, 0)]
        public void Blank_DoesNotDisableBootOverride(BootPlacement placement, int expected)
        {
            var automatic = Resolve(Blanked(), general: null);
            var line = BlankedLine(automatic);

            // El automatico de esa linea: solo la posterior, porque su cara de entrada cae en la interfaz.
            Assert.Single(LineBoots(automatic, line));

            var overridden = Resolve(Blanked(), general: null, (2, placement));
            Assert.Equal(expected, LineBoots(overridden, line).Count);
            Assert.Equal(Boots(overridden).Count, BootsInBom(overridden));
        }

        /// <summary>La linea que el blanco dejo con una sola bota automatica.</summary>
        private static double BlankedLine(PushBackSystem automatic)
            => Boots(automatic)
                .GroupBy(boot => Math.Round(boot.Insertion.Y, 2))
                .First(group => group.Count() == 1)
                .Key;

        /// <summary>Y una eleccion explicita de entrada/salida ahi coloca su bota, no la del otro extremo.</summary>
        [Fact]
        public void Blank_ExplicitEntryExit_CanMaterialize()
        {
            var automatic = Resolve(Blanked(), general: null);
            var line = BlankedLine(automatic);
            var overridden = Resolve(Blanked(), general: null, (2, BootPlacement.EntryExit));

            var before = LineBoots(automatic, line);
            var after = LineBoots(overridden, line);

            Assert.Single(after);
            Assert.NotEqual(before[0], after[0]);   // la ubicacion cambio: es la que se pidio
        }

        [Fact]
        public void Blank_ExplicitNone_MaterializesNone()
        {
            var automatic = Resolve(Blanked(), general: null);
            var line = BlankedLine(automatic);

            Assert.Empty(LineBoots(Resolve(Blanked(), general: null, (2, BootPlacement.None)), line));
        }

        /// <summary>Y la bota configurada a mano se queda en SU linea: ninguna otra se mueve ni desaparece.</summary>
        [Fact]
        public void Blank_BootDoesNotRelocate()
        {
            var automatic = Resolve(Blanked(), general: null);
            var line = BlankedLine(automatic);
            var overridden = Resolve(Blanked(), general: null, (2, BootPlacement.Both));

            foreach (var other in Lines(automatic).Where(y => Math.Abs(y - line) > 0.01))
            {
                Assert.Equal(LineBoots(automatic, other), LineBoots(overridden, other));
            }

            Assert.Equal(2, LineBoots(overridden, line).Count);
        }

        /// <summary>Ni cruza al otro lado: el lado B conserva exactamente sus botas.</summary>
        [Fact]
        public void Blank_BootDoesNotCrossSide()
        {
            var automatic = Resolve(Blanked(), general: null);
            var overridden = Resolve(Blanked(), general: null, (2, BootPlacement.EntryExit));
            var middle = automatic.Structure.TotalLength / 2.0;

            var farBefore = Boots(automatic).Count(boot => boot.Insertion.X > middle);
            var farAfter = Boots(overridden).Count(boot => boot.Insertion.X > middle);

            Assert.Equal(farBefore - 1, farAfter);   // solo la de ESA linea, que se pidio de entrada
        }

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

        /// <summary>BITE — el OVERRIDE en blanco. Es lo unico que decide si el filtro de cara se salta.</summary>
        [Fact]
        public void Bite_BlankOverride_IsWhatBypassesTheAutomaticFilter()
        {
            var automatic = Resolve(Blanked(), general: null);
            var line = BlankedLine(automatic);

            Assert.Single(LineBoots(automatic, line));
            Assert.Equal(2, LineBoots(Resolve(Blanked(), general: null, (2, BootPlacement.Both)), line).Count);
        }
    }
}
