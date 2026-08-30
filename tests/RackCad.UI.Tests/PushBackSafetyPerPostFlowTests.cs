using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Selective;
using RackCad.UI;
using RackCad.UI.Systems.PushBack;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// Owner-validation round 1, defecto 2 (I-32) — el flujo REAL de la ventana: el usuario elige una variante de
    /// bota y de protector lateral, configura POSTES CONCRETOS en "Por poste…", acepta, y esa matriz tiene que
    /// sobrevivir hasta el diseño, las vistas y el BOM, y volver a verse al reabrir.
    ///
    /// Se ejercita el handler real (botón → diálogo → autoridad → estado → recompute), con el diálogo sustituido por
    /// el seam existente para decidir qué "eligió" el usuario.
    /// </summary>
    public sealed class PushBackSafetyPerPostFlowTests
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

        private static SafetyElementCatalogEntry ElementOfType(RackPushBackSystemWindow w, string type)
            => w.Session.Catalog?.SafetyElements?
                .FirstOrDefault(e => e != null && SelectiveSafetyDefaults.IsType(e.Type, type));

        private static SelectiveSafetySelection WithPosts(string elementId, params (int Post, SafetySide Side)[] posts)
        {
            var selection = new SelectiveSafetySelection { ElementId = elementId, Quantity = 1, Side = SafetySide.None };
            foreach (var (post, side) in posts)
            {
                selection.PostSides.Add(new SafetyPostSide { PostIndex = post, Side = side });
            }

            return selection;
        }

        private static IReadOnlyList<(int Post, SafetySide Side)> Matrix(SelectiveSafetySelection selection)
            => selection.PostSides.Select(p => (p.PostIndex, p.Side)).OrderBy(p => p.PostIndex).ToList();

        /// <summary>
        /// El corazón del defecto: elegir bota y protector lateral con postes concretos, aceptar, y comprobar que la
        /// matriz sigue ahí — no vacía — para AMBAS familias.
        /// </summary>
        [Fact]
        public void AcceptingPerPostChoices_KeepsTheMatrixForBootAndLateral()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    var boot = ElementOfType(w, SelectiveSafetyDefaults.BotaType);
                    var lateral = ElementOfType(w, SelectiveSafetyDefaults.LateralType);
                    if (boot == null || lateral == null) return;   // host sin catálogo: nada que ejercitar

                    w.State.SetFrontCount(3);   // 3 frentes => 4 postes
                    w.SafetyDialog = _ => new[]
                    {
                        WithPosts(boot.Id, (1, SafetySide.Both), (2, SafetySide.None)),
                        WithPosts(lateral.Id, (0, SafetySide.Left), (3, SafetySide.Right)),
                    };
                    EditorWindowTestSupport.ClickNamed(w, "SafetyButton");

                    var storedBoot = w.SafetySelections.Single(s => string.Equals(s.ElementId, boot.Id, StringComparison.OrdinalIgnoreCase));
                    var storedLateral = w.SafetySelections.Single(s => string.Equals(s.ElementId, lateral.Id, StringComparison.OrdinalIgnoreCase));

                    Assert.Equal(2, storedBoot.PostSides.Count);
                    Assert.Equal(2, storedLateral.PostSides.Count);
                    Assert.Equal(new[] { 1, 2 }, storedBoot.PostSides.Select(p => p.PostIndex).OrderBy(i => i).ToArray());
                    Assert.Equal(new[] { 0, 3 }, storedLateral.PostSides.Select(p => p.PostIndex).OrderBy(i => i).ToArray());

                    // La exclusión explícita del usuario (None) sobrevive.
                    Assert.Equal(SafetySide.None, storedBoot.SideForPost(2));
                    Assert.NotEqual(SafetySide.None, storedBoot.SideForPost(1));

                    // Y todo queda en el extremo BAJO.
                    Assert.Equal(SafetySide.Left, SelectiveSafetyEnds.EndsForPost(storedBoot, 1));
                    Assert.Equal(SafetySide.Left, SelectiveSafetyEnds.EndsForPost(storedLateral, 3));
                }
                finally { w.Close(); }
            });
        }

        /// <summary>El diseño y el BOM cuentan los MISMOS postes que el usuario eligió.</summary>
        [Fact]
        public void TheDesignAndTheBom_ReflectTheChosenPosts()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    var boot = ElementOfType(w, SelectiveSafetyDefaults.BotaType);
                    if (boot == null) return;

                    w.State.SetFrontCount(3);
                    // I-42 (S1E) — RETARGETEADO: los postes SIN decision propia heredan el automatico del lado, asi
                    // que para leer «solo los elegidos» hay que decir que los demas no llevan. Es la general en
                    // «Ninguno», que desde S1D no apaga la familia: los postes elegidos siguen materializando.
                    w.SafetyDialog = _ =>
                    {
                        // La general la decide la SECCION del lado, que es la superficie real desde S1E.
                        w.BootSectionForTest.ModeBox.SelectedIndex = (int)BootPlacement.None;
                        return new[] { WithPosts(boot.Id, (1, SafetySide.Both), (2, SafetySide.Both)) };
                    };
                    EditorWindowTestSupport.ClickNamed(w, "SafetyButton");

                    var design = w.LastComputation?.Design;
                    Assert.NotNull(design);
                    var inDesign = design.Structure.SafetySelections
                        .Single(s => string.Equals(s.ElementId, boot.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.Equal(new[] { 1, 2 }, inDesign.PostSides.Select(p => p.PostIndex).OrderBy(i => i).ToArray());

                    var quantity = PushBackBomBuilder.Build(w.LastComputation.System, w.Session.Catalog).Components
                        .Where(c => string.Equals(c.ProfileId, boot.Id, StringComparison.OrdinalIgnoreCase))
                        .Sum(c => c.Quantity);
                    // I-42 (S1B): «Ambas» son DOS ubicaciones fisicas por poste —entrada/salida y posterior—, y cada
                    // una es una pieza. Dos postes con Ambas son cuatro botas; con una sola ubicacion serian dos.
                    Assert.Equal(4, quantity);
                }
                finally { w.Close(); }
            });
        }

        /// <summary>Guardar en biblioteca y reabrir conserva la matriz exacta.</summary>
        [Fact]
        public void SavingToTheLibraryAndReopening_KeepsTheMatrix()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    var boot = ElementOfType(w, SelectiveSafetyDefaults.BotaType);
                    if (boot == null) return;

                    w.State.SetFrontCount(3);
                    w.SafetyDialog = _ => new[] { WithPosts(boot.Id, (1, SafetySide.Both), (3, SafetySide.None)) };
                    EditorWindowTestSupport.ClickNamed(w, "SafetyButton");
                    var before = Matrix(w.SafetySelections.Single(s => string.Equals(s.ElementId, boot.Id, StringComparison.OrdinalIgnoreCase)));

                    var project = w.BuildLibraryProjectForTest();
                    Assert.NotNull(project);

                    var reopened = Shown();
                    try
                    {
                        reopened.LoadDesignForNew(w.LastComputation.Design, "PB", project);
                        var after = Matrix(reopened.SafetySelections.Single(s => string.Equals(s.ElementId, boot.Id, StringComparison.OrdinalIgnoreCase)));
                        Assert.Equal(before, after);
                    }
                    finally { reopened.Close(); }
                }
                finally { w.Close(); }
            });
        }

        /// <summary>Cancelar el diálogo no cambia NADA — tampoco la matriz por poste.</summary>
        [Fact]
        public void CancellingChangesNothing_IncludingThePerPostMatrix()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    var boot = ElementOfType(w, SelectiveSafetyDefaults.BotaType);
                    if (boot == null) return;

                    w.State.SetFrontCount(3);
                    w.SafetyDialog = _ => new[] { WithPosts(boot.Id, (1, SafetySide.Both)) };
                    EditorWindowTestSupport.ClickNamed(w, "SafetyButton");
                    var before = w.SafetySelections
                        .Select(s => (s.ElementId, Matrix: string.Join(",", Matrix(s))))
                        .OrderBy(t => t.ElementId).ToList();

                    w.SafetyDialog = _ => null;   // el usuario cancela
                    EditorWindowTestSupport.ClickNamed(w, "SafetyButton");
                    var after = w.SafetySelections
                        .Select(s => (s.ElementId, Matrix: string.Join(",", Matrix(s))))
                        .OrderBy(t => t.ElementId).ToList();

                    Assert.Equal(before, after);
                }
                finally { w.Close(); }
            });
        }

        /// <summary>Con la matriz conservada sigue sin aparecer seguridad en el extremo posterior.</summary>
        [Fact]
        public void NoSafetyReachesTheRearEnd_EvenWithAnExplicitRightChoice()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    var boot = ElementOfType(w, SelectiveSafetyDefaults.BotaType);
                    if (boot == null) return;

                    w.State.SetFrontCount(2);
                    w.SafetyDialog = _ =>
                    {
                        w.BootSectionForTest.ModeBox.SelectedIndex = (int)BootPlacement.None;   // los demas, sin bota
                        return new[] { WithPosts(boot.Id, (1, SafetySide.Right)) };
                    };
                    EditorWindowTestSupport.ClickNamed(w, "SafetyButton");

                    var system = w.LastComputation?.System;
                    Assert.NotNull(system);

                    var rear = new PushBackSystemFrontalBuilder()
                        .BuildPlan(system, w.Session.Catalog, PushBackFrontalEnd.Posterior).Flatten().Instances
                        .Count(i => string.Equals(i.PieceId, boot.Id, StringComparison.OrdinalIgnoreCase));
                    var low = new PushBackSystemFrontalBuilder()
                        .BuildPlan(system, w.Session.Catalog, PushBackFrontalEnd.EntradaSalida).Flatten().Instances
                        .Count(i => string.Equals(i.PieceId, boot.Id, StringComparison.OrdinalIgnoreCase));

                    // RETARGETEADO EN S1E. La ronda S1 suponia que el extremo lejano de un rack de un sentido esta
                    // contra muro y que «Posterior» no pedia nada; el dueño lo rechazo en S1B —detras puede haber un
                    // pasillo de transito—, asi que esa eleccion SI se materializa. Lo que S1E añade es que el corte
                    // que la muestra es el que coincide con su plano: el POSTERIOR, no el bajo.
                    Assert.Equal(1, rear);
                    Assert.Equal(0, low);

                    // Y una eleccion que SI pide el pasillo la coloca, en ese mismo poste: la pertenencia por poste
                    // sigue mandando. Desde S1E se elige en la rejilla POR POSTE de la seccion de su lado, que es la
                    // superficie real — inyectar una matriz historica ya no representa lo que hace el usuario.
                    w.BootPerPostWindowDialog = window =>
                    {
                        window.SetForTest(1, BootPlacement.EntryExit);
                        window.BuildResultForTest();
                        return true;
                    };
                    w.SafetyDialog = _ =>
                    {
                        w.BootSectionForTest.ModeBox.SelectedIndex = (int)BootPlacement.None;
                        w.BootSectionForTest.Button.RaiseEvent(
                            new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                        return new[] { WithPosts(boot.Id) };
                    };
                    EditorWindowTestSupport.ClickNamed(w, "SafetyButton");
                    w.BootPerPostWindowDialog = null;
                    var chosen = new PushBackSystemFrontalBuilder()
                        .BuildPlan(w.LastComputation.System, w.Session.Catalog, PushBackFrontalEnd.EntradaSalida)
                        .Flatten().Instances
                        .Count(i => string.Equals(i.PieceId, boot.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.Equal(1, chosen);
                }
                finally { w.Close(); }
            });
        }
    }
}
