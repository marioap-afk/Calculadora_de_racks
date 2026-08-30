using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.UI.Systems.PushBack;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-42 (S1E, contrato del dueño) — LAS DOS SECCIONES DE BOTAS POR LA RUTA REAL: «Elementos de seguridad» de un
    /// rack COMPUESTO ofrece una seccion por lado, cada una con su ubicacion general y su rejilla por poste.
    ///
    /// <para>
    /// Lo que estas pruebas afirman es la INDEPENDENCIA: elegir en A no toca B ni al reves, ni en la superficie ni
    /// en el dibujo ni en el BOM. Y que un rack de un solo sentido sigue teniendo UNA seccion, sin etiqueta: no se
    /// le inventa un lado que no existe.
    /// </para>
    /// </summary>
    public sealed class PushBackBootSidesUiFlowTests
    {
        private static RackPushBackSystemWindow Shown(bool composite, int fronts = 3)
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
            w.State.SetFrontCount(fronts);
            if (composite)
            {
                ((CheckBox)w.FindName("SideBPresentCheck")).IsChecked = true;
                w.UpdateLayout();
            }

            return w;
        }

        private static bool IsBoot(RackPushBackSystemWindow w, string pieceId)
            => w.Session.Catalog.SafetyElements.Any(entry =>
                SelectiveSafetyDefaults.IsType(entry.Type, SelectiveSafetyDefaults.BotaType)
                && string.Equals(entry.Id, pieceId, StringComparison.OrdinalIgnoreCase));

        private static IReadOnlyList<double> Boots(RackPushBackSystemWindow w)
        {
            var system = w.LastComputation?.System;
            if (system == null)
            {
                return new List<double>();
            }

            return new PushBackSystemPlantaBuilder().BuildPlan(system, w.Session.Catalog).Flatten().Instances
                .Where(instance => IsBoot(w, instance.PieceId))
                .Select(instance => Math.Round(instance.Insertion.X, 2))
                .OrderBy(value => value)
                .ToList();
        }

        private static int Near(RackPushBackSystemWindow w)
            => Boots(w).Count(x => x < w.LastComputation.System.Structure.TotalLength / 2.0);

        private static int Far(RackPushBackSystemWindow w)
            => Boots(w).Count(x => x > w.LastComputation.System.Structure.TotalLength / 2.0);

        private static int Bom(RackPushBackSystemWindow w)
            => PushBackBomBuilder.Build(w.LastComputation.System, w.Session.Catalog).Lines
                .Where(line => IsBoot(w, line.ProfileId))
                .Sum(line => line.Quantity);

        /// <summary>
        /// Recorre la ventana REAL de seguridad y aplica <paramref name="gesture"/> sobre las secciones de verdad.
        /// Solo se sustituye el <c>ShowDialog</c>.
        /// </summary>
        private static void Through(
            RackPushBackSystemWindow w,
            Action<PushBackBootSection, PushBackBootSection> gesture,
            bool accept = true)
        {
            w.SafetyWindowDialog = dialog =>
            {
                dialog.WindowStartupLocation = WindowStartupLocation.Manual;
                dialog.ShowInTaskbar = false;
                dialog.Left = -10000;
                dialog.Top = -10000;
                dialog.Show();
                dialog.UpdateLayout();

                var variant = dialog.BootVariantComboForTest;
                if (variant != null && variant.SelectedIndex <= 0 && variant.Items.Count > 1)
                {
                    variant.SelectedIndex = 1;
                }

                gesture?.Invoke(w.BootSectionForTest, w.BootSectionBForTest);
                dialog.BuildResultForTest();
                dialog.Close();
                return accept;
            };
            EditorWindowTestSupport.ClickNamed(w, "SafetyButton");
            w.SafetyWindowDialog = null;
        }

        // ==================================================================== la superficie

        /// <summary>Un rack de un solo sentido tiene UNA seccion, y sin etiqueta de lado.</summary>
        [Fact]
        public void SimpleRack_HasOneUnlabelledBootSection()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown(composite: false);
                try
                {
                    Through(w, (a, b) =>
                    {
                        Assert.NotNull(a);
                        Assert.Null(b);
                        Assert.Null(a.SideLabel);
                        Assert.Equal(PushBackBootSection.HeadingText, PushBackBootSection.Heading(a.SideLabel));
                    }, accept: false);
                }
                finally { w.Close(); }
            });
        }

        /// <summary>Y uno compuesto tiene dos, con su etiqueta y las cuatro ubicaciones cada una.</summary>
        [Fact]
        public void CompositeRack_HasOneBootSectionPerSide()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown(composite: true);
                try
                {
                    Through(w, (a, b) =>
                    {
                        Assert.Equal(PushBackSide.A, a.Side);
                        Assert.Equal(PushBackSide.B, b.Side);
                        Assert.Equal("A", a.SideLabel);
                        Assert.Equal("B", b.SideLabel);
                        Assert.Equal(
                            new[] { "Ninguno", "Entrada/Salida", "Posterior", "Ambas" },
                            a.ModeBox.Items.OfType<string>().ToList());
                        Assert.Equal(
                            a.ModeBox.Items.OfType<string>().ToList(),
                            b.ModeBox.Items.OfType<string>().ToList());
                    }, accept: false);
                }
                finally { w.Close(); }
            });
        }

        // ==================================================================== A y B, E2E

        /// <summary>A = Entrada/Salida y B = Ninguno: solo las botas del pasillo de A.</summary>
        [Fact]
        public void SideAEntry_SideBNone_DrawsOnlyTheAAisle()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown(composite: true);
                try
                {
                    Through(w, (a, b) =>
                    {
                        a.ModeBox.SelectedIndex = (int)BootPlacement.EntryExit;
                        b.ModeBox.SelectedIndex = (int)BootPlacement.None;
                    });

                    Assert.NotEmpty(Boots(w));
                    Assert.Equal(0, Far(w));
                    Assert.Equal(Boots(w).Count, Bom(w));
                }
                finally { w.Close(); }
            });
        }

        /// <summary>Y al reves: solo las del pasillo de B, que es el extremo opuesto.</summary>
        [Fact]
        public void SideANone_SideBEntry_DrawsOnlyTheBAisle()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown(composite: true);
                try
                {
                    Through(w, (a, b) =>
                    {
                        a.ModeBox.SelectedIndex = (int)BootPlacement.None;
                        b.ModeBox.SelectedIndex = (int)BootPlacement.EntryExit;
                    });

                    Assert.NotEmpty(Boots(w));
                    Assert.Equal(0, Near(w));
                    Assert.Equal(Boots(w).Count, Bom(w));
                }
                finally { w.Close(); }
            });
        }

        /// <summary>Con los dos en entrada/salida, los dos pasillos llevan las suyas.</summary>
        [Fact]
        public void BothSidesEntry_DrawTheTwoAisles()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown(composite: true);
                try
                {
                    Through(w, (a, b) =>
                    {
                        a.ModeBox.SelectedIndex = (int)BootPlacement.EntryExit;
                        b.ModeBox.SelectedIndex = (int)BootPlacement.EntryExit;
                    });

                    Assert.True(Near(w) > 0);
                    Assert.Equal(Near(w), Far(w));
                    Assert.Equal(Boots(w).Count, Bom(w));
                }
                finally { w.Close(); }
            });
        }

        /// <summary>Cambiar el lado A no mueve NINGUNA bota del lado B, ni al reves.</summary>
        [Fact]
        public void ChangingOneSide_LeavesTheOtherExactlyAsItWas()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown(composite: true);
                try
                {
                    Through(w, (a, b) =>
                    {
                        a.ModeBox.SelectedIndex = (int)BootPlacement.EntryExit;
                        b.ModeBox.SelectedIndex = (int)BootPlacement.EntryExit;
                    });
                    var farBefore = Far(w);
                    var nearBefore = Near(w);

                    Through(w, (a, _) => a.ModeBox.SelectedIndex = (int)BootPlacement.None);
                    Assert.Equal(farBefore, Far(w));
                    Assert.Equal(0, Near(w));

                    Through(w, (a, b) =>
                    {
                        a.ModeBox.SelectedIndex = (int)BootPlacement.EntryExit;
                        b.ModeBox.SelectedIndex = (int)BootPlacement.None;
                    });
                    Assert.Equal(nearBefore, Near(w));
                    Assert.Equal(0, Far(w));
                }
                finally { w.Close(); }
            });
        }

        /// <summary>Y la rejilla POR POSTE es la de SU lado: A/P2 y B/P2 son dos decisiones distintas.</summary>
        [Fact]
        public void PerPostEditor_BelongsToItsOwnSide()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown(composite: true);
                try
                {
                    Through(w, (a, b) =>
                    {
                        a.ModeBox.SelectedIndex = (int)BootPlacement.None;
                        b.ModeBox.SelectedIndex = (int)BootPlacement.None;

                        w.BootPerPostWindowDialog = window =>
                        {
                            window.SetForTest(2, BootPlacement.EntryExit);
                            window.BuildResultForTest();
                            return true;
                        };
                        a.Button.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));

                        w.BootPerPostWindowDialog = window =>
                        {
                            window.SetForTest(1, BootPlacement.EntryExit);
                            window.BuildResultForTest();
                            return true;
                        };
                        b.Button.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                        w.BootPerPostWindowDialog = null;

                        Assert.Equal(new[] { 2 }, a.Posts.Select(post => post.PostIndex).ToArray());
                        Assert.Equal(new[] { 1 }, b.Posts.Select(post => post.PostIndex).ToArray());
                    });

                    // Una por lado, en pasillos opuestos y en lineas distintas.
                    Assert.Equal(2, Boots(w).Count);
                    Assert.Equal(1, Near(w));
                    Assert.Equal(1, Far(w));
                    Assert.Equal(Boots(w).Count, Bom(w));
                }
                finally { w.Close(); }
            });
        }

        /// <summary>Cancelar no persiste nada: ni el lado A ni el B.</summary>
        [Fact]
        public void Cancel_DoesNotPersistEitherSide()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown(composite: true);
                try
                {
                    Through(w, (a, b) =>
                    {
                        a.ModeBox.SelectedIndex = (int)BootPlacement.EntryExit;
                        b.ModeBox.SelectedIndex = (int)BootPlacement.EntryExit;
                    });
                    var before = Boots(w);

                    Through(w, (a, b) =>
                    {
                        a.ModeBox.SelectedIndex = (int)BootPlacement.None;
                        b.ModeBox.SelectedIndex = (int)BootPlacement.None;
                    }, accept: false);

                    Assert.Equal(before, Boots(w));
                }
                finally { w.Close(); }
            });
        }

        /// <summary>Y RACKEDITAR devuelve las dos intenciones, distintas y exactas.</summary>
        [Fact]
        public void RackEditar_RoundTripsBothSides()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown(composite: true);
                try
                {
                    Through(w, (a, b) =>
                    {
                        a.ModeBox.SelectedIndex = (int)BootPlacement.EntryExit;
                        b.ModeBox.SelectedIndex = (int)BootPlacement.Rear;
                    });

                    Through(w, (a, b) =>
                    {
                        Assert.Equal(BootPlacement.EntryExit, a.Placement);
                        Assert.Equal(BootPlacement.Rear, b.Placement);

                        // El lado B eligio algo distinto de su automatico, y por eso queda registrado como decision
                        // propia. El lado A eligio justo lo que ya hacia: no hay nada que congelar, y sigue
                        // heredando su automatico — mostrar no es elegir.
                        Assert.True(b.Explicit);
                    }, accept: false);
                }
                finally { w.Close(); }
            });
        }
    }
}
