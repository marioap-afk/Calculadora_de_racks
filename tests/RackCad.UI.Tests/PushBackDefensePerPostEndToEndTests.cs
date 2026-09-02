using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
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
    /// I-42 (ronda 7C, defecto 2 de la Owner Validation) — LA DEFENSA POR POSTE, POR LA RUTA REAL.
    ///
    /// <para>
    /// El dueño reporto que cambiar el ON/OFF de un poste desde la ventana real no producia NINGUN cambio visible, y
    /// rechazo de antemano cualquier prueba de nivel bajo como evidencia: el contrato por poste ya estaba probado
    /// sobre el mecanismo (ronda 7B) y pasaba, asi que el fallo estaba en el CABLEADO.
    /// </para>
    ///
    /// <para>
    /// La causa medida: la rejilla trataba la casilla como un adorno. Una fila sin registro nace «Auto» en los dos
    /// extremos, y su OnOk descartaba entera toda fila con los dos «Auto» marcados —«todo automatico = ningun
    /// registro»— SIN mirar la casilla. Apagar un poste dejaba «Auto» marcado, la fila se tiraba y el rack no
    /// cambiaba. Ahora la casilla manda: tocarla saca a ESE extremo del automatico.
    /// </para>
    ///
    /// <para>
    /// Estas pruebas recorren la ruta ENTERA con los controles reales — ventana Push Back → «Elementos de seguridad»
    /// (la ventana real) → su rejilla por poste (la real) → Aceptar → commit → resolve → primitiva y BOM. Lo unico
    /// que se sustituye es el <c>ShowDialog</c>, que una prueba no puede ejecutar.
    /// </para>
    ///
    /// <para>
    /// <b>Ronda 7D.</b> La superficie cambio: la configuracion por poste ya no cuelga de la fila de la familia sino
    /// de una SECCION POR LADO, como los topes. Los contratos de esta clase no cambian —siguen siendo los mismos
    /// hechos sobre el mismo mecanismo— y se conducen ahora por esa seccion, que es la que el usuario tiene delante.
    /// </para>
    /// </summary>
    public sealed class PushBackDefensePerPostEndToEndTests
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

            return w;
        }

        /// <summary>
        /// EL GESTO DEL USUARIO, entero: abre «Elementos de seguridad», entra en la rejilla por poste de la seccion
        /// del lado A —la unica en un rack de un solo sentido— hace <paramref name="gesture"/> sobre sus controles
        /// reales y acepta las dos ventanas.
        /// </summary>
        private static void ThroughTheRealWindows(
            RackPushBackSystemWindow w, Action<SafetyDefensaGridWindow> gesture, bool accept = true)
        {
            w.DefenseDialog = section =>
            {
                var grid = new SafetyDefensaGridWindow(
                    "Defensa de montacargas", section.PostCount, section.Posts,
                    lowEndOnly: true, autoPerEnd: true, face: section.Face());
                gesture?.Invoke(grid);
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
                w.DefenseSectionForTest?.Configure();
                dialog.BuildResultForTest();
                dialog.Close();
                return accept;
            };
            EditorWindowTestSupport.ClickNamed(w, "SafetyButton");
            w.SafetyWindowDialog = null;
            w.DefenseDialog = null;
        }

        /// <summary>Las defensas que el dibujo materializa, como «X|Y» ordenadas.</summary>
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

        /// <summary>Cuantas defensas cuenta el BOM. Dibujo y lista deben moverse igual.</summary>
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

        private static SelectiveSafetySelection StoredDefense(RackPushBackSystemWindow w)
            => w.SafetySelections.FirstOrDefault(selection =>
            {
                var element = w.Session.Catalog.SafetyElements?.FirstOrDefault(entry =>
                    string.Equals(entry?.Id, selection.ElementId, StringComparison.OrdinalIgnoreCase));
                return element != null
                       && SelectiveSafetyDefaults.IsType(element.Type, SelectiveSafetyDefaults.DefensaType);
            });

        /// <summary>La linea base: la MISMA ruta sin tocar nada, para que la comparacion aisle el gesto.</summary>
        private static IReadOnlyList<string> Baseline(RackPushBackSystemWindow w)
        {
            ThroughTheRealWindows(w, null);
            return Defenses(w);
        }

        // ==================== el dato llega, y llega hasta el dibujo ====================

        /// <summary>Apagar un poste desde la rejilla real llega al DISEÑO resuelto como un registro explicito.</summary>
        [Fact]
        public void SafetyWindow_DefensePostChange_ReachesResolvedDesign()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    Baseline(w);
                    Assert.Empty(StoredDefense(w).DefensaPosts);   // nadie ha decidido nada todavia

                    ThroughTheRealWindows(w, grid => grid.ExitCheckForTest(1).IsChecked = false);

                    var design = w.LastComputation?.Design;
                    var inDesign = design.Structure.SafetySelections
                        .First(selection => string.Equals(
                            selection.ElementId, StoredDefense(w).ElementId, StringComparison.OrdinalIgnoreCase));
                    var record = Assert.Single(inDesign.DefensaPosts);
                    Assert.Equal(1, record.PostIndex);
                    Assert.Equal(0.0, PushBackDefenseSides.LengthOf(record, PushBackSide.A), 9);   // apagado = cero
                    Assert.False(PushBackDefenseSides.AutoOf(record, PushBackSide.A));
                    Assert.True(PushBackDefenseSides.AutoOf(record, PushBackSide.B));   // el otro lado, intacto
                }
                finally { w.Close(); }
            });
        }

        /// <summary>Y RETIRA exactamente la primitiva de ese poste, sin mover ninguna otra.</summary>
        [Fact]
        public void SafetyWindow_DefensePostOff_RemovesExpectedPrimitive()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    var before = Baseline(w);
                    ThroughTheRealWindows(w, grid => grid.ExitCheckForTest(1).IsChecked = false);
                    var after = Defenses(w);

                    Assert.Single(before.Except(after));           // se fue una, y solo una
                    Assert.Empty(after.Except(before));            // no aparecio ninguna nueva
                }
                finally { w.Close(); }
            });
        }

        /// <summary>Y volver a encenderlo la CREA otra vez: el viaje de ida y vuelta es una identidad.</summary>
        [Fact]
        public void SafetyWindow_DefensePostOn_CreatesExpectedPrimitive()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    var before = Baseline(w);
                    ThroughTheRealWindows(w, grid => grid.ExitCheckForTest(1).IsChecked = false);
                    var off = Defenses(w);
                    ThroughTheRealWindows(w, grid => grid.ExitCheckForTest(1).IsChecked = true);

                    Assert.True(off.Count < before.Count);
                    Assert.Equal(before, Defenses(w));
                }
                finally { w.Close(); }
            });
        }

        /// <summary>El BOM se mueve EXACTAMENTE igual que el dibujo: una sola verdad.</summary>
        [Fact]
        public void SafetyWindow_DefensePostChange_ChangesBomEqually()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    var before = Baseline(w).Count;
                    var bomBefore = DefensesInBom(w);
                    Assert.Equal(before, bomBefore);

                    ThroughTheRealWindows(w, grid => grid.ExitCheckForTest(1).IsChecked = false);

                    Assert.True(Defenses(w).Count < before);   // el gesto cambio algo, y la comparacion no es vacia
                    Assert.Equal(before - Defenses(w).Count, bomBefore - DefensesInBom(w));
                    Assert.Equal(Defenses(w).Count, DefensesInBom(w));
                }
                finally { w.Close(); }
            });
        }

        /// <summary>Tocar el poste 2 no toca al poste 3: la intencion es de UN poste.</summary>
        [Fact]
        public void SafetyWindow_DefensePost1_DoesNotChangePost2()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    var before = Baseline(w);
                    ThroughTheRealWindows(w, grid => grid.ExitCheckForTest(1).IsChecked = false);
                    var lost = before.Except(Defenses(w)).Single();

                    // Lo que se fue es del poste 2; todo lo del resto de las lineas sigue ahi.
                    Assert.All(before.Where(value => value != lost), value => Assert.Contains(value, Defenses(w)));
                    var record = Assert.Single(StoredDefense(w).DefensaPosts);
                    Assert.Equal(1, record.PostIndex);
                }
                finally { w.Close(); }
            });
        }

        /// <summary>Aceptar la ventana COMPROMETE la decision en el modelo del editor.</summary>
        [Fact]
        public void SafetyWindow_Accept_CommitsDefensePosts()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    Baseline(w);
                    ThroughTheRealWindows(w, grid => grid.ExitCheckForTest(1).IsChecked = false);

                    var record = Assert.Single(StoredDefense(w).DefensaPosts);
                    Assert.Equal(1, record.PostIndex);
                    Assert.Equal(0.0, PushBackDefenseSides.LengthOf(record, PushBackSide.A), 9);
                }
                finally { w.Close(); }
            });
        }

        /// <summary>Y cancelarla no compromete nada: ni el registro ni el dibujo se mueven.</summary>
        [Fact]
        public void SafetyWindow_Cancel_DoesNotCommitDefensePosts()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    var before = Baseline(w);
                    ThroughTheRealWindows(w, grid => grid.ExitCheckForTest(1).IsChecked = false, accept: false);

                    Assert.Empty(StoredDefense(w).DefensaPosts);
                    Assert.Equal(before, Defenses(w));
                }
                finally { w.Close(); }
            });
        }

        /// <summary>
        /// La decision sobrevive a guardar y volver a abrir (RACKEDITAR): es del rack, no de la pantalla.
        /// </summary>
        [Fact]
        public void SafetyWindow_DefensePosts_RackEditarRoundTrips()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    Baseline(w);
                    ThroughTheRealWindows(w, grid => grid.ExitCheckForTest(1).IsChecked = false);

                    var restored = PushBackDesignDocument.FromDomain(w.LastComputation.Design).ToDomain();
                    var selection = restored.Structure.SafetySelections
                        .First(item => item.DefensaPosts.Count > 0);
                    var record = Assert.Single(selection.DefensaPosts);

                    Assert.Equal(1, record.PostIndex);
                    Assert.Equal(0.0, PushBackDefenseSides.LengthOf(record, PushBackSide.A), 9);
                    Assert.False(PushBackDefenseSides.AutoOf(record, PushBackSide.A));
                }
                finally { w.Close(); }
            });
        }

        /// <summary>
        /// Un frente EN BLANCO no mueve la intencion a otra linea: el poste conserva su indice fisico y la defensa
        /// se queda donde el usuario la dejo. Es la regla que la ronda 6F cerro para las botas, leida aqui.
        /// </summary>
        [Fact]
        public void DefensePost_BlankDoesNotRelocate()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    Baseline(w);
                    ThroughTheRealWindows(w, grid => grid.ExitCheckForTest(2).IsChecked = false);
                    var defendedLines = Lines(Defenses(w));

                    // Un frente EN BLANCO por delante del poste decidido.
                    w.CompositeState.SetSlotPresent(PushBackSide.A, 0, false);

                    var record = Assert.Single(StoredDefense(w).DefensaPosts);
                    Assert.Equal(2, record.PostIndex);                 // el indice fisico no se compacta
                    Assert.Equal(0.0, PushBackDefenseSides.LengthOf(record, PushBackSide.A), 9);
                    Assert.Equal(defendedLines, Lines(Defenses(w)));   // y la linea sin defensa sigue siendo la misma
                }
                finally { w.Close(); }
            });
        }

        /// <summary>Las LINEAS (la Y transversal) que llevan defensa, sin repetir.</summary>
        private static IReadOnlyList<string> Lines(IEnumerable<string> defenses)
            => defenses.Select(value => value.Split('|')[1]).Distinct()
                .OrderBy(value => value, StringComparer.Ordinal).ToList();

        /// <summary>
        /// Una intencion sobre un poste que deja de EXISTIR no materializa nada, y no salta a otro poste: al
        /// encoger el rack, la ultima linea desaparece y con ella su decision, sin dejar defensa fantasma ni
        /// arrastrar la intencion a la linea vecina.
        /// </summary>
        [Fact]
        public void DefensePost_NonApplicableDoesNotMaterialize()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    Baseline(w);
                    ThroughTheRealWindows(w, grid => grid.ExitCheckForTest(3).IsChecked = false);   // el ultimo poste
                    var linesBefore = Lines(Defenses(w));

                    w.CompositeState.SetSlotCount(2);   // 3 postes: el poste 4 ya no existe
                    ThroughTheRealWindows(w, null);     // reabrir y aceptar sin tocar nada

                    var lines = Lines(Defenses(w));
                    Assert.Equal(3, lines.Count);   // solo las tres lineas que quedan
                    Assert.All(lines, value => Assert.Contains(value, linesBefore));   // ninguna nueva: nada se movio
                    Assert.Empty(StoredDefense(w).DefensaPosts);   // la decision del poste 4 se fue con el poste 4
                }
                finally { w.Close(); }
            });
        }

        /// <summary>
        /// El extremo ALTO no esta PROHIBIDO, solo apagado por defecto (PB-009): encenderlo a mano lo materializa,
        /// y en la linea que el usuario eligio — no en otra. Es la contraparte de la prueba anterior.
        /// </summary>
        [Fact]
        public void DefensePost_ExplicitHighEnd_IsHonouredOnItsOwnLine()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();   // un solo sentido: el extremo alto viene apagado
                try
                {
                    var before = Baseline(w);

                    // En un rack de un solo sentido la seccion ES toda la defensa y abre la rejilla historica de
                    // los dos extremos, que es lo que conserva la capacidad de PB-009.
                    ThroughTheRealWindows(w, grid => grid.EntranceCheckForTest(1).IsChecked = true);
                    var added = Defenses(w).Except(before).ToList();

                    var one = Assert.Single(added);
                    Assert.Equal(before.First(value => value.EndsWith(one.Split('|')[1], StringComparison.Ordinal))
                        .Split('|')[1], one.Split('|')[1]);   // la MISMA linea que ya llevaba la de abajo
                    Assert.Empty(before.Except(Defenses(w)));  // y no se movio ninguna existente
                }
                finally { w.Close(); }
            });
        }

        /// <summary>
        /// Y el CONTROL dice la verdad: la casilla que el usuario apago aparece apagada al volver a abrir la
        /// rejilla. Sin esto la decision existiria en el modelo y no en la pantalla, que es media correccion.
        /// </summary>
        [Fact]
        public void SafetyWindow_DefensePostOff_IsShownOffWhenReopened()
        {
            StaTestRunner.Run(() =>
            {
                var w = Composite();
                try
                {
                    Baseline(w);
                    ThroughTheRealWindows(w, grid => grid.ExitCheckForTest(1).IsChecked = false);

                    bool? reopened = null;
                    ThroughTheRealWindows(w, grid => reopened = grid.ExitCheckForTest(1).IsChecked);

                    Assert.False(reopened);
                }
                finally { w.Close(); }
            });
        }
    }
}
