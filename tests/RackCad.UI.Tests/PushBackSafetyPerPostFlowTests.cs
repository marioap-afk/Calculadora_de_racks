using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using RackCad.UI;
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
                    w.SafetyDialog = _ => new[] { WithPosts(boot.Id, (1, SafetySide.Both), (2, SafetySide.Both)) };
                    EditorWindowTestSupport.ClickNamed(w, "SafetyButton");

                    var design = w.LastComputation?.Design;
                    Assert.NotNull(design);
                    var inDesign = design.Structure.SafetySelections
                        .Single(s => string.Equals(s.ElementId, boot.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.Equal(new[] { 1, 2 }, inDesign.PostSides.Select(p => p.PostIndex).OrderBy(i => i).ToArray());

                    var quantity = PushBackBomBuilder.Build(w.LastComputation.System, w.Session.Catalog).Components
                        .Where(c => string.Equals(c.ProfileId, boot.Id, StringComparison.OrdinalIgnoreCase))
                        .Sum(c => c.Quantity);
                    Assert.Equal(2, quantity);   // dos postes elegidos, dos botas
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
                    w.SafetyDialog = _ => new[] { WithPosts(boot.Id, (1, SafetySide.Right)) };
                    EditorWindowTestSupport.ClickNamed(w, "SafetyButton");

                    var system = w.LastComputation?.System;
                    Assert.NotNull(system);

                    var rear = new PushBackSystemFrontalBuilder()
                        .BuildPlan(system, w.Session.Catalog, PushBackFrontalEnd.Posterior).Flatten().Instances
                        .Count(i => string.Equals(i.PieceId, boot.Id, StringComparison.OrdinalIgnoreCase));
                    var low = new PushBackSystemFrontalBuilder()
                        .BuildPlan(system, w.Session.Catalog, PushBackFrontalEnd.EntradaSalida).Flatten().Instances
                        .Count(i => string.Equals(i.PieceId, boot.Id, StringComparison.OrdinalIgnoreCase));

                    Assert.Equal(0, rear);   // nada atrás...
                    Assert.Equal(1, low);    // ...y la elección del usuario aterriza en el extremo bajo
                }
                finally { w.Close(); }
            });
        }
    }
}
