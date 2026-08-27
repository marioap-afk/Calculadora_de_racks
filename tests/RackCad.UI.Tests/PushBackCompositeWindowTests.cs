using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Shared;
using RackCad.UI.Controls;
using RackCad.UI.Systems.PushBack;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-42 (G6) — la seccion COMPUESTA de la ventana real: selector de lado, hueco, separador central, topologia por
    /// celda con sus cinco alcances y estructura efectiva con restauracion. Lo que se fija es que la ventana sigue
    /// siendo una capa fina sobre el modelo puro y que un rack de un solo sentido no cambia en nada.
    /// </summary>
    public sealed class PushBackCompositeWindowTests
    {
        private static ComboBox Combo(RackPushBackSystemWindow w, string name) => (ComboBox)w.FindName(name);
        private static CheckBox Check(RackPushBackSystemWindow w, string name) => (CheckBox)w.FindName(name);
        private static Button Btn(RackPushBackSystemWindow w, string name) => (Button)w.FindName(name);
        private static NumericField Field(RackPushBackSystemWindow w, string name) => (NumericField)w.FindName(name);

        private static void Click(ButtonBase button)
            => button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, button));

        private static void LoseFocus(FrameworkElement element)
            => element.RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, element));

        private static StackPanel Section(RackPushBackSystemWindow w) => (StackPanel)w.FindName("CompositeSection");

        /// <summary>
        /// La casilla «En blanco» de la columna <paramref name="front"/> de la matriz: desde I-42 (ronda
        /// post-82e918b) es la UNICA autoridad visible para retirar un frente de un lado. Actua sobre el lado que
        /// se este editando, asi que el selector de lado decide a quien afecta.
        /// </summary>
        private static CheckBox Blank(RackPushBackSystemWindow w, int front)
        {
            var boxes = EditorWindowTestSupport.FindAll<CheckBox>(w)
                .Where(box => box.Content as string == "En blanco")
                .ToList();
            return front >= 0 && front < boxes.Count ? boxes[front] : null;
        }

        /// <summary>Declara o retira un frente en el lado que se este editando, con el control real.</summary>
        private static void SetBlank(RackPushBackSystemWindow w, int front, bool blank)
        {
            var box = Blank(w, front);
            if (box != null)
            {
                box.IsChecked = blank;
            }
        }

        [Fact]
        public void ANewRack_OpensAsSingleSided_WithTheCompositeSectionCollapsed()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                // COLAPSADA, no solo deshabilitada: con un rack de un sentido no debe ocupar espacio en la barra.
                return !w.CompositeState.SideBPresent
                       && w.CompositeState.ActiveSide == PushBackSide.A
                       && Check(w, "SideBPresentCheck").IsChecked != true
                       && Section(w).Visibility == Visibility.Collapsed
                       && Check(w, "SideBPresentCheck").Visibility == Visibility.Visible
                       && w.LastComputation.IsValid
                       && !w.LastComputation.System.IsComposite;
            });

            Assert.True(ok);
        }

        [Fact]
        public void DeclaringSideB_ShowsTheSection_AndProducesACompositeSystem()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                Check(w, "SideBPresentCheck").IsChecked = true;

                // La CAPACIDAD abre la seccion, pero por si sola NO convierte el rack: mientras ningun frente tenga
                // lado B el rack sigue siendo, fisicamente, el de un solo sentido. En cuanto se declara en uno, si.
                var capability = w.CompositeState.SideBPresent
                       && Section(w).Visibility == Visibility.Visible
                       && Combo(w, "SideSelectorBox").IsEnabled
                       && Field(w, "GapBox").IsEnabled
                       && Btn(w, "ApplyTopologyButton").IsEnabled
                       && w.LastComputation.IsValid
                       && !w.LastComputation.System.IsComposite;

                DeclareSideBOnEveryFront(w, w.CompositeState.SlotCount);
                return capability
                       && w.LastComputation.IsValid
                       && w.LastComputation.System.IsComposite;
            });

            Assert.True(ok);
        }

        [Fact]
        public void TogglingTheCompositeOff_CollapsesTheSection_AndKeepsSideBDormant()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                Check(w, "SideBPresentCheck").IsChecked = true;
                // I-42: la CAPACIDAD ya no declara PRESENCIA; este caso quiere el rack compuesto entero.
                DeclareSideBOnEveryFront(w, w.CompositeState.SlotCount);
                Combo(w, "SideSelectorBox").SelectedIndex = 1;
                w.CompositeState.SideB.SetFrontCount(3);
                Field(w, "GapBox").SetNumber(14.0);
                LoseFocus(Field(w, "GapBox"));

                Check(w, "SideBPresentCheck").IsChecked = false;
                var collapsed = Section(w).Visibility == Visibility.Collapsed
                                && !w.LastComputation.System.IsComposite;

                Check(w, "SideBPresentCheck").IsChecked = true;
                // I-42: la CAPACIDAD ya no declara PRESENCIA; este caso quiere el rack compuesto entero.
                DeclareSideBOnEveryFront(w, w.CompositeState.SlotCount);
                // Todo reaparece intacto: la configuracion del lado B quedo dormante, no se destruyo.
                return collapsed
                       && Section(w).Visibility == Visibility.Visible
                       && w.CompositeState.SideB.Structure.Count == 3
                       && Math.Abs(w.CompositeState.Gap - 14.0) < 1e-6
                       && w.LastComputation.System.IsComposite;
            });

            Assert.True(ok);
        }

        [Fact]
        public void ApplyingAnEmptyStructureField_IsNotARestore()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                Check(w, "SideBPresentCheck").IsChecked = true;
                // I-42: la CAPACIDAD ya no declara PRESENCIA; este caso quiere el rack compuesto entero.
                DeclareSideBOnEveryFront(w, w.CompositeState.SlotCount);
                Field(w, "StructureOverrideBox").SetNumber(9.0);
                Click(Btn(w, "ApplyStructureButton"));
                var applied = w.CompositeState.StructureOverrideA;

                // Vaciar el campo y pulsar «Aplicar» NO restaura: restaurar es su propio boton.
                Field(w, "StructureOverrideBox").SetNumber(null);
                Click(Btn(w, "ApplyStructureButton"));
                var afterEmptyApply = w.CompositeState.StructureOverrideA;

                Click(Btn(w, "RestoreStructureButton"));
                return applied == 9 && afterEmptyApply == 9 && w.CompositeState.StructureOverrideA == null;
            });

            Assert.True(ok);
        }

        [Fact]
        public void TheSideSelector_SwitchesTheMatrixWithoutLosingTheOtherSide()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                Check(w, "SideBPresentCheck").IsChecked = true;
                // I-42: la CAPACIDAD ya no declara PRESENCIA; este caso quiere el rack compuesto entero.
                DeclareSideBOnEveryFront(w, w.CompositeState.SlotCount);
                var sideA = w.State;

                Combo(w, "SideSelectorBox").SelectedIndex = 1;
                var sideB = w.State;

                Combo(w, "SideSelectorBox").SelectedIndex = 0;
                return !ReferenceEquals(sideA, sideB)
                       && ReferenceEquals(w.State, sideA)
                       && w.CompositeState.ActiveSide == PushBackSide.A;
            });

            Assert.True(ok);
        }

        [Fact]
        public void RetiringSideB_KeepsItsConfigurationDormant()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                Check(w, "SideBPresentCheck").IsChecked = true;
                // I-42: la CAPACIDAD ya no declara PRESENCIA; este caso quiere el rack compuesto entero.
                DeclareSideBOnEveryFront(w, w.CompositeState.SlotCount);
                Combo(w, "SideSelectorBox").SelectedIndex = 1;
                w.CompositeState.SideB.SetFrontCount(3);

                Check(w, "SideBPresentCheck").IsChecked = false;
                var backToA = w.CompositeState.ActiveSide == PushBackSide.A && !w.LastComputation.System.IsComposite;

                Check(w, "SideBPresentCheck").IsChecked = true;
                // I-42: la CAPACIDAD ya no declara PRESENCIA; este caso quiere el rack compuesto entero.
                DeclareSideBOnEveryFront(w, w.CompositeState.SlotCount);

                // I-42 (ronda Owner): la retícula transversal es del RACK, asi que al volver se IGUALA — creciendo,
                // nunca recortando: el lado B conserva sus tres ranuras y el lado A, que solo tenia una, recibe las
                // otras dos como AUSENTES. Nada se destruye y nada se inventa.
                return backToA
                       && w.CompositeState.SideB.Structure.Count == 3
                       && w.CompositeState.SideA.Structure.Count == 3
                       && w.CompositeState.IsSlotPresent(PushBackSide.A, 0)
                       && !w.CompositeState.IsSlotPresent(PushBackSide.A, 1)
                       && !w.CompositeState.IsSlotPresent(PushBackSide.A, 2)
                       && w.CompositeState.IsSlotPresent(PushBackSide.B, 2);
            });

            Assert.True(ok);
        }

        [Fact]
        public void TheGapAndTheCentralSeparator_ReachTheResolvedSystem()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                Check(w, "SideBPresentCheck").IsChecked = true;
                // I-42: la CAPACIDAD ya no declara PRESENCIA; este caso quiere el rack compuesto entero.
                DeclareSideBOnEveryFront(w, w.CompositeState.SlotCount);
                var before = w.LastComputation.System.Structure.TotalLength;

                Field(w, "GapBox").SetNumber(12.0);
                LoseFocus(Field(w, "GapBox"));
                var widened = w.LastComputation.System.Structure.TotalLength;

                Check(w, "CentralSeparatorCheck").IsChecked = true;
                return Math.Abs(widened - before - 12.0) < 1e-6
                       && w.LastComputation.System.Composite.CentralSeparator;
            });

            Assert.True(ok);
        }

        [Fact]
        public void ApplyingTopology_UsesTheSameFiveScopes_AndReachesTheSystem()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                Check(w, "SideBPresentCheck").IsChecked = true;
                // I-42: la CAPACIDAD ya no declara PRESENCIA; este caso quiere el rack compuesto entero.
                DeclareSideBOnEveryFront(w, w.CompositeState.SlotCount);
                Combo(w, "CellTopologyBox").SelectedIndex = 3;    // Corrida
                Combo(w, "RunDirectionBox").SelectedIndex = 0;    // A -> B
                Combo(w, "TopologyScopeBox").SelectedIndex = 4;   // Todo
                Click(Btn(w, "ApplyTopologyButton"));

                var runs = PushBackRuns.Resolve(w.LastComputation.System);
                return runs.Runs.Count > 0
                       && runs.Runs.All(run => run.Topology == PushBackCellTopology.Corrida)
                       && runs.Runs.All(run => run.LowSide == PushBackSide.A && run.HighSide == PushBackSide.B);
            });

            Assert.True(ok);
        }

        /// <summary>
        /// I-42 (ronda 4) — el campo de fondo por celda CAMBIA DE AUTORIDAD cuando la celda es una cama corrida: su
        /// etiqueta lo dice y lo que escribe es el fondo propio de la corrida, no el de A ni el de B.
        /// </summary>
        [Fact]
        public void TheDepthField_EditsTheCorridaDepth_WhenTheCellIsACorrida()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                var label = (TextBlock)w.FindName("CellFondoLabel");

                // Un rack de un sentido: el campo sigue siendo el fondo de la celda, como en I-41.
                var legacyLabel = label.Text;

                Check(w, "SideBPresentCheck").IsChecked = true;
                // I-42: la CAPACIDAD ya no declara PRESENCIA; este caso quiere el rack compuesto entero.
                DeclareSideBOnEveryFront(w, w.CompositeState.SlotCount);
                Combo(w, "CellTopologyBox").SelectedIndex = 3;    // Corrida
                Combo(w, "RunDirectionBox").SelectedIndex = 0;    // A -> B
                Combo(w, "TopologyScopeBox").SelectedIndex = 4;   // Todo
                Click(Btn(w, "ApplyTopologyButton"));

                var corridaLabel = label.Text;

                // Escribir en el campo y salir de el escribe la autoridad de la CORRIDA.
                var field = Field(w, "CellFondoOverrideBox");
                var deepABefore = w.CompositeState.SideA.Cell(0, 0).PalletsDeepOverride;
                field.SetNumber(6);
                LoseFocus(field);

                var wroteCorrida = w.CompositeState.CorridaDepthAt(0, 0) == 6;
                var leftSidesAlone = w.CompositeState.SideA.Cell(0, 0).PalletsDeepOverride == deepABefore;
                var reachedTheSystem = w.LastComputation.System.Composite.Cell(0, 1).Beds
                    .Single().DemandPositions == 6;

                return legacyLabel == "Fondo celda"
                       && corridaLabel == "Fondo de cama corrida"
                       && wroteCorrida
                       && leftSidesAlone
                       && reachedTheSystem;
            });

            Assert.True(ok);
        }

        /// <summary>
        /// El tooltip del campo tiene que seguir a su autoridad tambien despues de la logica de «frente en blanco»
        /// de I-33, que guarda y restaura tooltips. Antes ese guardado devolvia el texto viejo sobre el campo ya
        /// reapuntado, y el usuario leia una explicacion que no correspondia a lo que iba a escribir.
        /// </summary>
        [Fact]
        public void TheDepthFieldToolTip_FollowsItsAuthority_ThroughReloads()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                var field = Field(w, "CellFondoOverrideBox");

                Check(w, "SideBPresentCheck").IsChecked = true;
                // I-42: la CAPACIDAD ya no declara PRESENCIA; este caso quiere el rack compuesto entero.
                DeclareSideBOnEveryFront(w, w.CompositeState.SlotCount);
                Combo(w, "TopologyScopeBox").SelectedIndex = 4;   // Todo
                Combo(w, "CellTopologyBox").SelectedIndex = 3;    // Corrida
                Click(Btn(w, "ApplyTopologyButton"));

                var corridaTip = field.ToolTip as string;

                // Una recarga cualquiera del panel no puede devolver el texto anterior.
                field.SetNumber(6);
                LoseFocus(field);
                var afterReload = field.ToolTip as string;

                Combo(w, "CellTopologyBox").SelectedIndex = 2;    // Encontradas
                Click(Btn(w, "ApplyTopologyButton"));
                var cellTip = field.ToolTip as string;

                return corridaTip != null
                       && corridaTip.Contains("cama corrida")
                       && afterReload == corridaTip
                       && cellTip != null
                       && cellTip.Contains("Fondos frente")
                       && cellTip != corridaTip;
            });

            Assert.True(ok);
        }

        /// <summary>Volver la celda a encontradas devuelve el campo —y su etiqueta— al fondo por lado de I-41.</summary>
        [Fact]
        public void TheDepthField_ReturnsToTheCellDepth_WhenTheCellStopsBeingACorrida()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                var label = (TextBlock)w.FindName("CellFondoLabel");

                Check(w, "SideBPresentCheck").IsChecked = true;
                // I-42: la CAPACIDAD ya no declara PRESENCIA; este caso quiere el rack compuesto entero.
                DeclareSideBOnEveryFront(w, w.CompositeState.SlotCount);
                Combo(w, "TopologyScopeBox").SelectedIndex = 4;   // Todo
                Combo(w, "CellTopologyBox").SelectedIndex = 3;    // Corrida
                Click(Btn(w, "ApplyTopologyButton"));

                var field = Field(w, "CellFondoOverrideBox");
                field.SetNumber(6);
                LoseFocus(field);

                Combo(w, "CellTopologyBox").SelectedIndex = 2;    // Encontradas
                Click(Btn(w, "ApplyTopologyButton"));

                // El fondo de la corrida NO se borra: queda dormante, listo para cuando vuelva a serlo.
                return label.Text == "Fondo celda"
                       && w.CompositeState.CorridaDepthAt(0, 0) == 6
                       && w.LastComputation.System.Composite.Cell(0, 1).Beds.Count == 2;
            });

            Assert.True(ok);
        }

        // ================= MULTIFRENTE / MULTINIVEL desde la VENTANA REAL ======================================

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        /// <summary>
        /// La operacion exacta del dueño: N frentes y N niveles, aplicados a TODO. La retícula es del RACK, asi que
        /// los dos lados crecen a la vez. Antes crecia solo el lado activo, y el resultado era un rack cuyo primer
        /// frente tenia las dos mitades y los demas solo una.
        /// </summary>
        private static RackPushBackSystemWindow MultiFront(int fronts = 4, int levels = 3)
        {
            var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
            Check(w, "SideBPresentCheck").IsChecked = true;
            // I-42: la CAPACIDAD ya no declara PRESENCIA; este caso quiere el rack compuesto entero.
            DeclareSideBOnEveryFront(w, w.CompositeState.SlotCount);

            var frontCount = Field(w, "FrontCountBox");
            frontCount.SetNumber(fronts);
            LoseFocus(frontCount);

            foreach (var side in new[] { 0, 1 })
            {
                Combo(w, "SideSelectorBox").SelectedIndex = side;
                var levelBox = Field(w, "LevelsBox");
                levelBox.SetNumber(levels);
                LoseFocus(levelBox);
                Click(Btn(w, "ApplyAllButton"));
            }

            // I-42 (ronda post-5a73b92): declarar la CAPACIDAD del lado B ya no lo declara PRESENTE en ningun
            // frente. Este fixture quiere el rack compuesto entero, asi que lo declara frente a frente CON LOS
            // CONTROLES REALES — que es ademas la operacion que el dueño no podia hacer.
            DeclareSideBOnEveryFront(w, fronts);
            Combo(w, "SideSelectorBox").SelectedIndex = 0;
            return w;
        }

        /// <summary>
        /// Declara el lado B en todos los frentes SIN tocar el selector de lado ni la celda seleccionada: para un
        /// caso cuyo asunto no es la presencia, mover el cursor o el lado activo cambiaria lo que se esta midiendo.
        /// La operacion CON LOS CONTROLES REALES la ejercita <c>SideB_CanBeDeclaredFrontByFront_FromTheWindow</c>.
        /// </summary>
        private static void DeclareSideBOnEveryFront(RackPushBackSystemWindow w, int fronts)
        {
            // Se escribe la intencion DIRECTAMENTE sobre la matriz del lado B: pasar por el control movería la celda
            // seleccionada, y para un caso cuyo asunto no es la presencia eso cambiaria lo que se esta midiendo.
            var matrix = w.CompositeState.Of(PushBackSide.B).Structure;
            for (var front = 0; front < Math.Min(Math.Max(fronts, w.CompositeState.SlotCount), matrix.Count); front++)
            {
                matrix.Fronts[front].IsActive = true;
            }

            // El modelo cambio por fuera de la ventana, asi que hay que pedirle que recalcule: se hace con el mismo
            // control real que usa el usuario, reescribiendo el hueco con su propio valor.
            var gap = Field(w, "GapBox");
            gap.SetNumber(w.CompositeState.Gap);
            LoseFocus(gap);
        }

        [Fact]
        public void FourFrontsAndThreeLevels_ReachTheDrawing_InEveryFront()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = MultiFront();
                var state = w.CompositeState;

                // Los DOS lados crecieron: la retícula es una sola.
                if (state.SideA.Structure.Count != 4 || state.SideB.Structure.Count != 4)
                {
                    return false;
                }

                for (var front = 0; front < 4; front++)
                {
                    if (state.SideA.Structure.Fronts[front].LoadLevels != 3) return false;
                    if (state.SideB.Structure.Fronts[front].LoadLevels != 3) return false;
                }

                var system = w.LastComputation.System;
                var runs = PushBackRuns.Resolve(system);

                // 4 frentes x 3 niveles x 2 camas encontradas: cada frente aporta SEIS, ninguno cero.
                for (var front = 0; front < 4; front++)
                {
                    if (runs.Runs.Count(run => run.Slot == front) != 6) return false;
                    for (var level = 1; level <= 3; level++)
                    {
                        var cell = system.Composite.Cell(front, level);
                        if (cell == null || cell.Beds.Count != 2) return false;
                    }
                }

                return runs.Runs.Count == 24 && w.LastComputation.IsValid;
            });

            Assert.True(ok);
        }

        // ================= El FONDO de UNA celda ================================================================

        /// <summary>
        /// El alcance «Celda» escribe UNA celda. Se comprueban las DOCE, no solo la elegida: una prueba que solo
        /// mirara la celda escrita pasaria igual si el editor las hubiera escrito todas.
        /// </summary>
        [Fact]
        public void TheCellScope_ChangesExactlyOneCell_AndTheDrawingFollows()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = MultiFront();
                Combo(w, "SelectedFrontBox").SelectedIndex = 1;      // F2
                Combo(w, "SelectedLevelBox").SelectedIndex = 1;      // L2
                Combo(w, "CellPropertyScopeBox").SelectedIndex = 0;  // Celda

                var field = Field(w, "CellFondoOverrideBox");
                field.SetNumber(3);
                LoseFocus(field);

                var state = w.CompositeState;
                for (var front = 0; front < 4; front++)
                {
                    for (var level = 0; level < 3; level++)
                    {
                        var expected = front == 1 && level == 1 ? (int?)3 : null;
                        if (state.SideA.Cell(front, level).PalletsDeepOverride != expected) return false;
                        if (state.SideB.Cell(front, level).PalletsDeepOverride != null) return false;
                    }
                }

                // Y el DIBUJO lo refleja: una sola cama mas corta entre las veinticuatro.
                var system = w.LastComputation.System;
                var shortBeds = 0;
                for (var front = 0; front < 4; front++)
                {
                    for (var level = 1; level <= 3; level++)
                    {
                        if (system.Composite.Cell(front, level).BedFrom(PushBackSide.A).DemandPositions == 3)
                        {
                            shortBeds++;
                        }
                    }
                }

                return shortBeds == 1;
            });

            Assert.True(ok);
        }

        /// <summary>Los cinco alcances, cada uno escribiendo EXACTAMENTE su conjunto.</summary>
        [Fact]
        public void TheFiveDepthScopes_WriteExactlyTheirTargets()
        {
            var ok = StaTestRunner.Run(() =>
            {
                bool Matches(RackPushBackSystemWindow window, Func<int, int, bool> expected)
                {
                    for (var front = 0; front < 4; front++)
                    {
                        for (var level = 0; level < 3; level++)
                        {
                            var has = window.CompositeState.SideA.Cell(front, level).PalletsDeepOverride == 3;
                            if (has != expected(front, level)) return false;
                        }
                    }

                    return true;
                }

                void Write(RackPushBackSystemWindow window, int scope)
                {
                    Combo(window, "CellPropertyScopeBox").SelectedIndex = scope;
                    var box = Field(window, "CellFondoOverrideBox");
                    box.SetNumber(3);
                    LoseFocus(box);
                    Click(Btn(window, "ApplyCellFondoButton"));
                }

                // Nivel: ese nivel en todos los frentes.
                var byLevel = MultiFront();
                Combo(byLevel, "SelectedFrontBox").SelectedIndex = 1;
                Combo(byLevel, "SelectedLevelBox").SelectedIndex = 2;
                Write(byLevel, 2);
                if (!Matches(byLevel, (f, l) => l == 2)) return false;

                // Frente: todos los niveles del frente seleccionado.
                var byFront = MultiFront();
                Combo(byFront, "SelectedFrontBox").SelectedIndex = 2;
                Combo(byFront, "SelectedLevelBox").SelectedIndex = 0;
                Write(byFront, 3);
                if (!Matches(byFront, (f, l) => f == 2)) return false;

                // Todo.
                var all = MultiFront();
                Write(all, 4);
                if (!Matches(all, (f, l) => true)) return false;

                // Y restaurar respeta el MISMO alcance: vacia solo el frente seleccionado.
                Combo(all, "SelectedFrontBox").SelectedIndex = 0;
                Combo(all, "CellPropertyScopeBox").SelectedIndex = 3;
                Click(Btn(all, "RestoreCellFondoButton"));
                return Matches(all, (f, l) => f != 0);
            });

            Assert.True(ok);
        }

        // ================= PRESENCIA DE B, FRENTE A FRENTE, CON LOS CONTROLES REALES ============================

        /// <summary>
        /// EL BLOQUEO DE ESTA RONDA. El dueño no podía declarar el lado B más que en el primer frente: al cambiar
        /// el selector de lado, el cursor de celda saltaba a la celda que ESE lado tenía seleccionada —la 1—, así que
        /// la casilla de presencia escribía siempre en la ranura 0.
        ///
        /// <para>
        /// Se reproduce la operación EXACTA: elegir el frente mirando el rack (lado A), cambiar al lado B y declarar.
        /// Cuatro frentes, uno por uno. No vale probar el estado directamente: el defecto vivía en el paso entre los
        /// dos controles.
        /// </para>
        /// </summary>
        [Fact]
        public void SideB_CanBeDeclaredFrontByFront_FromTheWindow()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                var frontCount = Field(w, "FrontCountBox");
                frontCount.SetNumber(4);
                LoseFocus(frontCount);
                Check(w, "SideBPresentCheck").IsChecked = true;

                for (var front = 0; front < 4; front++)
                {
                    // El usuario elige el frente MIRANDO EL RACK, que es el lado A.
                    Combo(w, "SideSelectorBox").SelectedIndex = 0;
                    Combo(w, "SelectedFrontBox").SelectedIndex = front;

                    // Y despues cambia al lado B para declararlo. El cursor tiene que seguirle.
                    Combo(w, "SideSelectorBox").SelectedIndex = 1;
                    if (Combo(w, "SelectedFrontBox").SelectedIndex != front) return false;
                    if (w.CompositeState.Active.Structure.SelectedFrontIndex != front) return false;

                    var box = Blank(w, front);
                    if (box == null || !box.IsEnabled) return false;
                    box.IsChecked = false;   // «En blanco» DESMARCADO = el frente existe en este lado

                    // Se declaro ESE frente, y solo hasta ese: los siguientes siguen sin lado B.
                    for (var other = 0; other < 4; other++)
                    {
                        if (w.CompositeState.IsSlotPresent(PushBackSide.B, other) != (other <= front)) return false;
                    }
                }

                return w.LastComputation.IsValid && w.LastComputation.System.IsComposite;
            });

            Assert.True(ok);
        }

        /// <summary>
        /// Y se puede QUITAR de un frente intermedio y volver a ponerlo, sin tocar a los demas: presencia NO
        /// CONTIGUA, que es lo que el contrato de I-42 permite y lo que el dueño pidio poder hacer.
        /// </summary>
        [Fact]
        public void SideB_CanBeRemovedAndRestored_OnAnIntermediateFront()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = MultiFront(fronts: 4, levels: 2);
                Combo(w, "SideSelectorBox").SelectedIndex = 1;

                Combo(w, "SelectedFrontBox").SelectedIndex = 1;
                SetBlank(w, 1, true);
                var removed = !w.CompositeState.IsSlotPresent(PushBackSide.B, 1)
                              && w.CompositeState.IsSlotPresent(PushBackSide.B, 0)
                              && w.CompositeState.IsSlotPresent(PushBackSide.B, 2)
                              && w.CompositeState.IsSlotPresent(PushBackSide.B, 3);

                SetBlank(w, 1, false);
                return removed
                       && w.CompositeState.IsSlotPresent(PushBackSide.B, 1)
                       && w.LastComputation.IsValid;
            });

            Assert.True(ok);
        }

        /// <summary>
        /// Cambiar de lado NO mueve el cursor: la celda seleccionada es una posicion FISICA del rack y existe en los
        /// dos lados. Es la causa raiz del bloqueo, aislada.
        /// </summary>
        [Fact]
        public void SwitchingSides_KeepsTheSelectedCell()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = MultiFront(fronts: 4, levels: 3);
                Combo(w, "SideSelectorBox").SelectedIndex = 0;
                Combo(w, "SelectedFrontBox").SelectedIndex = 2;
                Combo(w, "SelectedLevelBox").SelectedIndex = 1;

                Combo(w, "SideSelectorBox").SelectedIndex = 1;
                var kept = w.CompositeState.Active.Structure.SelectedFrontIndex == 2
                           && w.CompositeState.Active.Structure.SelectedLevelIndex == 1
                           && Combo(w, "SelectedFrontBox").SelectedIndex == 2;

                Combo(w, "SideSelectorBox").SelectedIndex = 0;
                return kept
                       && w.CompositeState.Active.Structure.SelectedFrontIndex == 2
                       && w.CompositeState.Active.Structure.SelectedLevelIndex == 1;
            });

            Assert.True(ok);
        }

        // ================= TOPES A / B desde la ventana =========================================================

        /// <summary>
        /// Las cuatro combinaciones de tope se deciden EN LA VENTANA, sin deducir nada del modo activo. Es el
        /// requisito que el dueño no encontro.
        /// </summary>
        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public void TheTwoTopeChecks_DecideEachSide(bool topeA, bool topeB)
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = MultiFront(fronts: 2, levels: 2);
                Combo(w, "CellTopologyBox").SelectedIndex = 2;    // Encontradas
                Combo(w, "TopologyScopeBox").SelectedIndex = 4;
                Click(Btn(w, "ApplyTopologyButton"));

                Check(w, "TopeSideACheck").IsChecked = topeA;
                Check(w, "TopeSideBCheck").IsChecked = topeB;
                Combo(w, "TopeScopeBox").SelectedIndex = 4;       // Todo
                Click(Btn(w, "ApplyTopesButton"));

                var state = w.CompositeState;
                for (var front = 0; front < 2; front++)
                {
                    for (var level = 0; level < 2; level++)
                    {
                        if (state.RearTopeAt(PushBackSide.A, front, level) != topeA) return false;
                        if (state.RearTopeAt(PushBackSide.B, front, level) != topeB) return false;
                    }
                }

                var runs = PushBackRuns.Resolve(w.LastComputation.System);
                var expected = runs.Runs.Count(run => run.HighSide == PushBackSide.A ? topeA : topeB);
                var quoted = PushBackBomBuilder.Build(w.LastComputation.System, Catalog).Components
                    .Where(component => component.Category == PushBackBomBuilder.RearTope)
                    .Sum(component => component.Quantity);

                return quoted == expected;
            });

            Assert.True(ok);
        }

        /// <summary>
        /// Una celda con UNA sola cama solo tiene un extremo alto, asi que la casilla del otro lado se deshabilita
        /// CON SU MOTIVO — y lo que ya estuviera elegido se conserva.
        /// </summary>
        [Fact]
        public void TheTopeChecks_DisableTheSideThatCannotCarryOne()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = MultiFront(fronts: 2, levels: 2);
                Check(w, "TopeSideACheck").IsChecked = true;
                Check(w, "TopeSideBCheck").IsChecked = true;
                Combo(w, "TopeScopeBox").SelectedIndex = 4;
                Click(Btn(w, "ApplyTopesButton"));

                Combo(w, "CellTopologyBox").SelectedIndex = 3;    // Corrida
                Combo(w, "RunDirectionBox").SelectedIndex = 0;    // A -> B
                Combo(w, "TopologyScopeBox").SelectedIndex = 4;
                Click(Btn(w, "ApplyTopologyButton"));

                var a = Check(w, "TopeSideACheck");
                var b = Check(w, "TopeSideBCheck");

                // El alto es B: A no puede llevar tope, pero su eleccion sigue viva y marcada.
                return !a.IsEnabled
                       && b.IsEnabled
                       && a.IsChecked == true
                       && w.CompositeState.RearTopeAt(PushBackSide.A, 0, 0)
                       && ((string)a.ToolTip).Contains("INTENCION GUARDADA");
            });

            Assert.True(ok);
        }

        // ================= ERROR 10: intencion, aplicabilidad y DONDE cae la pieza ==============================

        /// <summary>
        /// La casilla dice INTENCION y el tooltip separa los dos hechos: si hoy es EFECTIVA, y si no, por que. Son
        /// cosas distintas y confundirlas era la mitad del error 10.
        /// </summary>
        [Fact]
        public void TheTopeTooltips_SeparateIntentFromEffectiveness()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = MultiFront(fronts: 2, levels: 2);
                Combo(w, "CellTopologyBox").SelectedIndex = 2;    // Encontradas
                Combo(w, "TopologyScopeBox").SelectedIndex = 4;
                Click(Btn(w, "ApplyTopologyButton"));

                var encontradasA = (string)Check(w, "TopeSideACheck").ToolTip;
                var encontradasB = (string)Check(w, "TopeSideBCheck").ToolTip;

                Combo(w, "CellTopologyBox").SelectedIndex = 3;    // Corrida
                Combo(w, "RunDirectionBox").SelectedIndex = 0;    // A -> B
                Click(Btn(w, "ApplyTopologyButton"));

                var corridaA = (string)Check(w, "TopeSideACheck").ToolTip;
                var corridaB = (string)Check(w, "TopeSideBCheck").ToolTip;

                return encontradasA.Contains("EFECTIVO")
                       && encontradasB.Contains("EFECTIVO")
                       && corridaA.Contains("INTENCION GUARDADA")
                       && corridaA.Contains("corrida")
                       && corridaB.Contains("EFECTIVO");
            });

            Assert.True(ok);
        }

        /// <summary>
        /// El texto contextual permite PREDECIR la planta sin conocer la implementacion: dice que topologia tiene la
        /// celda, que lado es efectivo y DONDE cae la pieza. Con camas encontradas topan en el centro; una corrida
        /// topa al final de su recorrido, en la orilla opuesta a su pasillo de carga.
        /// </summary>
        [Fact]
        public void TheTopeText_SaysWhereThePieceWillLand()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = MultiFront(fronts: 2, levels: 2);
                var text = (TextBlock)w.FindName("TopeApplicabilityText");

                Combo(w, "CellTopologyBox").SelectedIndex = 2;    // Encontradas
                Combo(w, "TopologyScopeBox").SelectedIndex = 4;
                Click(Btn(w, "ApplyTopologyButton"));
                var encontradas = text.Text;

                Combo(w, "CellTopologyBox").SelectedIndex = 3;    // Corrida
                Combo(w, "RunDirectionBox").SelectedIndex = 0;    // A -> B
                Click(Btn(w, "ApplyTopologyButton"));
                var aToB = text.Text;

                Combo(w, "RunDirectionBox").SelectedIndex = 1;    // B -> A
                Click(Btn(w, "ApplyTopologyButton"));
                var bToA = text.Text;

                return encontradas.Contains("INDEPENDIENTES")
                       && encontradas.Contains("INTERIOR")
                       && aToB.Contains("A->B") && aToB.Contains("EFECTIVO") && aToB.Contains("EXTERIOR")
                       && bToA.Contains("B->A") && bToA.Contains("EXTERIOR")
                       && aToB != bToA;
            });

            Assert.True(ok);
        }

        /// <summary>
        /// Cambiar el SENTIDO mueve la efectividad de un lado al otro sin borrar ninguna intencion: las dos siguen
        /// marcadas y las dos siguen guardadas. Es la pregunta 7 del dueno.
        /// </summary>
        [Fact]
        public void ChangingTheDirection_MovesEffectiveness_WithoutErasingIntent()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = MultiFront(fronts: 2, levels: 2);
                Check(w, "TopeSideACheck").IsChecked = true;
                Check(w, "TopeSideBCheck").IsChecked = true;
                Combo(w, "TopeScopeBox").SelectedIndex = 4;
                Click(Btn(w, "ApplyTopesButton"));

                Combo(w, "CellTopologyBox").SelectedIndex = 3;    // Corrida
                Combo(w, "TopologyScopeBox").SelectedIndex = 4;
                Combo(w, "RunDirectionBox").SelectedIndex = 0;    // A -> B
                Click(Btn(w, "ApplyTopologyButton"));
                var aEffectiveFirst = Check(w, "TopeSideACheck").IsEnabled;
                var bEffectiveFirst = Check(w, "TopeSideBCheck").IsEnabled;

                Combo(w, "RunDirectionBox").SelectedIndex = 1;    // B -> A
                Click(Btn(w, "ApplyTopologyButton"));

                var state = w.CompositeState;
                return !aEffectiveFirst && bEffectiveFirst
                       && Check(w, "TopeSideACheck").IsEnabled
                       && !Check(w, "TopeSideBCheck").IsEnabled
                       // Y NINGUNA intencion se perdio por el camino.
                       && state.RearTopeAt(PushBackSide.A, 0, 0)
                       && state.RearTopeAt(PushBackSide.B, 0, 0)
                       && Check(w, "TopeSideACheck").IsChecked == true
                       && Check(w, "TopeSideBCheck").IsChecked == true;
            });

            Assert.True(ok);
        }

        /// <summary>
        /// Los CINCO alcances de topes escriben exactamente lo que dicen: celda, seleccion, nivel, frente y todo.
        /// Es la pregunta 3 del dueno, sobre los controles reales.
        /// </summary>
        [Theory]
        [InlineData(0, 1)]   // Celda
        [InlineData(2, 3)]   // Nivel: las 3 ranuras de ese nivel
        [InlineData(3, 2)]   // Frente: los 2 niveles de esa ranura
        [InlineData(4, 6)]   // Todo
        public void TheFiveTopeScopes_WriteExactlyWhatTheySay(int scopeIndex, int expected)
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = MultiFront(fronts: 3, levels: 2);
                Combo(w, "CellTopologyBox").SelectedIndex = 2;    // Encontradas: las dos casillas aplican
                Combo(w, "TopologyScopeBox").SelectedIndex = 4;
                Click(Btn(w, "ApplyTopologyButton"));

                // Se parte de TODO apagado para que el conteo sea el de lo escrito ahora.
                Check(w, "TopeSideACheck").IsChecked = false;
                Check(w, "TopeSideBCheck").IsChecked = false;
                Combo(w, "TopeScopeBox").SelectedIndex = 4;
                Click(Btn(w, "ApplyTopesButton"));

                Check(w, "TopeSideACheck").IsChecked = true;
                Combo(w, "TopeScopeBox").SelectedIndex = scopeIndex;
                Click(Btn(w, "ApplyTopesButton"));

                var state = w.CompositeState;
                var written = 0;
                for (var front = 0; front < 3; front++)
                {
                    for (var level = 0; level < 2; level++)
                    {
                        if (state.RearTopeAt(PushBackSide.A, front, level))
                        {
                            written++;
                        }
                    }
                }

                return written == expected;
            });

            Assert.True(ok);
        }

        // ================= PRESENCIA de la ranura ===============================================================

        /// <summary>
        /// A = 3 y B = 4 se declara con «En blanco» sobre UNA sola retícula: el cuarto frente queda en blanco en el
        /// lado A y sigue existiendo en el B. Es la unica autoridad visible desde la ronda post-82e918b.
        /// </summary>
        [Fact]
        public void TheBlankCheck_DeclaresAnAsymmetricRack()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = MultiFront(fronts: 4, levels: 2);
                Combo(w, "SideSelectorBox").SelectedIndex = 0;   // el lado A es el que pierde el frente
                Combo(w, "SelectedFrontBox").SelectedIndex = 3;
                SetBlank(w, 3, true);

                var state = w.CompositeState;
                if (state.IsSlotPresent(PushBackSide.A, 3) || !state.IsSlotPresent(PushBackSide.B, 3))
                {
                    return false;
                }

                var system = w.LastComputation.System;
                // UNA sola estructura de cuatro ranuras; la cuarta solo existe en B.
                return system.Structure.Fronts.Count == 4
                       && system.Composite.Cell(3, 1).Topology == PushBackCellTopology.SoloB
                       && system.Composite.Cell(2, 1).Topology == PushBackCellTopology.Encontradas;
            });

            Assert.True(ok);
        }

        /// <summary>
        /// Un lado puede quedarse ENTERO en blanco mientras el otro sostenga el rack —es la capacidad declarada y
        /// sin usar—, pero dejar el rack sin ningun frente se REHUSA y la casilla vuelve a su estado real.
        /// </summary>
        [Fact]
        public void TheBlankCheck_RefusesToLeaveTheWholeRackBlank()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = MultiFront(fronts: 1, levels: 2);

                // El lado B entero en blanco: legal, el lado A sostiene el rack.
                Combo(w, "SideSelectorBox").SelectedIndex = 1;
                SetBlank(w, 0, true);
                var sideBBlank = !w.CompositeState.IsSlotPresent(PushBackSide.B, 0)
                                 && w.CompositeState.IsSlotPresent(PushBackSide.A, 0);

                // Y ahora tambien el lado A: eso dejaria el rack vacio y se rehusa.
                Combo(w, "SideSelectorBox").SelectedIndex = 0;
                SetBlank(w, 0, true);

                return sideBBlank
                       && w.CompositeState.IsSlotPresent(PushBackSide.A, 0)
                       && Blank(w, 0).IsChecked != true;
            });

            Assert.True(ok);
        }

        // ================= T: la UX de LADO A / LADO B / AMBOS ==================================================

        private static int Levels(RackPushBackSystemWindow w, PushBackSide side, int front)
            => w.CompositeState.Of(side).Structure.Fronts[front].LoadLevels;

        private static int FrontDepth(RackPushBackSystemWindow w, PushBackSide side, int front)
            => w.CompositeState.Of(side).Structure.Fronts[front].PalletsDeep;

        private static void WriteFrontFields(RackPushBackSystemWindow w, int? levels = null, int? depth = null)
        {
            if (levels.HasValue)
            {
                var box = Field(w, "LevelsBox");
                box.SetNumber(levels.Value);
                LoseFocus(box);
            }

            if (depth.HasValue)
            {
                var box = Field(w, "FondosBox");
                box.SetNumber(depth.Value);
                LoseFocus(box);
            }

            Click(Btn(w, "ApplyFrontButton"));
        }

        /// <summary>El selector ofrece los TRES: lado A, lado B y ambos. «Ambos» es edicion, no un tercer lado.</summary>
        [Fact]
        public void TheSideSelector_OffersBothSidesAndBoth()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = MultiFront(fronts: 2, levels: 2);
                var selector = Combo(w, "SideSelectorBox");
                var items = ((IEnumerable<string>)selector.ItemsSource).ToList();

                selector.SelectedIndex = 2;
                var both = w.CompositeState.ActiveSelection == PushBackSideSelection.Both
                           && w.CompositeState.ActiveSide == PushBackSide.A;

                selector.SelectedIndex = 1;
                var onlyB = w.CompositeState.ActiveSelection == PushBackSideSelection.B
                            && w.CompositeState.ActiveSide == PushBackSide.B;

                return items.Count == 3 && both && onlyB;
            });

            Assert.True(ok);
        }

        /// <summary>Con un lado seleccionado, los campos del frente escriben SOLO en ese lado.</summary>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void EditingOneSide_LeavesTheOtherUntouched(int sideIndex)
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = MultiFront(fronts: 2, levels: 2);
                Combo(w, "SideSelectorBox").SelectedIndex = sideIndex;

                var edited = sideIndex == 1 ? PushBackSide.B : PushBackSide.A;
                var other = sideIndex == 1 ? PushBackSide.A : PushBackSide.B;
                var beforeOther = Levels(w, other, 0);

                WriteFrontFields(w, levels: 4);

                return Levels(w, edited, 0) == 4 && Levels(w, other, 0) == beforeOther;
            });

            Assert.True(ok);
        }

        /// <summary>Con «Ambos», la MISMA intencion se escribe en A y en B en una sola accion.</summary>
        [Fact]
        public void EditingBoth_WritesTheSameIntentOnBothSides()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = MultiFront(fronts: 2, levels: 2);
                Combo(w, "SideSelectorBox").SelectedIndex = 2;   // Ambos

                WriteFrontFields(w, levels: 4, depth: 5);

                return Levels(w, PushBackSide.A, 0) == 4
                       && Levels(w, PushBackSide.B, 0) == 4
                       && FrontDepth(w, PushBackSide.A, 0) == 5
                       && FrontDepth(w, PushBackSide.B, 0) == 5;
            });

            Assert.True(ok);
        }

        /// <summary>
        /// Con «Ambos» y valores DISTINTOS, el campo no miente: se muestra vacio. Escribir uno lo aplica a los dos, y
        /// dejarlo vacio conserva el de cada lado.
        /// </summary>
        [Fact]
        public void WithBothSelected_ADifferingField_ShowsBlank_AndWritingItAppliesToBoth()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = MultiFront(fronts: 2, levels: 3);

                // A = 3 niveles, B = 4.
                Combo(w, "SideSelectorBox").SelectedIndex = 1;
                WriteFrontFields(w, levels: 4);
                Combo(w, "SideSelectorBox").SelectedIndex = 0;

                if (Levels(w, PushBackSide.A, 0) != 3 || Levels(w, PushBackSide.B, 0) != 4) return false;

                // Ambos: el campo de niveles queda VACIO, ni 3 ni 4.
                Combo(w, "SideSelectorBox").SelectedIndex = 2;
                var levelBox = Field(w, "LevelsBox");
                if (!string.IsNullOrEmpty(levelBox.Text)) return false;
                if (levelBox.HasError) return false;   // vacio legitimo, no un error que bloquee la edicion

                // Vacio = cada lado conserva el suyo.
                Click(Btn(w, "ApplyFrontButton"));
                if (Levels(w, PushBackSide.A, 0) != 3 || Levels(w, PushBackSide.B, 0) != 4) return false;

                // Y escribir 5 lo aplica a los DOS.
                WriteFrontFields(w, levels: 5);
                return Levels(w, PushBackSide.A, 0) == 5 && Levels(w, PushBackSide.B, 0) == 5;
            });

            Assert.True(ok);
        }

        // ================= H: fondo del frente y ajuste de estructura son DOS autoridades =======================

        /// <summary>
        /// «Fondos frente» es el fondo BASE de almacenamiento de ese frente, por lado. El «ajuste manual» de la
        /// seccion de estructura es hasta donde llega la estructura del lado. Son dos cosas distintas y ninguna
        /// escribe en la otra: es justo la confusion que el dueño reporto.
        /// </summary>
        [Fact]
        public void TheFrontDepth_AndTheStructureOverride_AreTwoSeparateAuthorities()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = MultiFront(fronts: 2, levels: 2);

                // 1) Fijar el ajuste manual de estructura NO cambia el fondo base de ningun frente.
                var depthBefore = FrontDepth(w, PushBackSide.A, 0);
                var overrideBox = Field(w, "StructureOverrideBox");
                overrideBox.SetNumber(9);
                LoseFocus(overrideBox);
                Click(Btn(w, "ApplyStructureButton"));

                if (w.CompositeState.StructureOverride(PushBackSide.A) != 9) return false;
                if (FrontDepth(w, PushBackSide.A, 0) != depthBefore) return false;

                // 2) Y cambiar el fondo base NO crea ni mueve el ajuste manual.
                WriteFrontFields(w, depth: 4);
                if (FrontDepth(w, PushBackSide.A, 0) != 4) return false;
                if (w.CompositeState.StructureOverride(PushBackSide.A) != 9) return false;

                // 3) Volver a automatico retira SOLO el ajuste.
                Click(Btn(w, "RestoreStructureButton"));
                return w.CompositeState.StructureOverride(PushBackSide.A) == null
                       && FrontDepth(w, PushBackSide.A, 0) == 4;
            });

            Assert.True(ok);
        }

        /// <summary>El fondo base es POR LADO: cambiarlo en A no toca el de B.</summary>
        [Fact]
        public void TheFrontDepth_IsPerSide()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = MultiFront(fronts: 2, levels: 2);

                Combo(w, "SideSelectorBox").SelectedIndex = 0;
                WriteFrontFields(w, depth: 5);
                Combo(w, "SideSelectorBox").SelectedIndex = 1;
                WriteFrontFields(w, depth: 7);

                return FrontDepth(w, PushBackSide.A, 0) == 5 && FrontDepth(w, PushBackSide.B, 0) == 7;
            });

            Assert.True(ok);
        }

        // ================= La seccion compuesta y las vistas ====================================================

        /// <summary>
        /// Con «Ambos», los controles que por definicion son de UN lado —la presencia del frente y el ajuste de
        /// estructura— se deshabilitan CON SU MOTIVO, en vez de escribir en A a escondidas.
        /// </summary>
        [Fact]
        public void WithBothSelected_ThePerSideControls_AreDisabledWithAReason()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = MultiFront(fronts: 2, levels: 2);
                var presence = Field(w, "StructureOverrideBox");
                var structure = Field(w, "StructureOverrideBox");

                if (!presence.IsEnabled || !structure.IsEnabled) return false;

                Combo(w, "SideSelectorBox").SelectedIndex = 2;   // Ambos
                if (presence.IsEnabled || structure.IsEnabled) return false;
                if (!((string)presence.ToolTip).Contains("Lado A")) return false;

                // Y al volver a un lado concreto se recuperan.
                Combo(w, "SideSelectorBox").SelectedIndex = 1;
                return presence.IsEnabled && structure.IsEnabled;
            });

            Assert.True(ok);
        }

        /// <summary>
        /// Añadir un frente con el boton hace lo mismo que escribir el numero: crece la RETICULA, no un lado. Antes
        /// crecia solo el lado activo y el rack acababa a medias.
        /// </summary>
        [Fact]
        public void TheAddFrontButton_GrowsTheGrid_NotOneSide()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = MultiFront(fronts: 2, levels: 2);
                Combo(w, "SideSelectorBox").SelectedIndex = 1;   // editando B
                Click(Btn(w, "AddFrontButton"));

                return w.CompositeState.SideA.Structure.Count == 3
                       && w.CompositeState.SideB.Structure.Count == 3;
            });

            Assert.True(ok);
        }

        /// <summary>
        /// PRUEBA VINCULANTE (J). «Frente seleccionado» muestra los datos del lado ELEGIDO, campo por campo. El
        /// fixture pone A y B deliberadamente distintos en TODOS los valores, asi que un solo campo que se lea del
        /// lado equivocado falla.
        /// </summary>
        [Fact]
        public void TheSelectedFrontPanel_ShowsTheChosenSide_FieldByField()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = MultiFront(fronts: 2, levels: 2);

                // A y B distintos en los cinco campos del frente.
                Write(w, 0, positions: 2, levels: 3, depth: 5, start: 1, first: 8);
                Write(w, 1, positions: 3, levels: 2, depth: 7, start: 1, first: 12);

                // Con A elegido, el panel es el de A...
                Combo(w, "SideSelectorBox").SelectedIndex = 0;
                if (!Shows(w, positions: 2, levels: 3, depth: 5, first: 8)) return false;

                // ...y con B elegido, el de B. Ni un campo se queda con el valor del otro.
                Combo(w, "SideSelectorBox").SelectedIndex = 1;
                if (!Shows(w, positions: 3, levels: 2, depth: 7, first: 12)) return false;

                // Y volver a A lo devuelve intacto: no hay valores rancios de la visita anterior.
                Combo(w, "SideSelectorBox").SelectedIndex = 0;
                return Shows(w, positions: 2, levels: 3, depth: 5, first: 8);
            });

            Assert.True(ok);
        }

        /// <summary>
        /// Y el ESTADO recibe lo que el panel dice: editar con B elegido escribe en B, y el lado A no se entera. Es
        /// la otra mitad del contrato — leer de un lado y escribir en otro es lo que corrompe los datos.
        /// </summary>
        [Fact]
        public void EditingWithSideBSelected_WritesOnlySideB()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = MultiFront(fronts: 2, levels: 2);
                Write(w, 0, positions: 2, levels: 3, depth: 5, start: 1, first: 8);
                Write(w, 1, positions: 3, levels: 2, depth: 7, start: 1, first: 12);

                var state = w.CompositeState;
                var a = state.SideA.Structure.Fronts[0];
                var b = state.SideB.Structure.Fronts[0];

                return a.PalletCount == 2 && a.LoadLevels == 3 && a.PalletsDeep == 5
                       && Math.Abs(a.FirstLevelHeight - 8.0) < 1e-6
                       && b.PalletCount == 3 && b.LoadLevels == 2 && b.PalletsDeep == 7
                       && Math.Abs(b.FirstLevelHeight - 12.0) < 1e-6;
            });

            Assert.True(ok);
        }

        /// <summary>Escribe los cinco campos del frente en el lado indicado y los aplica a todos sus frentes.</summary>
        private static void Write(
            RackPushBackSystemWindow w, int sideIndex, int positions, int levels, int depth, int start, double first)
        {
            Combo(w, "SideSelectorBox").SelectedIndex = sideIndex;
            Set(w, "PositionsBox", positions);
            Set(w, "LevelsBox", levels);
            Set(w, "FondosBox", depth);
            Set(w, "DepthStartBox", start);
            Set(w, "FirstLevelHeightBox", first);
            Click(Btn(w, "ApplyAllButton"));
        }

        private static void Set(RackPushBackSystemWindow w, string name, double value)
        {
            var box = Field(w, name);
            box.SetNumber(value);
            LoseFocus(box);
        }

        /// <summary>Lo que el panel MUESTRA ahora mismo, campo por campo.</summary>
        private static bool Shows(
            RackPushBackSystemWindow w, int positions, int levels, int depth, double first)
            => Field(w, "PositionsBox").Value == positions
               && Field(w, "LevelsBox").Value == levels
               && Field(w, "FondosBox").Value == depth
               && Field(w, "FirstLevelHeightBox").Value.HasValue
               && Math.Abs(Field(w, "FirstLevelHeightBox").Value.Value - first) < 1e-6;

        /// <summary>Con el compuesto APAGADO, ningun control exclusivo de A/B ocupa la pantalla.</summary>
        [Fact]
        public void WithTheCompositeOff_TheSideOnlyControls_AreHidden()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);

                var hiddenBefore = Section(w).Visibility == Visibility.Collapsed
                                   && Combo(w, "FrontalSideBox").Visibility == Visibility.Collapsed;

                Check(w, "SideBPresentCheck").IsChecked = true;
                // I-42: la CAPACIDAD ya no declara PRESENCIA; este caso quiere el rack compuesto entero.
                DeclareSideBOnEveryFront(w, w.CompositeState.SlotCount);
                var shownAfter = Section(w).Visibility == Visibility.Visible
                                 && Combo(w, "FrontalSideBox").Visibility == Visibility.Visible;

                Check(w, "SideBPresentCheck").IsChecked = false;
                var hiddenAgain = Section(w).Visibility == Visibility.Collapsed
                                  && Combo(w, "FrontalSideBox").Visibility == Visibility.Collapsed;

                return hiddenBefore && shownAfter && hiddenAgain;
            });

            Assert.True(ok);
        }

        /// <summary>
        /// Los CUATRO cortes frontales son pedibles y su lado es EXPLICITO: lo dice su propio selector, no el lado
        /// que se este editando. Se comprueba que el lado del corte NO sigue al de la edicion.
        /// </summary>
        [Fact]
        public void TheFourFrontalCuts_AreRequestableWithAnExplicitSide()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = MultiFront(fronts: 2, levels: 2);

                // Editando el lado B, pero pidiendo el corte de A: el corte NO sigue a la edicion.
                Combo(w, "SideSelectorBox").SelectedIndex = 1;
                Combo(w, "FrontalSideBox").SelectedIndex = 0;
                if (w.FrontalSideForTest != PushBackSide.A) return false;

                Combo(w, "FrontalSideBox").SelectedIndex = 1;
                if (w.FrontalSideForTest != PushBackSide.B) return false;

                // Y las cuatro secciones existen y son distintas entre si.
                var sections = new[]
                {
                    PushBackSystemFrontalBuilder.EncodeSection(PushBackFrontalEnd.EntradaSalida, PushBackSide.A),
                    PushBackSystemFrontalBuilder.EncodeSection(PushBackFrontalEnd.Posterior, PushBackSide.A),
                    PushBackSystemFrontalBuilder.EncodeSection(PushBackFrontalEnd.EntradaSalida, PushBackSide.B),
                    PushBackSystemFrontalBuilder.EncodeSection(PushBackFrontalEnd.Posterior, PushBackSide.B)
                };

                return sections.Distinct().Count() == 4
                       && sections.All(PushBackSystemFrontalBuilder.IsValidSection);
            });

            Assert.True(ok);
        }

        /// <summary>
        /// I-42 (correccion aislada 1B) — el contenido de los cortes frontales sigue al selector de VISTA, no al
        /// lado que se esta EDITANDO, y cambiar ese selector RECONSTRUYE los dos cortes.
        ///
        /// <para>
        /// La ventana construia los frontales con el lado ACTIVO de la edicion mientras rotulaba y direccionaba la
        /// seccion con el del selector: con «Editando A» y «Frontal de B» el panel mostraba el pasillo de A. Y
        /// cambiar el selector solo repintaba, de modo que el plan seguia siendo el del lado anterior hasta que
        /// cualquier otra edicion forzara un recalculo.
        /// </para>
        /// </summary>
        [Fact]
        public void TheFrontalCuts_FollowTheViewSelector_NotTheEditedSide()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = MultiFront(fronts: 2, levels: 2);

                // Una CORRIDA A->B: carga por A y descarga en B, asi que los dos cortes de cada lado son distintos
                // y confundirlos se ve. Se declara sobre el estado y se recalcula con los controles reales.
                w.CompositeState.SetDefaults(PushBackCellTopology.Corrida, PushBackRunDirection.AToB);
                Combo(w, "SideSelectorBox").SelectedIndex = 0;

                // El propio selector de vista es el que RECONSTRUYE: se pasa por B y se vuelve a A sin tocar nada
                // mas, y con eso basta para que el panel tenga los cortes de A con la topologia recien declarada.
                Combo(w, "FrontalSideBox").SelectedIndex = 1;
                Combo(w, "FrontalSideBox").SelectedIndex = 0;

                var entradaA = Beams(w.LastComputation?.FrontalEntradaSalida);
                var posteriorA = Beams(w.LastComputation?.FrontalPosterior);

                // El corte de A es el de ENTRADA de la corrida; su posterior esta vacio, porque el extremo alto
                // esta en el otro pasillo.
                if (entradaA == 0 || posteriorA != 0) return false;

                // Ahora se EDITA A y se pide el frontal de B: el contenido tiene que cambiar de lado sin tocar la
                // edicion, y sin ninguna otra accion que el propio selector.
                Combo(w, "FrontalSideBox").SelectedIndex = 1;
                if (w.CompositeState.ActiveSide != PushBackSide.A) return false;
                if (w.FrontalSideForTest != PushBackSide.B) return false;

                var entradaB = Beams(w.LastComputation?.FrontalEntradaSalida);
                var posteriorB = Beams(w.LastComputation?.FrontalPosterior);
                if (entradaB != 0 || posteriorB == 0) return false;

                // Y la SECCION del posterior de B direcciona el corte posterior, no el de entrada.
                var section = PushBackSystemFrontalBuilder.EncodeSection(PushBackFrontalEnd.Posterior, PushBackSide.B);
                return PushBackSystemFrontalBuilder.DecodeSection(section).End == PushBackFrontalEnd.Posterior
                       && PushBackSystemFrontalBuilder.DecodeSection(section).Side == PushBackSide.B;
            });

            Assert.True(ok);
        }

        /// <summary>Los largueros de un plan de corte, que es lo que distingue un pasillo que carga de uno que no.</summary>
        private static int Beams(RackCad.Application.Drawing.HeaderRunPlan plan)
            => plan == null
                ? 0
                : plan.Flatten().Instances.Count(i => i.Role == RackCad.Application.Drawing.HeaderBlockRole.Beam);

        /// <summary>
        /// I-42 (correccion aislada 2B) — RACKEDITAR: reabrir un rack guardado conserva las ranuras EN BLANCO del
        /// lado B, con su ancho, y no resucita su almacenamiento.
        ///
        /// <para>
        /// La presencia del lado B se aplicaba ANTES de <c>LoadFromDesign</c>, que rehace la matriz entera desde el
        /// diseño resuelto: al volver de un archivo, las ranuras que el usuario habia dejado en blanco volvian
        /// activas y con un ancho por defecto. Ahora la presencia se aplica DESPUES, y el frente del blanco viaja
        /// completo.
        /// </para>
        /// </summary>
        [Fact]
        public void BlankB_RackEditarPreservesBlankState()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = MultiFront(fronts: 3, levels: 2);

                // La ranura 1 declara calles de mas SOLO en B, y se pone EN BLANCO en B.
                w.CompositeState.Of(PushBackSide.B).Structure.AdjustPositions(1, 1);
                if (!w.CompositeState.SetSlotPresent(PushBackSide.B, 1, false)) return false;

                var design = new PushBackCompositeEditorAssembler(RackCad.Application.Catalogs.JsonRackCatalogProvider
                    .FromBaseDirectory().Load())
                    .BuildDesign(w.CompositeState, PushBackEditorInputs.NewDesign());
                if (!design.Composite.AbsentSlotsB.Contains(1)) return false;

                // RACKEDITAR sobre ese mismo diseño, en una ventana NUEVA.
                var reopened = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                reopened.LoadExisting(design, "GUID-I42-2B", "PB compuesto");

                // La ranura sigue en blanco en B, y sólo esa; el ancho declarado no se perdio.
                var kept = reopened.CompositeState.SlotCount == 3
                           && !reopened.CompositeState.IsSlotPresent(PushBackSide.B, 1)
                           && reopened.CompositeState.IsSlotPresent(PushBackSide.B, 0)
                           && reopened.CompositeState.IsSlotPresent(PushBackSide.B, 2)
                           && reopened.CompositeState.IsSlotPresent(PushBackSide.A, 1)
                           && reopened.CompositeState.Of(PushBackSide.B).Structure.Fronts[1].PalletCount
                              == w.CompositeState.Of(PushBackSide.B).Structure.Fronts[1].PalletCount;

                // Y un documento ANTERIOR —entrada nula, sin la declaracion nueva— tampoco resucita. Era el orden:
                // la presencia se aplicaba antes de reconstruir la matriz, asi que el relleno la devolvia activa.
                design.SideB.Fronts[1] = null;
                design.SideB.FrontConfigs[1] = null;
                design.Composite.AbsentSlotsB.Clear();

                var legacy = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                legacy.LoadExisting(design, "GUID-I42-2B-LEGACY", "PB compuesto");

                return kept
                       && legacy.CompositeState.SlotCount == 3
                       && !legacy.CompositeState.IsSlotPresent(PushBackSide.B, 1)
                       && legacy.CompositeState.IsSlotPresent(PushBackSide.B, 0)
                       && legacy.CompositeState.IsSlotPresent(PushBackSide.B, 2);
            });

            Assert.True(ok);
        }

        /// <summary>
        /// I-42 (correccion aislada 3) — RACKEDITAR de un rack COMPUESTO conserva el troquel de los dos lados.
        ///
        /// <para>
        /// El diseño con el que la ventana reconstruye el lado B se arma aqui, y no llevaba el DATUM del documento:
        /// la matriz de B volvia leyendo «Alto 1er nivel» con la semantica historica mientras la estructura
        /// compuesta usaba la del documento, asi que los dos lados podian acabar en troqueles distintos.
        /// </para>
        /// </summary>
        [Theory]
        [InlineData(5.0)]
        [InlineData(7.0)]
        public void RackEditar_CompositeKeepsBothSidesOnTheSamePunch(double first)
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = MultiFront(fronts: 2, levels: 2);
                foreach (var side in new[] { PushBackSide.A, PushBackSide.B })
                {
                    var matrix = w.CompositeState.Of(side).Structure;
                    for (var index = 0; index < matrix.Count; index++)
                    {
                        matrix.Fronts[index].FirstLevelHeight = first;
                    }
                }

                var design = new PushBackCompositeEditorAssembler(Catalog)
                    .BuildDesign(w.CompositeState, PushBackEditorInputs.NewDesign());
                var before = new PushBackResolver(Catalog).Resolve(design);

                var reopened = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                reopened.LoadExisting(design, "GUID-I42-DATUM-AB", "PB compuesto");

                var after = reopened.LastComputation?.System;
                if (after == null || !reopened.LastComputation.IsValid) return false;

                // El mismo troquel en el rack, y los dos lados alineados entre si.
                var lowBefore = Low(before);
                var lowAfter = Low(after);
                var localA = Low(after.Composite.Of(PushBackSide.A).Local);
                var localB = Low(after.Composite.Of(PushBackSide.B).Local);

                return Math.Abs(lowBefore - lowAfter) < 1e-6
                       && Math.Abs(localA - localB) < 1e-6
                       && Math.Abs(localA - lowAfter) < 1e-6;
            });

            Assert.True(ok);
        }

        /// <summary>La elevacion FISICA del primer larguero de entrada del frente 0.</summary>
        private static double Low(PushBackSystem system)
        {
            var levels = system?.Structure?.Fronts?.FirstOrDefault()?.LoadBeamLevels;
            return levels == null || levels.Count == 0
                ? double.NaN
                : Math.Round(levels.OrderBy(level => level.LevelNumber).First().ExitElevation, 4);
        }

        /// <summary>La casilla de presencia se llama como el dueño la entiende y explica para que sirve.</summary>
        [Fact]
        public void TheBlankCheck_IsNamedInProductLanguage()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = MultiFront(fronts: 2, levels: 2);
                var box = Blank(w, 0);
                var label = box.Content as string;
                var tip = box.ToolTip as string;

                // Una sola autoridad visible, y en lenguaje de producto: «En blanco». El control que competia con
                // ella —«Frente presente en este lado»— ya no existe.
                return label == "En blanco"
                       && tip != null
                       && tip.Contains("claro")
                       && !label.Contains("ranura")
                       && w.FindName("SlotPresentCheck") == null;
            });

            Assert.True(ok);
        }

        /// <summary>
        /// La seccion de estructura muestra la PROPUESTA y la EFECTIVA, para que no parezca «otro fondo». Es la
        /// duplicidad conceptual que el dueño reporto.
        /// </summary>
        [Fact]
        public void TheStructureSection_ShowsProposedAndEffective()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = MultiFront(fronts: 2, levels: 2);
                var proposed = (TextBlock)w.FindName("StructureProposedText");
                var effective = (TextBlock)w.FindName("StructureEffectiveText");

                if (proposed.Text == "—" || effective.Text == "—") return false;

                var box = Field(w, "StructureOverrideBox");
                box.SetNumber(9);
                LoseFocus(box);
                Click(Btn(w, "ApplyStructureButton"));

                // La efectiva sigue al ajuste; la propuesta sigue siendo la derivada de la demanda.
                return effective.Text.StartsWith("9", StringComparison.Ordinal)
                       && !proposed.Text.StartsWith("9", StringComparison.Ordinal);
            });

            Assert.True(ok);
        }

        [Fact]
        public void TheStructureOverride_AppliesAndRestores()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                Check(w, "SideBPresentCheck").IsChecked = true;
                // I-42: la CAPACIDAD ya no declara PRESENCIA; este caso quiere el rack compuesto entero.
                DeclareSideBOnEveryFront(w, w.CompositeState.SlotCount);
                var proposed = w.LastComputation.System.Composite.SideA.ProposedStructure;

                Field(w, "StructureOverrideBox").SetNumber(proposed + 3);
                Click(Btn(w, "ApplyStructureButton"));
                var overridden = w.LastComputation.System.Composite.SideA.EffectiveStructure;

                Click(Btn(w, "RestoreStructureButton"));
                var restored = w.LastComputation.System.Composite.SideA;

                return overridden == proposed + 3
                       && restored.EffectiveStructure == restored.ProposedStructure
                       && !restored.StructureOverride.HasValue;
            });

            Assert.True(ok);
        }

        /// <summary>
        /// RACKEDITAR sobre un rack COMPLETO de esta ronda: cuatro ranuras con una ausente en A, topes distintos por
        /// lado y un fondo de cama corrida. Todo tiene que volver por la ventana tal como salio.
        /// </summary>
        [Fact]
        public void AFullCompositeRack_ReopensWithItsPresenceTopesAndCorridaDepth()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var source = MultiFront(fronts: 4, levels: 2);

                // Asimetria: la cuarta ranura solo existe en B.
                Combo(source, "SelectedFrontBox").SelectedIndex = 3;
                SetBlank(source, source.CompositeState.SlotCount - 1, true);

                // Topes: solo A.
                Check(source, "TopeSideACheck").IsChecked = true;
                Check(source, "TopeSideBCheck").IsChecked = false;
                Combo(source, "TopeScopeBox").SelectedIndex = 4;
                Click(Btn(source, "ApplyTopesButton"));

                // Y una corrida con fondo propio en el primer frente.
                Combo(source, "SelectedFrontBox").SelectedIndex = 0;
                Combo(source, "SelectedLevelBox").SelectedIndex = 0;
                Combo(source, "CellTopologyBox").SelectedIndex = 3;
                Combo(source, "TopologyScopeBox").SelectedIndex = 0;
                Click(Btn(source, "ApplyTopologyButton"));
                var depth = Field(source, "CellFondoOverrideBox");
                depth.SetNumber(4);
                LoseFocus(depth);

                var design = source.LastComputation.Design;
                var before = source.LastComputation.System;

                var reopened = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                reopened.LoadExisting(design, Guid.NewGuid().ToString(), "PB compuesto");

                var state = reopened.CompositeState;
                if (state.IsSlotPresent(PushBackSide.A, 3)) return false;
                if (!state.IsSlotPresent(PushBackSide.B, 3)) return false;
                if (!state.RearTopeAt(PushBackSide.A, 0, 0)) return false;
                if (state.RearTopeAt(PushBackSide.B, 0, 0)) return false;
                if (state.CorridaDepthAt(0, 0) != 4) return false;

                var after = reopened.LastComputation.System;
                if (Math.Abs(after.Structure.TotalLength - before.Structure.TotalLength) > 1e-6) return false;

                var beforeRuns = PushBackRuns.Resolve(before).Runs.Count;
                var afterRuns = PushBackRuns.Resolve(after).Runs.Count;
                return beforeRuns == afterRuns
                       && after.Composite.Cell(3, 1).Topology == PushBackCellTopology.SoloB
                       && after.Composite.Cell(0, 1).Beds.Single().DemandPositions == 4;
            });

            Assert.True(ok);
        }

        [Fact]
        public void ACompositeDesign_RoundTripsThroughTheWindow()
        {
            var design = new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                    PalletsDeep = 4,
                    LoadLevels = 2,
                    FirstLevelHeight = 4.0,
                    BeamDepth = 4.0
                },
                SideB = new PushBackSideDesign { IsPresent = true, LoadLevels = 2, FirstLevelHeight = 4.0 },
                Composite = new PushBackCompositeDesign
                {
                    Gap = 8.0,
                    DefaultTopology = PushBackCellTopology.Encontradas
                }
            };
            design.Structure.Fronts.Add(new DynamicRackFrontDesign
            {
                PalletCount = 1, LoadLevels = 2, PalletsDeep = 4, DepthStartPosition = 1
            });
            design.Fronts.Add(new PushBackFrontConfig { DefaultPalletsDeep = 4 });
            design.SideB.Fronts.Add(new DynamicRackFrontDesign
            {
                PalletCount = 1, LoadLevels = 2, PalletsDeep = 4, DepthStartPosition = 1
            });
            design.SideB.FrontConfigs.Add(new PushBackFrontConfig { DefaultPalletsDeep = 4 });

            var ok = StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                w.LoadExisting(design, Guid.NewGuid().ToString(), "PB compuesto");

                return w.CompositeState.SideBPresent
                       && Math.Abs(w.CompositeState.Gap - 8.0) < 1e-6
                       && w.LastComputation.IsValid
                       && w.LastComputation.System.IsComposite
                       && w.DesignToInsert == null;
            });

            Assert.True(ok);
        }

        [Fact]
        public void ALegacyDesign_OpensAsSingleSided_WithoutAskingForAnything()
        {
            var design = new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                    PalletsDeep = 5,
                    LoadLevels = 3,
                    FirstLevelHeight = 6.0,
                    BeamDepth = 4.0
                }
            };
            design.Structure.Fronts.Add(new DynamicRackFrontDesign
            {
                PalletCount = 2, LoadLevels = 3, PalletsDeep = 5, DepthStartPosition = 1
            });
            design.Fronts.Add(new PushBackFrontConfig());

            var ok = StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                w.LoadExisting(design, Guid.NewGuid().ToString(), "PB legacy");

                return !w.CompositeState.SideBPresent
                       && w.CompositeState.ActiveSide == PushBackSide.A
                       && w.LastComputation.IsValid
                       && !w.LastComputation.System.IsComposite
                       && w.LastComputation.Design.SideB == null;
            });

            Assert.True(ok);
        }
    }
}
