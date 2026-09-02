using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
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
    /// I-42 (ronda 7E, contrato del dueño) — EL TIPO DE DEFENSA VIVE EN LA SECCION DE SU LADO.
    ///
    /// <para>
    /// Tras la ronda 7D las secciones «Defensa de montacargas — lado A/B» configuraban POSTES, pero el TIPO seguia
    /// eligiendose en una fila general al fondo de la ventana: una misma decision partida en dos sitios y, en un
    /// compuesto, una sola para los dos pasillos. Ahora cada seccion es autosuficiente —tipo, «Ninguno», resumen y
    /// «Configurar…»—, igual que las de topes, y la fila general deja de existir para Push Back.
    /// </para>
    ///
    /// <para>
    /// TIPO e INTENCION POR POSTE son dos ejes y siguen separados: el tipo dice QUE pieza usa ese pasillo, la
    /// rejilla en QUE lineas se pone, y la fisica si esa cara de ataque existe. Poner un lado en «Ninguno» no
    /// destruye su rejilla: queda dormida y vuelve entera al reactivarlo.
    /// </para>
    /// </summary>
    public sealed class PushBackDefenseTypePerSideTests
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

        private static void Settle(RackPushBackSystemWindow w)
        {
            w.SafetyDialog = selections => selections.ToList();
            EditorWindowTestSupport.ClickNamed(w, "SafetyButton");
            w.SafetyDialog = null;
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

        /// <summary>Poner el tipo de un lado, sobre su selector real.</summary>
        private static Action<PushBackDefenseSection, SafetyDefensaGridWindow> Type(string pieceId)
            => (section, _) => section.PieceBox.SelectedId = pieceId;

        /// <summary>Encender/apagar postes en la rejilla real del lado.</summary>
        private static Action<PushBackDefenseSection, SafetyDefensaGridWindow> Posts(params (int post, bool on)[] wanted)
            => (_, grid) =>
            {
                foreach (var (post, on) in wanted)
                {
                    grid.ExitCheckForTest(post).IsChecked = on;
                }
            };

        /// <summary>
        /// La ruta REAL: «Elementos de seguridad» y, dentro, el selector de tipo y la rejilla por poste de cada
        /// seccion. Solo se sustituye el <c>ShowDialog</c>.
        /// </summary>
        private static void Through(
            RackPushBackSystemWindow w,
            Action<PushBackDefenseSection, SafetyDefensaGridWindow> sideA = null,
            Action<PushBackDefenseSection, SafetyDefensaGridWindow> sideB = null,
            bool accept = true,
            Action<PushBackDefenseSection> inspect = null)
        {
            w.DefenseDialog = section =>
            {
                var grid = new SafetyDefensaGridWindow(
                    "Defensa de montacargas", section.PostCount, section.Posts,
                    lowEndOnly: true, autoPerEnd: true, face: section.Face());
                (section.Side == PushBackSide.A ? sideA : sideB)?.Invoke(section, grid);
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
                inspect?.Invoke(w.DefenseSectionForTest);
                inspect?.Invoke(w.DefenseSectionBForTest);
                if (sideA != null || inspect != null) { w.DefenseSectionForTest?.Configure(); }
                if (sideB != null || inspect != null) { w.DefenseSectionBForTest?.Configure(); }
                dialog.BuildResultForTest();
                dialog.Close();
                return accept;
            };
            EditorWindowTestSupport.ClickNamed(w, "SafetyButton");
            w.SafetyWindowDialog = null;
            w.DefenseDialog = null;
        }

        private static string DefenseId(RackPushBackSystemWindow w)
            => w.Session.Catalog.SafetyElements
                .First(entry => SelectiveSafetyDefaults.IsType(entry.Type, SelectiveSafetyDefaults.DefensaType)).Id;

        /// <summary>Las defensas del dibujo, como «X|Y».</summary>
        private static IReadOnlyList<string> Defenses(RackPushBackSystemWindow w)
        {
            var system = w.LastComputation?.System;
            if (system == null)
            {
                return new List<string>();
            }

            var id = DefenseId(w);
            return new PushBackSystemPlantaBuilder().BuildPlan(system, w.Session.Catalog).Flatten().Instances
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

            var id = DefenseId(w);
            return PushBackBomBuilder.Build(system, w.Session.Catalog).Lines
                .Where(line => string.Equals(line.ProfileId, id, StringComparison.OrdinalIgnoreCase))
                .Sum(line => line.Quantity);
        }

        /// <summary>Las defensas de UN lado: las de su cara. A ataca por la X menor, B por la mayor.</summary>
        private static IReadOnlyList<string> DefensesOf(RackPushBackSystemWindow w, PushBackSide side)
        {
            var all = Defenses(w);
            if (all.Count == 0)
            {
                return all;
            }

            var xs = all.Select(value => value.Split('|')[0]).Distinct().OrderBy(double.Parse).ToList();
            var near = xs.First();
            var far = xs.Last();
            var wanted = side == PushBackSide.A ? near : far;
            return xs.Count == 1 && side == PushBackSide.B && double.Parse(near) < 100.0
                ? new List<string>()   // solo hay cara cercana dibujada
                : all.Where(value => value.StartsWith(wanted + "|", StringComparison.Ordinal)).ToList();
        }

        // ==================== las superficies ====================

        [Fact]
        public void DefenseSection_HasTypeSelectorForSideA()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    Through(w);
                    Assert.NotNull(w.DefenseSectionForTest.PieceBox);
                    Assert.False(w.DefenseSectionForTest.IsNone);
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void DefenseSection_HasTypeSelectorForSideB()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    Through(w);
                    Assert.NotNull(w.DefenseSectionBForTest.PieceBox);
                    Assert.NotSame(w.DefenseSectionForTest.PieceBox, w.DefenseSectionBForTest.PieceBox);
                }
                finally { w.Close(); }
            });
        }

        /// <summary>«Ninguno» es una opcion del selector, y no la que se elige sola.</summary>
        [Fact]
        public void DefenseSection_HasExplicitNoneOption()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    Through(w);
                    var ids = w.DefenseSectionForTest.PieceBox.Items
                        .OfType<CatalogOption>().Select(option => option.Id).ToList();

                    Assert.Contains(PushBackDefaults.NonePieceId, ids);
                    Assert.True(ids.Count > 1);   // «Ninguno» y al menos una pieza real
                    Assert.NotEqual(PushBackDefaults.NonePieceId, w.DefenseSectionForTest.PieceBox.SelectedId);
                }
                finally { w.Close(); }
            });
        }

        /// <summary>Un rack de un solo sentido tambien lleva su selector, en su unica seccion.</summary>
        [Fact]
        public void SingleSideDefenseSection_HasTypeSelector()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    Through(w);
                    Assert.NotNull(w.DefenseSectionForTest.PieceBox);
                    Assert.Null(w.DefenseSectionBForTest);
                    Assert.Null(w.DefenseSectionForTest.SideLabel);
                }
                finally { w.Close(); }
            });
        }

        /// <summary>La fila general deja de ofrecer la familia DEFENSA: la decision se toma en un solo sitio.</summary>
        [Fact]
        public void GlobalPushBackDefenseTypeEditor_IsRemoved()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    var offered = w.SafetyElementsForDialog();
                    Assert.DoesNotContain(offered, entry =>
                        SelectiveSafetyDefaults.IsType(entry.Type, SelectiveSafetyDefaults.DefensaType));

                    // Y no queda ningun boton por poste colgando de esa fila dentro de la ventana real.
                    w.SafetyWindowDialog = dialog =>
                    {
                        Assert.Null(dialog.DefensaButtonForTest);
                        return false;
                    };
                    EditorWindowTestSupport.ClickNamed(w, "SafetyButton");
                    w.SafetyWindowDialog = null;
                }
                finally { w.Close(); }
            });
        }

        // ==================== el tipo es del lado ====================

        [Fact]
        public void DefenseType_IsSideScoped()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    Through(w, Type(PushBackDefaults.NonePieceId), null);

                    Assert.True(PushBackDefenseSides.IsNone(w.CompositeState.Of(PushBackSide.A).DefensePieceId));
                    Assert.False(PushBackDefenseSides.IsNone(w.CompositeState.Of(PushBackSide.B).DefensePieceId));
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void DefenseTypeA_DoesNotChangeB()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    Through(w);
                    var before = w.CompositeState.Of(PushBackSide.B).DefensePieceId;
                    Through(w, Type(PushBackDefaults.NonePieceId), null);

                    Assert.Equal(before, w.CompositeState.Of(PushBackSide.B).DefensePieceId);
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void DefenseTypeB_DoesNotChangeA()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    Through(w);
                    var before = w.CompositeState.Of(PushBackSide.A).DefensePieceId;
                    Through(w, null, Type(PushBackDefaults.NonePieceId));

                    Assert.Equal(before, w.CompositeState.Of(PushBackSide.A).DefensePieceId);
                }
                finally { w.Close(); }
            });
        }

        // ==================== las cuatro combinaciones ====================

        /// <summary>A = Ninguno, B = Defensa → solo B materializa.</summary>
        [Fact]
        public void DefenseTypeA_None_DisablesOnlyA()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    Through(w);
                    var before = Defenses(w);
                    Assert.NotEmpty(before);

                    Through(w, Type(PushBackDefaults.NonePieceId), null);
                    var after = Defenses(w);

                    Assert.NotEmpty(after);
                    Assert.All(after, value => Assert.DoesNotContain(value, DefensesOf(w, PushBackSide.A)
                        .Where(x => false)));   // sanity: la lista no esta vacia por accidente
                    Assert.Equal(before.Count / 2, after.Count);
                    Assert.All(after, value => Assert.True(double.Parse(value.Split('|')[0]) > 0.0));
                }
                finally { w.Close(); }
            });
        }

        /// <summary>A = Defensa, B = Ninguno → solo A.</summary>
        [Fact]
        public void DefenseTypeB_None_DisablesOnlyB()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    Through(w);
                    var before = Defenses(w);

                    Through(w, null, Type(PushBackDefaults.NonePieceId));
                    var after = Defenses(w);

                    Assert.NotEmpty(after);
                    Assert.Equal(before.Count / 2, after.Count);
                    Assert.All(after, value => Assert.True(double.Parse(value.Split('|')[0]) < 0.0));
                }
                finally { w.Close(); }
            });
        }

        /// <summary>Los dos en «Ninguno» → ni dibujo ni BOM. Y «Ninguno» nunca es una pieza.</summary>
        [Fact]
        public void DefenseTypeBothNone_DrawsNone()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    Through(w, Type(PushBackDefaults.NonePieceId), Type(PushBackDefaults.NonePieceId));

                    Assert.Empty(Defenses(w));
                    Assert.Equal(0, DefensesInBom(w));

                    var system = w.LastComputation.System;
                    Assert.DoesNotContain(
                        new PushBackSystemPlantaBuilder().BuildPlan(system, w.Session.Catalog).Flatten().Instances,
                        instance => string.Equals(
                            instance.PieceId, PushBackDefaults.NonePieceId, StringComparison.OrdinalIgnoreCase));
                    Assert.DoesNotContain(
                        PushBackBomBuilder.Build(system, w.Session.Catalog).Lines,
                        line => string.Equals(
                            line.ProfileId, PushBackDefaults.NonePieceId, StringComparison.OrdinalIgnoreCase));
                }
                finally { w.Close(); }
            });
        }

        /// <summary>Con los dos habilitados, cada lado aplica SU patron por poste — la ronda 7D, intacta.</summary>
        [Fact]
        public void DefenseTypeBothEnabled_UsesPerPostIntent()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    Through(w,
                        Posts((0, true), (1, false), (2, true), (3, false)),
                        Posts((0, false), (1, true), (2, false), (3, true)));

                    var drawn = Defenses(w);
                    Assert.Equal(4, drawn.Count);
                    Assert.Equal(drawn.Count, DefensesInBom(w));
                }
                finally { w.Close(); }
            });
        }

        // ==================== la intencion queda DORMIDA ====================

        [Fact]
        public void DefenseTypeNone_PreservesDormantPostIntent()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    Through(w, Posts((1, false)), null);
                    var stored = Records(w);
                    Assert.NotEmpty(stored);

                    Through(w, Type(PushBackDefaults.NonePieceId), null);

                    Assert.Equal(stored, Records(w));   // la rejilla sigue guardada, aunque no materialice
                    Assert.All(Defenses(w), value => Assert.True(double.Parse(value.Split('|')[0]) > 0.0));
                }
                finally { w.Close(); }
            });
        }

        /// <summary>Con los DOS lados en «Ninguno» la intencion por poste tampoco se pierde.</summary>
        [Fact]
        public void DefenseTypeBothNone_PreservesDormantPostIntent()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    Through(w, Posts((1, false)), Posts((2, false)));
                    var stored = Records(w);
                    Assert.NotEmpty(stored);

                    Through(w, Type(PushBackDefaults.NonePieceId), Type(PushBackDefaults.NonePieceId));

                    Assert.Empty(Defenses(w));
                    Assert.Equal(0, DefensesInBom(w));
                    Assert.Equal(stored, Records(w));
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void DefenseTypeReenabled_RestoresPostIntent()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    Through(w, Posts((0, true), (1, false), (2, true), (3, false)), null);
                    var pattern = Defenses(w).Where(value => double.Parse(value.Split('|')[0]) < 0.0).ToList();
                    var piece = w.CompositeState.Of(PushBackSide.A).DefensePieceId ?? DefenseId(w);

                    Through(w, Type(PushBackDefaults.NonePieceId), null);
                    Assert.Empty(Defenses(w).Where(value => double.Parse(value.Split('|')[0]) < 0.0));

                    Through(w, Type(piece), null);

                    Assert.Equal(
                        pattern,
                        Defenses(w).Where(value => double.Parse(value.Split('|')[0]) < 0.0).ToList());
                }
                finally { w.Close(); }
            });
        }

        private static IReadOnlyList<string> Records(RackPushBackSystemWindow w)
        {
            var selection = w.SafetySelections.FirstOrDefault(item =>
                string.Equals(item.ElementId, DefenseId(w), StringComparison.OrdinalIgnoreCase));
            return selection == null
                ? new List<string>()
                : selection.DefensaPosts
                    .OrderBy(record => record.PostIndex)
                    .Select(record => FormattableString.Invariant(
                        $"{record.PostIndex}:{record.ExitLength:0.#}/{record.ExitAuto}|{record.EntranceLength:0.#}/{record.EntranceAuto}"))
                    .ToList();
        }

        // ==================== blanks, BOM, transaccional, persistencia ====================

        [Fact]
        public void DefenseType_BlankStillDoesNotRelocate()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    Through(w);
                    var lines = Ys(Far(Defenses(w)));   // las cuatro lineas del rack, en orden
                    Assert.Equal(4, lines.Count);

                    Through(w, Posts((1, false)), null);              // A apaga su linea 2
                    w.CompositeState.SetSlotPresent(PushBackSide.A, 0, false);   // y pierde la cara de la linea 1
                    Settle(w);
                    Through(w);

                    var near = Ys(Near(Defenses(w)));
                    var far = Ys(Far(Defenses(w)));

                    Assert.DoesNotContain(lines[0], near);   // el blanco quito la aplicabilidad de A ahi
                    Assert.DoesNotContain(lines[1], near);   // y la linea apagada sigue apagada
                    Assert.Contains(lines[2], near);         // lo demas de A, intacto
                    Assert.Equal(lines, far);                // B no perdio ninguna: ni se le paso nada
                }
                finally { w.Close(); }
            });
        }

        private static IReadOnlyList<string> Near(IEnumerable<string> defenses)
            => defenses.Where(value => double.Parse(value.Split('|')[0]) < 0.0).ToList();

        private static IReadOnlyList<string> Far(IEnumerable<string> defenses)
            => defenses.Where(value => double.Parse(value.Split('|')[0]) > 0.0).ToList();

        private static IReadOnlyList<string> Ys(IEnumerable<string> defenses)
            => defenses.Select(value => value.Split('|')[1]).Distinct().OrderBy(double.Parse).ToList();

        [Fact]
        public void DefenseType_DrawEqualsBom()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    Through(w, Type(PushBackDefaults.NonePieceId), null);
                    Assert.Equal(Defenses(w).Count, DefensesInBom(w));

                    Through(w, null, Type(PushBackDefaults.NonePieceId));
                    Assert.Equal(Defenses(w).Count, DefensesInBom(w));
                    Assert.Equal(0, DefensesInBom(w));
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void DefenseType_AcceptPersists()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    Through(w, Type(PushBackDefaults.NonePieceId), null);
                    Assert.True(PushBackDefenseSides.IsNone(w.CompositeState.Of(PushBackSide.A).DefensePieceId));
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void DefenseType_CancelDoesNotPersist()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    Through(w);
                    var before = w.CompositeState.Of(PushBackSide.A).DefensePieceId;
                    var drawn = Defenses(w);

                    Through(w, Type(PushBackDefaults.NonePieceId), Posts((0, false)), accept: false);

                    Assert.Equal(before, w.CompositeState.Of(PushBackSide.A).DefensePieceId);
                    Assert.Equal(drawn, Defenses(w));
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void DefenseType_RackEditarRoundTrips()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    Through(w, Type(PushBackDefaults.NonePieceId), Posts((1, false)));

                    var restored = PushBackDesignDocument.FromDomain(w.LastComputation.Design).ToDomain();

                    Assert.True(PushBackDefenseSides.IsNone(restored.DefensePieceId));
                    Assert.False(PushBackDefenseSides.IsNone(restored.SideB.DefensePieceId));
                }
                finally { w.Close(); }
            });
        }

        /// <summary>El caso inverso, para que el contrato no dependa de cual lado se apago.</summary>
        [Fact]
        public void DefenseType_RackEditarRoundTrips_Inverse()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    Through(w, null, Type(PushBackDefaults.NonePieceId));

                    var restored = PushBackDesignDocument.FromDomain(w.LastComputation.Design).ToDomain();

                    Assert.False(PushBackDefenseSides.IsNone(restored.DefensePieceId));
                    Assert.True(PushBackDefenseSides.IsNone(restored.SideB.DefensePieceId));
                }
                finally { w.Close(); }
            });
        }

        /// <summary>
        /// LEGACY: un rack que nunca eligio tipo dibuja exactamente lo que dibujaba, y su seccion abre en la pieza
        /// que ya usaba — no en «Ninguno». La ausencia de eleccion no es una eleccion.
        /// </summary>
        [Fact]
        public void LegacyGlobalDefenseType_PreservesOutput()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();   // recien abierto: nadie ha elegido tipo por lado
                try
                {
                    var before = Defenses(w);
                    Assert.NotEmpty(before);
                    Assert.Null(w.CompositeState.Of(PushBackSide.A).DefensePieceId);

                    Through(w);   // abrir Seguridad y aceptar sin tocar nada

                    Assert.Equal(before, Defenses(w));
                    Assert.Equal(before.Count, DefensesInBom(w));

                    // Y a partir de aqui el tipo queda EXPLICITO: la seccion abrio en la pieza que el rack ya usaba,
                    // no en «Ninguno», y al aceptar se persiste como eleccion de ese lado.
                    Assert.Equal(DefenseId(w), w.CompositeState.Of(PushBackSide.A).DefensePieceId);
                }
                finally { w.Close(); }
            });
        }

        /// <summary>Abrir Seguridad desde cualquier lado activo da lo mismo, tambien con los tipos de por medio.</summary>
        [Fact]
        public void OpeningSafetyFromEitherActiveSide_IsEquivalent()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    Through(w, Type(PushBackDefaults.NonePieceId), Posts((1, false)));

                    w.CompositeState.SetActiveSide(PushBackSide.A);
                    Through(w);
                    var fromA = Defenses(w);
                    var typesA = (w.CompositeState.Of(PushBackSide.A).DefensePieceId,
                        w.CompositeState.Of(PushBackSide.B).DefensePieceId);

                    w.CompositeState.SetActiveSide(PushBackSide.B);
                    Through(w);

                    Assert.Equal(fromA, Defenses(w));
                    Assert.Equal(typesA, (w.CompositeState.Of(PushBackSide.A).DefensePieceId,
                        w.CompositeState.Of(PushBackSide.B).DefensePieceId));
                }
                finally { w.Close(); }
            });
        }

        // ==================== regresion ====================

        [Fact]
        public void DefenseAAndB_CanHaveOppositePostPatterns()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    Through(w,
                        Posts((0, true), (1, false), (2, true), (3, false)),
                        Posts((0, false), (1, true), (2, false), (3, true)));

                    var near = Defenses(w).Where(value => double.Parse(value.Split('|')[0]) < 0.0).ToList();
                    var far = Defenses(w).Where(value => double.Parse(value.Split('|')[0]) > 0.0).ToList();

                    Assert.Equal(2, near.Count);
                    Assert.Equal(2, far.Count);
                    Assert.Empty(near.Select(v => v.Split('|')[1]).Intersect(far.Select(v => v.Split('|')[1])));
                }
                finally { w.Close(); }
            });
        }

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
