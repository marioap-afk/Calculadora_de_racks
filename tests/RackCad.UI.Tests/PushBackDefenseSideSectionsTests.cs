using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using RackCad.Application.Drawing;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.UI;
using RackCad.UI.Systems.PushBack;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-42 (ronda 7D, contrato del dueño) — LA DEFENSA SE EDITA POR LADO, dentro de «Elementos de seguridad».
    ///
    /// <para>
    /// <b>El defecto.</b> Tras la ronda 7C el ON/OFF por poste ya atravesaba la ruta completa, pero seguia
    /// aplicandose siempre al lado A. Medido: en un compuesto la ventana ofrecia UNA sola superficie de defensa,
    /// cuya rejilla hablaba de «entrada/salida» y «posterior» —el vocabulario de un rack de un solo sentido—. La
    /// columna baja movia la cara del lado A; la alta se pintaba con la regla de un rack de un sentido, aparecia
    /// apagada aunque el lado B si llevara defensa, y no habia forma de decidir B. El lado nunca «se perdia» en una
    /// frontera: no estaba expresado en ninguna.
    /// </para>
    ///
    /// <para>
    /// <b>La identidad.</b> Una intencion es LADO + LINEA FISICA. La ronda 6D ya habia establecido que un compuesto
    /// tiene dos pasillos, uno en cada extremo de la cobertura de cada linea, y que el dibujo coloca el del cercano
    /// con <c>ExitLength</c> y el del lejano con <c>EntranceLength</c>: el registro por poste YA distinguia las dos
    /// caras. Lo que faltaba era nombrarlas por lado. <see cref="PushBackDefenseSides"/> es donde eso esta escrito.
    /// </para>
    /// </summary>
    public sealed class PushBackDefenseSideSectionsTests
    {
        private static RackPushBackSystemWindow Shown()
        {
            var w = new RackPushBackSystemWindow
            {
                WindowStartupLocation = WindowStartupLocation.Manual,
                ShowInTaskbar = false,
                Left = -10000,
                Top = -10000,
            };
            w.Show();
            w.UpdateLayout();
            return w;
        }

        private static RackPushBackSystemWindow Composite()
        {
            var w = Shown();
            w.CompositeState.SetSlotCount(3);
            w.CompositeState.SetSideBPresent(true);
            for (var slot = 0; slot < 3; slot++)
            {
                w.CompositeState.SetSlotPresent(PushBackSide.B, slot, true);
            }

            Settle(w);
            return w;
        }

        /// <summary>
        /// Recalcula el modelo, que es lo que la ventana hace sola en cuanto el usuario toca un control. Estas
        /// pruebas escriben en el estado compuesto directamente, asi que lo piden a mano: sin ello mediria contra
        /// una estructura vieja, no contra el rack que el usuario tiene delante.
        /// </summary>
        private static void Settle(RackPushBackSystemWindow w)
        {
            w.SafetyDialog = selections => selections.ToList();
            EditorWindowTestSupport.ClickNamed(w, "SafetyButton");
            w.SafetyDialog = null;
        }

        /// <summary>El gesto del usuario sobre la rejilla REAL de una seccion: encender/apagar postes.</summary>
        private static Action<PushBackDefenseSection, SafetyDefensaGridWindow> Set(params (int post, bool on)[] wanted)
            => (_, grid) =>
            {
                foreach (var (post, on) in wanted)
                {
                    grid.ExitCheckForTest(post).IsChecked = on;
                }
            };

        /// <summary>
        /// Recorre las ventanas REALES: «Elementos de seguridad», y dentro de ella la rejilla por poste de cada
        /// seccion, construida con la CARA que la seccion declara. Solo se sustituye el <c>ShowDialog</c>.
        /// </summary>
        private static void Through(
            RackPushBackSystemWindow w,
            Action<PushBackDefenseSection, SafetyDefensaGridWindow> gestureA = null,
            Action<PushBackDefenseSection, SafetyDefensaGridWindow> gestureB = null,
            bool accept = true,
            Action<PushBackDefenseSection> inspect = null)
        {
            w.DefenseDialog = section =>
            {
                inspect?.Invoke(section);
                var grid = new SafetyDefensaGridWindow(
                    "Defensa de montacargas", section.PostCount, section.Posts,
                    lowEndOnly: true, autoPerEnd: true, face: section.Face());
                (section.Side == PushBackSide.A ? gestureA : gestureB)?.Invoke(section, grid);
                grid.BuildResultForTest();
                return grid.Accepted ? grid.Result : null;
            };

            w.SafetyWindowDialog = dialog =>
            {
                dialog.WindowStartupLocation = WindowStartupLocation.Manual;
                dialog.ShowInTaskbar = false;
                dialog.Left = -10000;
                dialog.Top = -10000;
                dialog.Show();
                dialog.UpdateLayout();
                if (gestureA != null || inspect != null) { w.DefenseSectionForTest?.Configure(); }
                if (gestureB != null || inspect != null) { w.DefenseSectionBForTest?.Configure(); }
                dialog.BuildResultForTest();
                dialog.Close();
                return accept;
            };
            EditorWindowTestSupport.ClickNamed(w, "SafetyButton");
            w.SafetyWindowDialog = null;
            w.DefenseDialog = null;
        }

        /// <summary>Las defensas del dibujo, como «X|Y».</summary>
        private static IReadOnlyList<string> Defenses(RackPushBackSystemWindow w)
        {
            var system = w.LastComputation?.System;
            if (system == null)
            {
                return new List<string>();
            }

            var catalog = w.Session.Catalog;
            var id = catalog.SafetyElements
                .First(entry => SelectiveSafetyDefaults.IsType(entry.Type, SelectiveSafetyDefaults.DefensaType)).Id;
            return new PushBackSystemPlantaBuilder().BuildPlan(system, catalog).Flatten().Instances
                .Where(instance => instance.Role == HeaderBlockRole.Safety
                    && string.Equals(instance.PieceId, id, StringComparison.OrdinalIgnoreCase))
                .Select(instance => FormattableString.Invariant(
                    $"{instance.Insertion.X:0.###}|{instance.Insertion.Y:0.###}"))
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToList();
        }

        private static int DefensesInBom(RackPushBackSystemWindow w)
        {
            var system = w.LastComputation?.System;
            if (system == null)
            {
                return 0;
            }

            var catalog = w.Session.Catalog;
            var id = catalog.SafetyElements
                .First(entry => SelectiveSafetyDefaults.IsType(entry.Type, SelectiveSafetyDefaults.DefensaType)).Id;
            return PushBackBomBuilder.Build(system, catalog).Lines
                .Where(line => string.Equals(line.ProfileId, id, StringComparison.OrdinalIgnoreCase))
                .Sum(line => line.Quantity);
        }

        private static SelectiveSafetySelection Defense(RackPushBackSystemWindow w)
            => w.SafetySelections.FirstOrDefault(selection =>
            {
                var element = w.Session.Catalog.SafetyElements?.FirstOrDefault(entry =>
                    string.Equals(entry?.Id, selection.ElementId, StringComparison.OrdinalIgnoreCase));
                return element != null
                       && SelectiveSafetyDefaults.IsType(element.Type, SelectiveSafetyDefaults.DefensaType);
            });

        /// <summary>
        /// El estado RESUELTO de un lado en cada linea: lo que ese lado materializa de verdad. Se lee del sistema
        /// RESUELTO, no de la copia del editor: las marcas de segunda cara de carga son DERIVADAS y solo existen
        /// despues de resolver.
        /// </summary>
        private static IReadOnlyList<bool> Resolved(RackPushBackSystemWindow w, PushBackSide side)
        {
            var system = w.LastComputation?.System;
            var structure = system?.Structure;
            var postCount = Math.Max(2, w.CompositeState.SlotCount + 1);
            var selection = RackCad.Application.Systems.Selective.SelectiveSafetyFamilies.SelectedOfType(
                structure?.SafetySelections, w.Session.Catalog?.SafetyElements, SelectiveSafetyDefaults.DefensaType);
            return Enumerable.Range(0, postCount)
                .Select(post =>
                {
                    if (selection == null || !PushBackDefenseSides.HasFace(structure, post, side))
                    {
                        return false;
                    }

                    var setting = RackCad.Application.Systems.Dynamic.DynamicForkliftDefensePlan.ForSelection(
                        selection, post, postCount);
                    return PushBackDefenseSides.Resolved(setting, side) > 0.0;
                })
                .ToList();
        }

        // ==================== las superficies ====================

        [Fact]
        public void SafetyWindow_HasDefenseSectionForSideA()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    Through(w);
                    Assert.NotNull(w.DefenseSectionForTest);
                    Assert.Equal(PushBackSide.A, w.DefenseSectionForTest.Side);
                    Assert.Equal("Defensa de montacargas — lado A",
                        PushBackDefenseSection.Heading(w.DefenseSectionForTest.SideLabel));
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void SafetyWindow_HasDefenseSectionForSideB()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    Through(w);
                    Assert.NotNull(w.DefenseSectionBForTest);
                    Assert.Equal(PushBackSide.B, w.DefenseSectionBForTest.Side);
                    Assert.Equal("Defensa de montacargas — lado B",
                        PushBackDefenseSection.Heading(w.DefenseSectionBForTest.SideLabel));
                    Assert.NotSame(w.DefenseSectionForTest.Posts, w.DefenseSectionBForTest.Posts);
                }
                finally { w.Close(); }
            });
        }

        /// <summary>Un rack de un solo sentido ofrece UNA seccion, sin ruido de A/B — como los topes.</summary>
        [Fact]
        public void SafetyWindow_SingleSideHasSingleDefenseSection()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    Through(w);
                    Assert.NotNull(w.DefenseSectionForTest);
                    Assert.Null(w.DefenseSectionBForTest);
                    Assert.Null(w.DefenseSectionForTest.SideLabel);

                    // Y conserva la rejilla historica de los dos extremos: PB-009 dejo el posterior apagado por
                    // defecto, no prohibido, y esta ronda no viene a quitar esa capacidad.
                    Assert.True(w.DefenseSectionForTest.OwnsBothEnds);
                    Assert.Null(w.DefenseSectionForTest.Face());
                }
                finally { w.Close(); }
            });
        }

        /// <summary>La ventana principal deja de participar: desde A o desde B, exactamente lo mismo.</summary>
        [Fact]
        public void OpeningSafetyFromActiveSideAAndB_IsEquivalent()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    // Un estado con patrones opuestos, para que la comparacion tenga algo que perder.
                    Through(w, Set((0, true), (1, false)), Set((0, false), (1, true)));

                    w.CompositeState.SetActiveSide(PushBackSide.A);
                    Through(w);
                    var headingsA = (w.DefenseSectionForTest.SideLabel, w.DefenseSectionBForTest.SideLabel);
                    var drawnA = Defenses(w);
                    var resolvedA = (Resolved(w, PushBackSide.A), Resolved(w, PushBackSide.B));

                    w.CompositeState.SetActiveSide(PushBackSide.B);
                    Through(w);
                    var headingsB = (w.DefenseSectionForTest.SideLabel, w.DefenseSectionBForTest.SideLabel);

                    Assert.Equal(headingsA, headingsB);
                    Assert.Equal(drawnA, Defenses(w));
                    Assert.Equal(resolvedA.Item1, Resolved(w, PushBackSide.A));
                    Assert.Equal(resolvedA.Item2, Resolved(w, PushBackSide.B));
                }
                finally { w.Close(); }
            });
        }

        // ==================== identidad ====================

        /// <summary>A/Pn y B/Pn comparten la linea y NO son la misma intencion.</summary>
        [Fact]
        public void DefenseIntent_IdentityIncludesSideAndPhysicalPost()
        {
            var record = new SafetyPostDefense { PostIndex = 0, ExitAuto = true, EntranceAuto = true };
            PushBackDefenseSides.Set(record, PushBackSide.A, 36.0, auto: false);

            Assert.Equal(36.0, PushBackDefenseSides.LengthOf(record, PushBackSide.A), 9);
            Assert.False(PushBackDefenseSides.AutoOf(record, PushBackSide.A));
            Assert.True(PushBackDefenseSides.AutoOf(record, PushBackSide.B));   // el otro lado, intacto

            PushBackDefenseSides.Set(record, PushBackSide.B, 0.0, auto: false);
            Assert.Equal(36.0, PushBackDefenseSides.LengthOf(record, PushBackSide.A), 9);
            Assert.Equal(0.0, PushBackDefenseSides.LengthOf(record, PushBackSide.B), 9);
        }

        /// <summary>La rejilla recibe el lado EXPLICITAMENTE: nombre, extremo y donde existe esa cara.</summary>
        [Fact]
        public void DefenseGrid_ReceivesExplicitSide()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    var seen = new List<(PushBackSide side, string label, bool farEnd)>();
                    Through(w, inspect: section =>
                    {
                        var face = section.Face();
                        seen.Add((section.Side, face.Label, face.IsFarEnd));
                    });

                    Assert.Equal(2, seen.Count);
                    Assert.Equal((PushBackSide.A, "Lado A", false), seen[0]);
                    Assert.Equal((PushBackSide.B, "Lado B", true), seen[1]);
                }
                finally { w.Close(); }
            });
        }

        /// <summary>Y la rejilla de cada lado muestra el estado de SU lado, no un resumen de los dos.</summary>
        [Fact]
        public void DefenseGrid_DisplaysStateForItsOwnSide()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    Through(w, Set((1, false)), null);   // A/P1 apagado, B intacto

                    bool? shownA = null;
                    bool? shownB = null;
                    Through(w,
                        (_, grid) => shownA = grid.ExitCheckForTest(1).IsChecked,
                        (_, grid) => shownB = grid.ExitCheckForTest(1).IsChecked);

                    Assert.False(shownA);
                    Assert.True(shownB);   // la cara lejana SI lleva defensa, y su rejilla lo dice
                }
                finally { w.Close(); }
            });
        }

        // ==================== independencia ====================

        [Fact]
        public void DefenseA_Post1DoesNotChangeDefenseB_Post1()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    Through(w);
                    var before = Resolved(w, PushBackSide.B);
                    Through(w, Set((1, false)), null);

                    Assert.False(Resolved(w, PushBackSide.A)[1]);
                    Assert.Equal(before, Resolved(w, PushBackSide.B));
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void DefenseB_Post1DoesNotChangeDefenseA_Post1()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    Through(w);
                    var before = Resolved(w, PushBackSide.A);
                    Through(w, null, Set((1, false)));

                    Assert.False(Resolved(w, PushBackSide.B)[1]);
                    Assert.Equal(before, Resolved(w, PushBackSide.A));
                }
                finally { w.Close(); }
            });
        }

        /// <summary>Dentro de un mismo lado los postes siguen siendo independientes: la granularidad no cambia.</summary>
        [Fact]
        public void DefenseSameSide_PostsRemainIndependent()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    Through(w, Set((0, true), (1, false), (2, true), (3, false)), null);
                    Assert.Equal(new[] { true, false, true, false }, Resolved(w, PushBackSide.A));
                }
                finally { w.Close(); }
            });
        }

        /// <summary>Las seis decisiones del contrato del dueño, opuestas entre lados, se cumplen exactamente.</summary>
        [Fact]
        public void DefenseAAndB_CanHaveOppositePatterns()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    Through(w,
                        Set((0, true), (1, false), (2, true), (3, false)),
                        Set((0, false), (1, true), (2, false), (3, true)));

                    Assert.Equal(new[] { true, false, true, false }, Resolved(w, PushBackSide.A));
                    Assert.Equal(new[] { false, true, false, true }, Resolved(w, PushBackSide.B));
                }
                finally { w.Close(); }
            });
        }

        // ==================== blanks ====================

        /// <summary>Un frente en blanco en A quita la cara de ataque de A y no toca la de B.</summary>
        [Fact]
        public void DefenseBlankA_DoesNotAffectB()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    Through(w);
                    var before = Resolved(w, PushBackSide.B);

                    w.CompositeState.SetSlotPresent(PushBackSide.A, 0, false);
                    Settle(w);
                    Through(w);

                    Assert.False(Resolved(w, PushBackSide.A)[0]);   // A pierde su cara en la linea 0
                    Assert.Equal(before, Resolved(w, PushBackSide.B));
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void DefenseBlankB_DoesNotAffectA()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    Through(w);
                    var before = Resolved(w, PushBackSide.A);

                    w.CompositeState.SetSlotPresent(PushBackSide.B, 0, false);
                    Settle(w);
                    Through(w);

                    Assert.Equal(before, Resolved(w, PushBackSide.A));
                }
                finally { w.Close(); }
            });
        }

        /// <summary>El blanco quita APLICABILIDAD; no mueve la intencion a otra linea ni al otro lado.</summary>
        [Fact]
        public void DefenseBlank_DoesNotRelocate()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    Through(w, Set((1, false)), null);   // A/P1 apagado a mano
                    w.CompositeState.SetSlotPresent(PushBackSide.A, 0, false);
                    Settle(w);
                    Through(w);

                    var record = Defense(w).DefensaPosts.Single(item => item.PostIndex == 1);
                    Assert.Equal(0.0, PushBackDefenseSides.LengthOf(record, PushBackSide.A), 9);
                    Assert.False(PushBackDefenseSides.AutoOf(record, PushBackSide.A));
                    Assert.True(Resolved(w, PushBackSide.B)[1]);   // B sigue protegida en esa linea
                }
                finally { w.Close(); }
            });
        }

        // ==================== transaccional ====================

        [Fact]
        public void DefenseAccept_CommitsBothSections()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    Through(w, Set((0, false)), Set((1, false)));

                    Assert.False(Resolved(w, PushBackSide.A)[0]);
                    Assert.False(Resolved(w, PushBackSide.B)[1]);
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void DefenseCancel_CommitsNeitherSection()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    Through(w);
                    var a = Resolved(w, PushBackSide.A);
                    var b = Resolved(w, PushBackSide.B);
                    var drawn = Defenses(w);

                    Through(w, Set((0, false), (1, false)), Set((2, false), (3, false)), accept: false);

                    Assert.Equal(a, Resolved(w, PushBackSide.A));
                    Assert.Equal(b, Resolved(w, PushBackSide.B));
                    Assert.Equal(drawn, Defenses(w));
                }
                finally { w.Close(); }
            });
        }

        // ==================== persistencia y reconciliacion ====================

        [Fact]
        public void DefenseSidePostState_RackEditarRoundTrips()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    Through(w,
                        Set((0, true), (1, false), (2, true), (3, false)),
                        Set((0, false), (1, true), (2, false), (3, true)));

                    var before = Defense(w).DefensaPosts
                        .OrderBy(record => record.PostIndex)
                        .Select(record => (record.PostIndex, record.ExitLength, record.ExitAuto,
                            record.EntranceLength, record.EntranceAuto))
                        .ToList();
                    var drawn = Defenses(w);
                    Assert.NotEmpty(before);

                    var restored = PushBackDesignDocument.FromDomain(w.LastComputation.Design).ToDomain();
                    var selection = restored.Structure.SafetySelections.First(item => item.DefensaPosts.Count > 0);
                    var after = selection.DefensaPosts
                        .OrderBy(record => record.PostIndex)
                        .Select(record => (record.PostIndex, record.ExitLength, record.ExitAuto,
                            record.EntranceLength, record.EntranceAuto))
                        .ToList();

                    // Campo por campo: ni A se copia a B, ni B a A, ni se pierde ninguna decision.
                    Assert.Equal(before, after);
                    Assert.Equal(drawn, Defenses(w));
                }
                finally { w.Close(); }
            });
        }

        /// <summary>
        /// Al cambiar el numero de lineas: las que conservan identidad conservan su intencion POR LADO, y una que
        /// deja de existir no deja fantasma. A/P2 no se reconcilia nunca como B/P2.
        /// </summary>
        [Fact]
        public void DefenseSidePostState_ReconcilesByStableIdentity()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    Through(w, Set((1, false)), Set((3, false)));

                    w.CompositeState.SetSlotCount(2);   // 3 lineas: la cuarta deja de existir
                    Settle(w);
                    Through(w);

                    Assert.False(Resolved(w, PushBackSide.A)[1]);          // la linea 1 conserva SU intencion en A
                    Assert.True(Resolved(w, PushBackSide.B)[1]);           // y B no la heredo
                    Assert.Equal(3, Resolved(w, PushBackSide.A).Count);    // el rack tiene tres lineas
                    Assert.DoesNotContain(Defense(w).DefensaPosts, record => record.PostIndex >= 3);   // sin fantasma
                    Assert.Equal(new[] { true, true, true }, Resolved(w, PushBackSide.B));   // la linea nueva, al defecto
                }
                finally { w.Close(); }
            });
        }

        // ==================== dibujo == BOM ====================

        [Fact]
        public void DefenseDraw_EqualsBom()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    Through(w,
                        Set((0, true), (1, false), (2, true), (3, false)),
                        Set((0, false), (1, true), (2, false), (3, true)));

                    Assert.Equal(Defenses(w).Count, DefensesInBom(w));
                    Assert.Equal(4, Defenses(w).Count);   // dos lineas por lado, y ni una mas
                }
                finally { w.Close(); }
            });
        }

        /// <summary>
        /// Un rack de un solo sentido —el caso legacy— dibuja igual que antes de esta ronda: nadie ha decidido nada
        /// por lado, la seccion no cambia el modelo y el automatico 12"/36" del extremo bajo sigue mandando.
        /// </summary>
        [Fact]
        public void LegacyDefense_OutputUnchanged()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    var before = Defenses(w);
                    Through(w);   // abrir y aceptar sin tocar nada

                    Assert.Equal(before, Defenses(w));
                    Assert.Empty(Defense(w).DefensaPosts);
                    Assert.Equal(before.Count, DefensesInBom(w));
                }
                finally { w.Close(); }
            });
        }

        // ==================== regresion de la ronda 7C ====================

        /// <summary>Los topes siguen teniendo su blanco acotado al lado.</summary>
        [Fact]
        public void RearTope_BlankA_DoesNotBlankB()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    w.CompositeState.SetSlotPresent(PushBackSide.A, 1, false);
                    var seen = new List<int[]>();
                    w.RearTopeDialog = (_, levels) => { seen.Add(levels.ToArray()); return null; };
                    w.SafetyDialog = _ =>
                    {
                        w.RearTopeSectionForTest.Configure();
                        w.RearTopeSectionBForTest.Configure();
                        return null;
                    };
                    EditorWindowTestSupport.ClickNamed(w, "SafetyButton");

                    Assert.Equal(0, seen[0][1]);
                    Assert.NotEqual(0, seen[1][1]);
                }
                finally { w.Close(); }
            });
        }

        /// <summary>Y «Ninguno» sigue existiendo y persistiendo.</summary>
        [Fact]
        public void RearTope_None_Persists()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    w.SafetyDialog = selections =>
                    {
                        w.RearTopeSectionForTest.PieceBox.SelectedId = PushBackRearTopeConfig.NonePieceId;
                        return selections.ToList();
                    };
                    EditorWindowTestSupport.ClickNamed(w, "SafetyButton");
                    w.SafetyDialog = null;

                    var restored = PushBackDesignDocument.FromDomain(w.LastComputation.Design).ToDomain();
                    Assert.True(restored.RearTope.IsNone);
                    Assert.False(restored.SideB.RearTope.IsNone);
                }
                finally { w.Close(); }
            });
        }
    }
}
