using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RackCad.Application.Catalogs;
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
    /// I-42 (S1E) — LOS PROTECTORES DE BOTA POR LA RUTA REAL: la ventana «Elementos de seguridad», la SECCION DE
    /// SU LADO, su editor por poste, aceptar, resolver, dibujo y BOM.
    ///
    /// <para>
    /// El contrato del dueño en la superficie que el usuario tiene delante: <b>Ninguno · Entrada/Salida · Posterior
    /// · Ambas</b>, y en el editor por poste ademas <b>(por defecto)</b>. Ni «Izquierda» ni «Derecha» aparecen ya
    /// para esta familia — nombraban una orientacion, no la ubicacion que hay que proteger.
    /// </para>
    /// <para>
    /// Desde S1E cada LADO tiene su seccion, con su general y sus postes. Estas pruebas recorren la del lado A —la
    /// unica de un rack de un solo sentido— y <see cref="PushBackBootSidesUiFlowTests"/> recorre las dos.
    /// </para>
    /// </summary>
    public sealed class PushBackBootUiFlowTests
    {
        /// <summary>
        /// Un rack con postes INTERIORES. El protector lateral ocupa los de orilla y sustituye alli la bota
        /// —comportamiento historico que esta ronda no toca—, asi que las botas se observan en los interiores.
        /// </summary>
        private static RackPushBackSystemWindow Shown(int fronts = 3)
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
            return w;
        }

        /// <summary>Los ids de catalogo de la familia BOTA: la fila puede ofrecer varias variantes.</summary>
        private static IReadOnlyList<string> BootIds(RackPushBackSystemWindow w)
            => w.Session.Catalog.SafetyElements
                .Where(entry => SelectiveSafetyDefaults.IsType(entry.Type, SelectiveSafetyDefaults.BotaType))
                .Select(entry => entry.Id)
                .ToList();

        private static bool IsBoot(RackPushBackSystemWindow w, string pieceId)
            => BootIds(w).Any(id => string.Equals(id, pieceId, StringComparison.OrdinalIgnoreCase));

        /// <summary>Las botas que el dibujo materializa, como «X|Y».</summary>
        private static IReadOnlyList<string> Boots(RackPushBackSystemWindow w)
        {
            var system = w.LastComputation?.System;
            if (system == null)
            {
                return new List<string>();
            }

            return new PushBackSystemPlantaBuilder().BuildPlan(system, w.Session.Catalog).Flatten().Instances
                .Where(instance => IsBoot(w, instance.PieceId))
                .Select(instance => FormattableString.Invariant(
                    $"{instance.Insertion.X:0.##}|{instance.Insertion.Y:0.##}"))
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToList();
        }

        private static int BootsInBom(RackPushBackSystemWindow w)
        {
            var system = w.LastComputation?.System;
            if (system == null)
            {
                return 0;
            }

            return PushBackBomBuilder.Build(system, w.Session.Catalog).Lines
                .Where(line => IsBoot(w, line.ProfileId))
                .Sum(line => line.Quantity);
        }

        /// <summary>
        /// Recorre la ventana REAL de seguridad: encuentra la fila de la bota, aplica <paramref name="gesture"/>
        /// sobre sus controles reales y acepta. Solo se sustituye el <c>ShowDialog</c>.
        /// </summary>
        private static void Through(
            RackPushBackSystemWindow w,
            Action<ComboBox> general = null,
            Action<SafetyPerPostWindow> perPost = null,
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

                // La familia de la bota puede ofrecer variantes: se elige una, que es lo que hace el usuario antes
                // de decidir donde va.
                var variant = dialog.BootVariantComboForTest;
                if (variant != null && variant.SelectedIndex <= 0 && variant.Items.Count > 1)
                {
                    variant.SelectedIndex = 1;
                }

                // I-42 (S1E) — la ubicacion se elige en la SECCION del lado, no en la fila: la fila solo aporta
                // el tipo. La seccion vive en la ventana anfitriona, ya construida cuando el dialogo se muestra.
                general?.Invoke(w.BootSectionForTest.ModeBox);
                if (perPost != null)
                {
                    w.BootPerPostWindowDialog = window =>
                    {
                        perPost(window);
                        window.BuildResultForTest();
                        return true;
                    };
                    w.BootSectionForTest.Button.RaiseEvent(
                        new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                    w.BootPerPostWindowDialog = null;
                }

                dialog.BuildResultForTest();
                dialog.Close();
                return accept;
            };
            EditorWindowTestSupport.ClickNamed(w, "SafetyButton");
            w.SafetyWindowDialog = null;
        }

        // ==================================================================== la superficie

        /// <summary>Las cuatro opciones se llaman por lo que son. «Izquierda» y «Derecha» ya no aparecen.</summary>
        [Fact]
        public void BootSelector_OffersThePhysicalLocations()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    IReadOnlyList<string> labels = null;
                    Through(w, general: combo => labels = combo.Items.OfType<string>().ToList(), accept: false);

                    Assert.Equal(new[] { "Ninguno", "Entrada/Salida", "Posterior", "Ambas" }, labels);
                }
                finally { w.Close(); }
            });
        }

        /// <summary>Y el editor POR POSTE ofrece las mismas cuatro, mas «(por defecto)».</summary>
        [Fact]
        public void BootPerPostEditor_OffersDefaultPlusTheFour()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    IReadOnlyList<string> labels = null;
                    Through(w, perPost: window => labels = window.OptionsForTest, accept: false);

                    Assert.Equal(
                        new[] { "(por defecto)", "Ninguno", "Entrada/Salida", "Posterior", "Ambas" },
                        labels);
                }
                finally { w.Close(); }
            });
        }

        // ==================================================================== las cuatro opciones, E2E

        [Fact]
        public void BootGeneral_EntryExit_DrawsAtTheOperatingFace()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    Through(w, general: combo => combo.SelectedIndex = (int)BootPlacement.EntryExit);
                    var middle = w.LastComputation.System.Structure.TotalLength / 2.0;

                    Assert.NotEmpty(Boots(w));
                    Assert.All(Boots(w), boot => Assert.True(double.Parse(boot.Split('|')[0]) < middle));
                    Assert.Equal(Boots(w).Count, BootsInBom(w));
                }
                finally { w.Close(); }
            });
        }

        /// <summary>Y «Posterior» dibuja detras — aunque por ahi no se cargue producto.</summary>
        [Fact]
        public void BootGeneral_Rear_DrawsAtTheRearFace()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    Through(w, general: combo => combo.SelectedIndex = (int)BootPlacement.Rear);
                    var middle = w.LastComputation.System.Structure.TotalLength / 2.0;

                    Assert.NotEmpty(Boots(w));
                    Assert.All(Boots(w), boot => Assert.True(double.Parse(boot.Split('|')[0]) > middle));
                    Assert.Equal(Boots(w).Count, BootsInBom(w));
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void BootGeneral_Both_DrawsTheUnion()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    Through(w, general: combo => combo.SelectedIndex = (int)BootPlacement.EntryExit);
                    var entry = Boots(w).ToList();
                    Through(w, general: combo => combo.SelectedIndex = (int)BootPlacement.Rear);
                    var rear = Boots(w).ToList();
                    Through(w, general: combo => combo.SelectedIndex = (int)BootPlacement.Both);

                    Assert.Equal(
                        entry.Concat(rear).OrderBy(value => value, StringComparer.Ordinal).ToList(),
                        Boots(w));
                    Assert.Equal(Boots(w).Count, BootsInBom(w));
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void BootGeneral_None_DrawsNothing()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    Through(w, general: combo => combo.SelectedIndex = (int)BootPlacement.None);

                    Assert.Empty(Boots(w));
                    Assert.Equal(0, BootsInBom(w));
                }
                finally { w.Close(); }
            });
        }

        // ==================================================================== por poste, E2E

        /// <summary>El patron del dueño, por la ruta real: por defecto · Posterior · Ambas · Ninguno.</summary>
        [Fact]
        public void BootPerPost_TheOwnerPattern_MaterializesExactly()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    Through(
                        w,
                        general: combo => combo.SelectedIndex = (int)BootPlacement.EntryExit,
                        perPost: window =>
                        {
                            window.SetForTest(1, BootPlacement.Rear);
                            window.SetForTest(2, BootPlacement.Both);
                            window.SetForTest(3, BootPlacement.None);
                        });

                    var middle = w.LastComputation.System.Structure.TotalLength / 2.0;
                    var byLine = Boots(w)
                        .GroupBy(boot => boot.Split('|')[1])
                        .ToDictionary(group => group.Key, group => group.ToList());
                    var lines = byLine.Keys.OrderBy(double.Parse).ToList();

                    // Los postes de orilla los ocupa el protector lateral; los INTERIORES son 2 y 3, que es donde
                    // se ven «Posterior» y «Ambas». El poste 4 pidio Ninguno y no aparece.
                    Assert.Equal(2, lines.Count);
                    Assert.Single(byLine[lines[0]]);
                    Assert.True(double.Parse(byLine[lines[0]][0].Split('|')[0]) > middle, "el poste 2 pidio Posterior");
                    Assert.Equal(2, byLine[lines[1]].Count);   // el poste 3 pidio Ambas
                    Assert.Equal(Boots(w).Count, BootsInBom(w));
                }
                finally { w.Close(); }
            });
        }

        /// <summary>Cambiar la general mueve SOLO los postes «(por defecto)».</summary>
        [Fact]
        public void ChangingGeneral_OnlyMovesTheDefaultPosts()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    Through(
                        w,
                        general: combo => combo.SelectedIndex = (int)BootPlacement.EntryExit,
                        perPost: window => window.SetForTest(1, BootPlacement.Rear));
                    var before = Boots(w).ToList();
                    var middle = w.LastComputation.System.Structure.TotalLength / 2.0;
                    var pinned = before.Where(boot => double.Parse(boot.Split('|')[0]) > middle).ToList();
                    Assert.Single(pinned);

                    Through(w, general: combo => combo.SelectedIndex = (int)BootPlacement.Rear);

                    // El poste explicito sigue exactamente donde estaba; los demas siguieron a la general.
                    Assert.Contains(pinned[0], Boots(w));
                    Assert.All(Boots(w), boot => Assert.True(double.Parse(boot.Split('|')[0]) > middle));
                }
                finally { w.Close(); }
            });
        }

        // ==================================================================== transaccional y RACKEDITAR

        [Fact]
        public void BootSelection_CancelDoesNotPersist()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    Through(w, general: combo => combo.SelectedIndex = (int)BootPlacement.EntryExit);
                    var before = Boots(w).ToList();

                    Through(w, general: combo => combo.SelectedIndex = (int)BootPlacement.Both, accept: false);

                    Assert.Equal(before, Boots(w));
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void BootSelection_RackEditarRoundTrips()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    Through(
                        w,
                        general: combo => combo.SelectedIndex = (int)BootPlacement.Rear,
                        perPost: window => window.SetForTest(1, BootPlacement.Both));

                    var restored = PushBackDesignDocument
                        .FromDomain(w.LastComputation.Design).ToDomain();
                    var selection = restored.Structure.SafetySelections
                        .First(item => IsBoot(w, item.ElementId));

                    Assert.Equal(BootPlacement.Rear, selection.Bota.Placement);
                    Assert.Equal(BootPlacement.Both, selection.BootPlacementAt(1));
                    Assert.Equal(BootPlacement.Rear, selection.BootPlacementAt(0));
                }
                finally { w.Close(); }
            });
        }
    }
}
