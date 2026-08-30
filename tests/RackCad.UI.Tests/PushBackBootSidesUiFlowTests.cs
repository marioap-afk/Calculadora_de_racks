using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.Application.Catalogs;
using RackCad.UI;
using RackCad.UI.Controls;
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

                // Un rack COMPUESTO de verdad: el lado B declarado Y con almacenamiento en cada frente. Sin esto el
                // lado B no existe fisicamente y no puede llevar botas, por mucho que su seccion se ofrezca.
                var matrix = w.CompositeState.Of(PushBackSide.B).Structure;
                for (var front = 0; front < Math.Min(w.CompositeState.SlotCount, matrix.Count); front++)
                {
                    matrix.Fronts[front].IsActive = true;
                }

                w.State.SetFrontCount(fronts);   // el modelo cambio por fuera: se pide recalcular
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
            bool accept = true,
            Action<SelectiveSafetyWindow> dialogCheck = null)
        {
            w.SafetyWindowDialog = dialog =>
            {
                dialog.WindowStartupLocation = WindowStartupLocation.Manual;
                dialog.ShowInTaskbar = false;
                dialog.Left = -10000;
                dialog.Top = -10000;
                dialog.Show();
                dialog.UpdateLayout();

                dialogCheck?.Invoke(dialog);
                gesture?.Invoke(w.BootSectionForTest, w.BootSectionBForTest);
                dialog.BuildResultForTest();
                dialog.Close();
                return accept;
            };
            EditorWindowTestSupport.ClickNamed(w, "SafetyButton");
            w.SafetyWindowDialog = null;
        }

        /// <summary>Los ids de las variantes de BOTA del catalogo.</summary>
        private static IReadOnlyList<string> BootVariants(RackPushBackSystemWindow w)
            => w.Session.Catalog.SafetyElements
                .Where(entry => SelectiveSafetyDefaults.IsType(entry.Type, SelectiveSafetyDefaults.BotaType))
                .Select(entry => entry.Id)
                .OrderBy(id => id, StringComparer.Ordinal)
                .ToList();

        private static int Of(RackPushBackSystemWindow w, PushBackSide side)
            => PushBackBootPlan.Resolve(w.LastComputation.System, w.Session.Catalog).Count(boot => boot.Side == side);

        private static IReadOnlyList<ResolvedBoot> Physical(RackPushBackSystemWindow w)
            => PushBackBootPlan.Resolve(w.LastComputation.System, w.Session.Catalog);

        // ==================================================================== §30 la superficie por lado

        /// <summary>Un rack compuesto ofrece la seccion del lado A…</summary>
        [Fact]
        public void PushBackComposite_ShowsBootSectionA()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown(composite: true);
                try
                {
                    Through(w, (a, _) =>
                    {
                        Assert.NotNull(a);
                        Assert.Equal(PushBackSide.A, a.Side);
                        Assert.Equal("A", a.SideLabel);
                    }, accept: false);
                }
                finally { w.Close(); }
            });
        }

        /// <summary>…y la del lado B, con sus propios controles.</summary>
        [Fact]
        public void PushBackComposite_ShowsBootSectionB()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown(composite: true);
                try
                {
                    Through(w, (a, b) =>
                    {
                        Assert.NotNull(b);
                        Assert.Equal(PushBackSide.B, b.Side);
                        Assert.NotSame(a.PieceBox, b.PieceBox);
                        Assert.NotSame(a.ModeBox, b.ModeBox);
                        Assert.NotSame(a.Button, b.Button);
                    }, accept: false);
                }
                finally { w.Close(); }
            });
        }

        /// <summary>Y NO existe una tercera fila global de botas: seria una autoridad por encima de los dos lados.</summary>
        [Fact]
        public void PushBackComposite_DoesNotShowGenericBootRow()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown(composite: true);
                try
                {
                    Through(w, (_, __) => { }, accept: false, dialogCheck: dialog =>
                        Assert.Null(dialog.BootVariantComboForTest));
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void PushBackSimple_ShowsSingleBootSection()
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
                    }, accept: false);
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void PushBackSimple_DoesNotShowGenericBootRow()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown(composite: false);
                try
                {
                    Through(w, (_, __) => { }, accept: false, dialogCheck: dialog =>
                        Assert.Null(dialog.BootVariantComboForTest));
                }
                finally { w.Close(); }
            });
        }

        /// <summary>Cada seccion es autosuficiente: tipo, ubicacion y postes.</summary>
        [Fact]
        public void BootSectionA_HasTypePlacementAndPerPost()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown(composite: true);
                try
                {
                    Through(w, (a, _) =>
                    {
                        Assert.NotNull(a.PieceBox);
                        Assert.NotNull(a.ModeBox);
                        Assert.NotNull(a.Button);
                        Assert.Contains(
                            PushBackDefaults.NonePieceId,
                            a.PieceBox.Items.OfType<CatalogOption>().Select(option => option.Id));
                        Assert.NotEqual(PushBackDefaults.NonePieceId, a.PieceBox.SelectedId);
                    }, accept: false);
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void BootSectionB_HasTypePlacementAndPerPost()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown(composite: true);
                try
                {
                    Through(w, (_, b) =>
                    {
                        Assert.NotNull(b.PieceBox);
                        Assert.NotNull(b.ModeBox);
                        Assert.NotNull(b.Button);
                        Assert.Contains(
                            PushBackDefaults.NonePieceId,
                            b.PieceBox.Items.OfType<CatalogOption>().Select(option => option.Id));
                    }, accept: false);
                }
                finally { w.Close(); }
            });
        }

        /// <summary>Cambiar el TIPO de A no toca nada de B…</summary>
        [Fact]
        public void ChangingBootTypeA_DoesNotChangeB()
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
                    var farBefore = Of(w, PushBackSide.B);

                    Through(w, (a, _) => a.PieceBox.SelectedId = PushBackDefaults.NonePieceId);

                    Assert.Equal(0, Of(w, PushBackSide.A));
                    Assert.Equal(farBefore, Of(w, PushBackSide.B));
                }
                finally { w.Close(); }
            });
        }

        /// <summary>…y al reves.</summary>
        [Fact]
        public void ChangingBootTypeB_DoesNotChangeA()
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
                    var nearBefore = Of(w, PushBackSide.A);

                    Through(w, (_, b) => b.PieceBox.SelectedId = PushBackDefaults.NonePieceId);

                    Assert.Equal(0, Of(w, PushBackSide.B));
                    Assert.Equal(nearBefore, Of(w, PushBackSide.A));
                }
                finally { w.Close(); }
            });
        }

        // ==================================================================== §24 el recorrido completo

        /// <summary>
        /// EL RECORRIDO DEL DUEÑO: A con pieza y entrada/salida, B sin pieza pero con su posterior ya elegida.
        /// Solo dibuja A. Al dar pieza a B aparece SU posterior sin volver a configurarla; al quitarsela a A
        /// desaparece la de A y B queda intacto; y al devolversela, A vuelve como estaba.
        /// </summary>
        [Fact]
        public void DormantSide_ComesBackExactlyWhenItsTypeReturns()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown(composite: true);
                try
                {
                    var piece = BootVariants(w)[0];

                    Through(w, (a, b) =>
                    {
                        a.PieceBox.SelectedId = piece;
                        a.ModeBox.SelectedIndex = (int)BootPlacement.EntryExit;
                        b.PieceBox.SelectedId = PushBackDefaults.NonePieceId;
                        b.ModeBox.SelectedIndex = (int)BootPlacement.Rear;
                    });

                    var onlyA = Of(w, PushBackSide.A);
                    Assert.True(onlyA > 0);
                    Assert.Equal(0, Of(w, PushBackSide.B));

                    // Solo el TIPO de B: su posterior estaba dormida y vuelve sola.
                    Through(w, (_, b) => b.PieceBox.SelectedId = piece);
                    Assert.Equal(onlyA, Of(w, PushBackSide.A));
                    var rearB = Physical(w).Where(boot => boot.Side == PushBackSide.B).ToList();
                    Assert.NotEmpty(rearB);
                    Assert.All(rearB, boot => Assert.Equal(BootFace.Rear, boot.Face));

                    // Ahora se apaga A: B no se entera.
                    Through(w, (a, _) => a.PieceBox.SelectedId = PushBackDefaults.NonePieceId);
                    Assert.Equal(0, Of(w, PushBackSide.A));
                    Assert.Equal(rearB.Count, Of(w, PushBackSide.B));

                    // Y al devolverle la pieza, A recupera su entrada/salida sin reconfigurarla.
                    Through(w, (a, _) => a.PieceBox.SelectedId = piece);
                    Assert.Equal(onlyA, Of(w, PushBackSide.A));
                    Assert.All(
                        Physical(w).Where(boot => boot.Side == PushBackSide.A),
                        boot => Assert.Equal(BootFace.EntryExit, boot.Face));
                }
                finally { w.Close(); }
            });
        }

        /// <summary>Y los postes configurados con el tipo en «Ninguno» tambien vuelven.</summary>
        [Fact]
        public void DormantPerPost_ComesBackWhenTheTypeReturns()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown(composite: true);
                try
                {
                    var piece = BootVariants(w)[0];

                    Through(w, (a, b) =>
                    {
                        a.PieceBox.SelectedId = PushBackDefaults.NonePieceId;
                        a.ModeBox.SelectedIndex = (int)BootPlacement.None;
                        b.PieceBox.SelectedId = PushBackDefaults.NonePieceId;
                        b.ModeBox.SelectedIndex = (int)BootPlacement.None;

                        w.BootPerPostWindowDialog = window =>
                        {
                            window.SetForTest(2, BootPlacement.Rear);
                            window.BuildResultForTest();
                            return true;
                        };
                        a.Button.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                        w.BootPerPostWindowDialog = null;
                    });

                    Assert.Empty(Physical(w));   // sin pieza no materializa…

                    Through(w, (a, _) => a.PieceBox.SelectedId = piece);

                    var boots = Physical(w);     // …y la intencion seguia ahi
                    Assert.Single(boots);
                    Assert.Equal(2, boots[0].PostIndex);
                    Assert.Equal(BootFace.Rear, boots[0].Face);
                    Assert.Equal(PushBackSide.A, boots[0].Side);
                }
                finally { w.Close(); }
            });
        }

        // ==================================================================== §31 los otros sistemas

        /// <summary>El Selectivo conserva su fila generica de botas: es su superficie historica.</summary>
        [Fact]
        public void Selective_GenericBootRowRegression()
            => AssertGenericBootRowSurvives();

        /// <summary>Y el Dinamico igual — la fila solo desaparece donde su lugar lo ocupan las secciones por lado.</summary>
        [Fact]
        public void Dynamic_GenericBootRowRegression()
            => AssertGenericBootRowSurvives();

        private static void AssertGenericBootRowSurvives()
        {
            StaTestRunner.Run(() =>
            {
                var catalog = RackCad.Application.Catalogs.JsonRackCatalogProvider.FromBaseDirectory().Load();
                var dialog = new SelectiveSafetyWindow(
                    catalog.SafetyElements,
                    new List<SelectiveSafetySelection>(),
                    postCount: 3,
                    catalog: catalog)
                {
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    ShowInTaskbar = false,
                    Left = -10000,
                    Top = -10000,
                };
                try
                {
                    dialog.Show();
                    dialog.UpdateLayout();

                    Assert.NotNull(dialog.BootVariantComboForTest);
                }
                finally { dialog.Close(); }
            });
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
