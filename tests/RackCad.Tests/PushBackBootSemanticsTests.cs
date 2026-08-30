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
    /// I-42 (S1) — LOS PROTECTORES DE BOTA: Ninguno / Izquierda / Derecha / Ambas.
    ///
    /// <para>
    /// <b>El defecto, medido.</b> Una bota protege el POSTE del golpe del montacargas, y el montacargas ataca por un
    /// pasillo. Pero el lado no elegia pasillos: en un Push Back la autoridad colapsaba la eleccion a
    /// <c>Izquierda</c> antes de que nadie la leyera, asi que <b>las tres opciones daban exactamente la misma
    /// bota</b> —3 piezas en la misma cara, con el mismo BOM— y en un compuesto las tres daban las dos caras. El
    /// selector era inerte. Ademas, la autoridad compartida emitia para <c>Ambas</c> con dos caras de carga CUATRO
    /// copias sobre DOS sitios: dos piezas dibujadas y contadas sobre el mismo poste.
    /// </para>
    ///
    /// <para>
    /// <b>El contrato.</b> El lado elige UBICACIONES FISICAS: <c>Izquierda</c> el extremo cercano, <c>Derecha</c> el
    /// lejano, <c>Ambas</c> los dos. Cada ubicacion aparece UNA vez y solo existe donde ese extremo es una CARA DE
    /// ATAQUE — un extremo contra muro no se protege. La ORIENTACION la decide la cara, nunca la eleccion.
    /// </para>
    ///
    /// <para>
    /// <b>Contenido.</b> Es una regla de ESTA familia. El PROTECTOR LATERAL lee Izquierda/Derecha como orientacion
    /// en su sitio (contrato validado en I-32) y el DESVIADOR sigue en el extremo bajo (R1): ninguno cambia.
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

        private static PushBackSystem Resolve(
            PushBackCompositeEditorState state, SafetySide side, params (int Post, SafetySide Side)[] perPost)
        {
            var design = new PushBackCompositeEditorAssembler(Catalog).Build(state, Inputs(), Catalog).Design;
            design.Structure.SafetySelections.Clear();
            if (side != SafetySide.None || perPost.Length > 0)
            {
                var selection = new SelectiveSafetySelection { ElementId = BootId, Quantity = 1, Side = side };
                foreach (var (post, postSide) in perPost)
                {
                    selection.PostSides.Add(new SafetyPostSide { PostIndex = post, Side = postSide });
                }

                design.Structure.SafetySelections.Add(selection);
            }

            return new PushBackResolver(Catalog).Resolve(design);
        }

        /// <summary>Las UBICACIONES fisicas donde hay bota, sin repetir.</summary>
        private static IReadOnlyList<string> Locations(PushBackSystem system)
            => Boots(system)
                .Select(instance => FormattableString.Invariant(
                    $"{instance.Insertion.X:0.##}|{instance.Insertion.Y:0.##}"))
                .Distinct()
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToList();

        private static IReadOnlyList<HeaderBlockInstance> Boots(PushBackSystem system)
        {
            var id = BootId;
            return new PushBackSystemPlantaBuilder().BuildPlan(system, Catalog).Flatten().Instances
                .Where(instance => string.Equals(instance.PieceId, id, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        private static int BootsInBom(PushBackSystem system)
        {
            var id = BootId;
            return PushBackBomBuilder.Build(system, Catalog).Lines
                .Where(line => string.Equals(line.ProfileId, id, StringComparison.OrdinalIgnoreCase))
                .Sum(line => line.Quantity);
        }

        /// <summary>La X del pasillo de cada lado, para saber en cual cae una bota.</summary>
        private static double Near(PushBackSystem system) => 0.0;

        private static double Far(PushBackSystem system) => system.Structure.TotalLength;

        private static int At(PushBackSystem system, bool farEnd)
        {
            var middle = system.Structure.TotalLength / 2.0;
            return Boots(system).Count(instance =>
                farEnd ? instance.Insertion.X >= middle : instance.Insertion.X < middle);
        }

        // ==================================================================== el contrato

        [Fact]
        public void BootSelection_None_HasNoPhysicalBoots()
        {
            foreach (var composite in new[] { false, true })
            {
                var system = Resolve(State(2, composite), SafetySide.None);
                Assert.Empty(Boots(system));
                Assert.Equal(0, BootsInBom(system));
            }
        }

        /// <summary>
        /// Izquierda y Derecha son UBICACIONES DISTINTAS, no la misma con otro espejo: en un compuesto cada una
        /// protege un pasillo, y sus conjuntos fisicos no se solapan en ninguna pieza.
        /// </summary>
        [Fact]
        public void BootSelection_Left_MapsToPhysicalLocationsNotMirrorOnly()
        {
            var system = Resolve(State(2, composite: true), SafetySide.Left);

            Assert.NotEmpty(Locations(system));
            Assert.Equal(Boots(system).Count, Locations(system).Count);   // ni una repetida
            Assert.True(At(system, farEnd: false) > 0);
            Assert.Equal(0, At(system, farEnd: true));
        }

        [Fact]
        public void BootSelection_Right_MapsToPhysicalLocationsNotMirrorOnly()
        {
            var system = Resolve(State(2, composite: true), SafetySide.Right);

            Assert.NotEmpty(Locations(system));
            Assert.Equal(Boots(system).Count, Locations(system).Count);
            Assert.Equal(0, At(system, farEnd: false));
            Assert.True(At(system, farEnd: true) > 0);
        }

        /// <summary>Y sus conjuntos son DISJUNTOS: ninguna ubicacion aparece en los dos.</summary>
        [Fact]
        public void BootSelection_LeftAndRight_AreDisjointPhysicalSets()
        {
            var left = Locations(Resolve(State(2, composite: true), SafetySide.Left));
            var right = Locations(Resolve(State(2, composite: true), SafetySide.Right));

            Assert.NotEmpty(left);
            Assert.NotEmpty(right);
            Assert.Empty(left.Intersect(right));
        }

        /// <summary>Ambas es la UNION fisica de las dos, sin duplicar ninguna ubicacion.</summary>
        [Fact]
        public void BootSelection_Both_IsPhysicalUnionWithoutDuplicateLocation()
        {
            var state = State(2, composite: true);
            var left = Locations(Resolve(state, SafetySide.Left));
            var right = Locations(Resolve(state, SafetySide.Right));
            var both = Resolve(state, SafetySide.Both);

            Assert.Equal(
                left.Concat(right).OrderBy(value => value, StringComparer.Ordinal),
                Locations(both));
            Assert.Equal(left.Count + right.Count, Boots(both).Count);   // una pieza por ubicacion
            Assert.Equal(Boots(both).Count, Locations(both).Count);
        }

        /// <summary>Ninguna opcion pone DOS botas sobre el mismo poste y cara solo por espejar una.</summary>
        [Theory]
        [InlineData(SafetySide.Left)]
        [InlineData(SafetySide.Right)]
        [InlineData(SafetySide.Both)]
        public void BootSelection_DoesNotDuplicateSamePhysicalPostByMirror(SafetySide side)
        {
            foreach (var composite in new[] { false, true })
            {
                var system = Resolve(State(2, composite), side);
                Assert.Equal(Locations(system).Count, Boots(system).Count);
            }
        }

        /// <summary>
        /// La ORIENTACION no define la pertenencia: la copia del pasillo lejano es la imagen espejo de la del
        /// cercano, y eso es consecuencia de QUE cara protege — no una segunda pieza.
        /// </summary>
        [Fact]
        public void BootOrientation_DoesNotDefinePhysicalMembership()
        {
            Assert.False(SelectiveSafetyEnds.Mirror(farEnd: false));
            Assert.True(SelectiveSafetyEnds.Mirror(farEnd: true));

            var system = Resolve(State(2, composite: true), SafetySide.Both);
            var middle = system.Structure.TotalLength / 2.0;

            Assert.All(Boots(system), boot => Assert.Equal(boot.Insertion.X >= middle, boot.MirroredX));
        }

        // ==================================================================== caras de ataque

        /// <summary>
        /// Un Push Back de un solo sentido tiene UNA cara de ataque: su pasillo. «Derecha» pide el extremo lejano,
        /// que esta contra muro, asi que no coloca nada — y no muda la bota al pasillo, que es lo que hacia antes.
        /// </summary>
        [Fact]
        public void PushBackBoot_UsesApplicableLowAttackFaces()
        {
            var state = State(2, composite: false);

            Assert.NotEmpty(Boots(Resolve(state, SafetySide.Left)));
            Assert.Empty(Boots(Resolve(state, SafetySide.Right)));
            Assert.Equal(
                Locations(Resolve(state, SafetySide.Left)),
                Locations(Resolve(state, SafetySide.Both)));
        }

        /// <summary>Un compuesto SI tiene dos caras de ataque, y las dos pueden llevar bota.</summary>
        [Fact]
        public void CompositeBoots_CanExistOnBothDistinctLowFaces()
        {
            var system = Resolve(State(2, composite: true), SafetySide.Both);

            Assert.True(At(system, farEnd: false) > 0);
            Assert.True(At(system, farEnd: true) > 0);
            Assert.Equal(Boots(system).Count, At(system, farEnd: false) + At(system, farEnd: true));
        }

        /// <summary>El DEFECTO de un rack nuevo es «Ambas», asi que un compuesto nace con los dos pasillos protegidos.</summary>
        [Fact]
        public void ANewRack_DefaultsToBothAttackFaces()
        {
            var defaults = new PushBackSafetyAuthority(Catalog).Defaults();
            var boot = defaults.First(selection =>
                string.Equals(selection.ElementId, BootId, StringComparison.OrdinalIgnoreCase));

            Assert.Equal(SafetySide.Both, boot.AuthoredSide);
        }

        // ==================================================================== blanks

        /// <summary>Un blanco quita la NECESIDAD: la bota desaparece y no se muda a ningun otro sitio.</summary>
        [Fact]
        public void BlankLowFace_RemovesBootWithoutRelocation()
        {
            var full = Resolve(State(3, composite: true), SafetySide.Both);
            var blanked = State(3, composite: true);
            blanked.SetSlotPresent(PushBackSide.A, 0, false);
            blanked.SetSlotPresent(PushBackSide.A, 1, false);
            var system = Resolve(blanked, SafetySide.Both);

            var before = Locations(full);
            var after = Locations(system);

            Assert.True(after.Count < before.Count, "el blanco quita al menos una bota");
            Assert.All(after, location => Assert.Contains(location, before));   // ninguna ubicacion NUEVA
        }

        [Fact]
        public void BlankBoot_DoesNotMoveToInteriorOrOppositeSide()
        {
            var blanked = State(3, composite: true);
            blanked.SetSlotPresent(PushBackSide.A, 0, false);
            blanked.SetSlotPresent(PushBackSide.A, 1, false);
            var system = Resolve(blanked, SafetySide.Both);

            var interiorLow = system.Composite.SideA.InnerX;
            var interiorHigh = system.Composite.SideB.InnerX;

            Assert.All(Boots(system), boot =>
            {
                Assert.True(Math.Abs(boot.Insertion.X - interiorLow) > 1.0, "ninguna bota en la interfaz");
                Assert.True(Math.Abs(boot.Insertion.X - interiorHigh) > 1.0, "ninguna bota en la interfaz");
            });
        }

        // ==================================================================== por poste

        /// <summary>La eleccion POR POSTE manda sobre la general y no se contagia a los demas.</summary>
        [Fact]
        public void BootPerPostOverride_RemainsScoped()
        {
            var state = State(2, composite: true);
            var system = Resolve(state, SafetySide.Both, (1, SafetySide.None));
            var all = Resolve(state, SafetySide.Both);

            var lines = Boots(all).Select(boot => Math.Round(boot.Insertion.Y, 3)).Distinct().OrderBy(y => y).ToList();
            var kept = Boots(system).Select(boot => Math.Round(boot.Insertion.Y, 3)).Distinct().OrderBy(y => y).ToList();

            Assert.Equal(lines.Count - 1, kept.Count);          // exactamente una linea sin bota
            Assert.All(kept, y => Assert.Contains(y, lines));   // y ninguna se movio
        }

        /// <summary>Una entrada por poste NUNCA la colapsa la autoridad: es del usuario, y se lee literal.</summary>
        [Fact]
        public void BootPerPostOverride_IsNeverCollapsedByTheAuthority()
        {
            var selection = new SelectiveSafetySelection { ElementId = BootId, Side = SafetySide.Both };
            selection.PostSides.Add(new SafetyPostSide { PostIndex = 0, Side = SafetySide.Right });
            PushBackSafetyAuthority.RestrictToLowEnd(selection);

            Assert.Equal(SafetySide.Right, selection.ChosenSide(0));
            Assert.Equal(SafetySide.Both, selection.ChosenSide(1));   // el general conserva la eleccion
        }

        // ==================================================================== dibujo == BOM

        [Theory]
        [InlineData(SafetySide.None)]
        [InlineData(SafetySide.Left)]
        [InlineData(SafetySide.Right)]
        [InlineData(SafetySide.Both)]
        public void BootDraw_EqualsBom(SafetySide side)
        {
            foreach (var composite in new[] { false, true })
            {
                var system = Resolve(State(2, composite), side);
                Assert.Equal(Boots(system).Count, BootsInBom(system));
            }
        }

        // ==================================================================== regresiones de otros sistemas

        /// <summary>
        /// SELECTIVO y DINAMICO cargan por los DOS extremos, asi que los dos son caras de ataque y el contrato
        /// historico se cumple sin cambiar nada: Izquierda el cercano, Derecha el lejano, Ambas los dos.
        /// </summary>
        [Fact]
        public void SelectiveAndDynamicBootRegression_BothEndsRemainAttackFaces()
        {
            foreach (var side in new[] { SafetySide.Left, SafetySide.Right, SafetySide.Both })
            {
                var selection = new SelectiveSafetySelection { ElementId = BootId, Side = side };
                var copies = SelectiveSafetyEnds.BootCopiesForPost(selection, 0);

                var expected = side == SafetySide.Both ? 2 : 1;
                Assert.Equal(expected, copies.Count);
                Assert.Equal(expected, copies.Select(copy => copy.AtHighEnd).Distinct().Count());
                Assert.Equal(side != SafetySide.Right, copies.Any(copy => !copy.AtHighEnd));
                Assert.Equal(side != SafetySide.Left, copies.Any(copy => copy.AtHighEnd));
            }
        }

        /// <summary>
        /// El PROTECTOR LATERAL conserva su contrato de I-32 —Izquierda/Derecha es orientacion en su sitio y un
        /// Derecha en Push Back se queda delante, espejado—: esta ronda no lo toca.
        /// </summary>
        [Fact]
        public void LateralGuardRegression_KeepsItsOwnContract()
        {
            var selection = new SelectiveSafetySelection { ElementId = "GUARD", Side = SafetySide.Right };
            selection.LowEndOnly = true;

            var copies = SelectiveSafetyEnds.CopiesForPost(selection, 0);
            var copy = Assert.Single(copies);

            Assert.False(copy.AtHighEnd);   // delante
            Assert.True(copy.Mirrored);     // y espejado, que es su orientacion elegida
        }

        // ==================================================================== bite tests

        /// <summary>
        /// BITE — la PERTENENCIA. Si la regla de ubicaciones se rompiera y las tres opciones volvieran a dar el
        /// mismo conjunto, esto lo dice; y no depende de ningun espejo.
        /// </summary>
        [Fact]
        public void Bite_Membership_ThreeOptionsProduceThreeDifferentPhysicalSets()
        {
            var state = State(2, composite: true);
            var none = Locations(Resolve(state, SafetySide.None));
            var left = Locations(Resolve(state, SafetySide.Left));
            var right = Locations(Resolve(state, SafetySide.Right));
            var both = Locations(Resolve(state, SafetySide.Both));

            Assert.Empty(none);
            Assert.NotEqual(left, right);
            Assert.NotEqual(left, both);
            Assert.NotEqual(right, both);
        }

        /// <summary>
        /// BITE — la ORIENTACION. Depende SOLO de la cara, asi que romperla mueve espejos y no ubicaciones: el
        /// conjunto fisico de cada opcion sigue siendo el mismo.
        /// </summary>
        [Fact]
        public void Bite_Orientation_IsAFunctionOfTheFaceAlone()
        {
            Assert.Equal(SelectiveSafetyEnds.Mirror(farEnd: false), SelectiveSafetyEnds.Mirror(farEnd: false));
            Assert.NotEqual(SelectiveSafetyEnds.Mirror(farEnd: false), SelectiveSafetyEnds.Mirror(farEnd: true));

            foreach (var side in new[] { SafetySide.Left, SafetySide.Right, SafetySide.Both })
            {
                var selection = new SelectiveSafetySelection { ElementId = BootId, Side = side };
                foreach (var copy in SelectiveSafetyEnds.BootCopiesForPost(selection, 0))
                {
                    Assert.Equal(SelectiveSafetyEnds.Mirror(copy.AtHighEnd), copy.Mirrored);
                }
            }
        }

        /// <summary>
        /// BITE — la APLICABILIDAD. Es la unica que decide si un extremo puede llevar bota, y romperla solo afecta
        /// a los casos donde ese extremo no es cara de ataque.
        /// </summary>
        [Fact]
        public void Bite_Applicability_OnlyGovernsTheFarEnd()
        {
            var open = new SelectiveSafetySelection { ElementId = BootId };
            Assert.True(SelectiveSafetyEnds.IsAttackFace(open, 0, farEnd: false));
            Assert.True(SelectiveSafetyEnds.IsAttackFace(open, 0, farEnd: true));

            var lowOnly = new SelectiveSafetySelection { ElementId = BootId, LowEndOnly = true };
            Assert.True(SelectiveSafetyEnds.IsAttackFace(lowOnly, 0, farEnd: false));
            Assert.False(SelectiveSafetyEnds.IsAttackFace(lowOnly, 0, farEnd: true));

            var composite = new SelectiveSafetySelection { ElementId = BootId, LowEndOnly = true };
            composite.BothEndsAreLoadFaces = true;
            Assert.True(SelectiveSafetyEnds.IsAttackFace(composite, 0, farEnd: true));
        }

        // ==================================================================== legacy

        /// <summary>
        /// LEGACY — el lado elegido sobrevive a la restriccion del sistema. La autoridad sigue colapsando el lado
        /// GENERAL para las familias que lo leen como extremo, pero la eleccion original queda registrada y es la
        /// que lee la bota: sin ella, las tres opciones volverian a ser la misma.
        /// </summary>
        [Theory]
        [InlineData(SafetySide.Left)]
        [InlineData(SafetySide.Right)]
        [InlineData(SafetySide.Both)]
        public void LegacyBootSelection_RoundTrips(SafetySide side)
        {
            var selection = new SelectiveSafetySelection { ElementId = BootId, Side = side };
            PushBackSafetyAuthority.RestrictToLowEnd(selection);

            Assert.Equal(side, selection.AuthoredSide);
            Assert.Equal(side, selection.ChosenSide(0));
            Assert.Equal(side, selection.DeepCopy().AuthoredSide);
        }

        /// <summary>
        /// Y una seleccion que NUNCA paso por una autoridad restrictiva —todo Selectivo, todo Dinamico y todo
        /// documento anterior— se lee por su lado de siempre: el campo nuevo es aditivo y NULL no cambia nada.
        /// </summary>
        [Fact]
        public void LegacySelectionWithoutAuthoredSide_ReadsItsOwnSide()
        {
            var selection = new SelectiveSafetySelection { ElementId = BootId, Side = SafetySide.Right };

            Assert.Null(selection.AuthoredSide);
            Assert.Equal(SafetySide.Right, selection.ChosenSide(0));
        }
    }
}
