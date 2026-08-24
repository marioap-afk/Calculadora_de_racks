using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.PushBack;

namespace RackCad.UI.Systems.PushBack
{
    /// <summary>
    /// I-42 — la parte COMPUESTA del editor de Push Back: el selector de lado, la interfaz central (hueco y separador),
    /// la topologia por celda con sus cinco alcances, la estructura efectiva por lado con su restauracion y los
    /// diagnosticos.
    ///
    /// <para>
    /// Vive en su propio archivo parcial a proposito: la ventana ya es un archivo caliente, y todo lo que I-42 anade a
    /// la UI cabe aqui sin tocar ni una linea del editor de un solo sentido. La matriz, la celda seleccionada y los
    /// alcances NO se duplican — siguen siendo los mismos controles, conducidos sobre el lado ACTIVO.
    /// </para>
    /// </summary>
    public partial class RackPushBackSystemWindow
    {
        /// <summary>Las cuatro topologias, en el orden en que las ofrece el desplegable.</summary>
        private static readonly PushBackCellTopology[] TopologyOptions =
        {
            PushBackCellTopology.SoloA,
            PushBackCellTopology.SoloB,
            PushBackCellTopology.Encontradas,
            PushBackCellTopology.Corrida
        };

        private static readonly PushBackRunDirection[] DirectionOptions =
        {
            PushBackRunDirection.AToB,
            PushBackRunDirection.BToA
        };

        /// <summary>El estado compuesto, para las pruebas de UI.</summary>
        internal PushBackCompositeEditorState CompositeState => composite;

        /// <summary>Rellena los desplegables de la seccion compuesta. Se llama una vez, al construir la ventana.</summary>
        private void InitializeCompositeSection()
        {
            SideSelectorBox.ItemsSource = new[] { "Lado A", "Lado B" };
            SideSelectorBox.SelectedIndex = 0;
            CellTopologyBox.ItemsSource = new[] { "Solo A", "Solo B", "Encontradas", "Corrida" };
            CellTopologyBox.SelectedIndex = 2;
            RunDirectionBox.ItemsSource = new[] { "A → B", "B → A" };
            RunDirectionBox.SelectedIndex = 0;
            TopologyScopeBox.SelectedIndex = 0;
            UpdateCompositeEnabled();
        }

        /// <summary>
        /// La configuracion exclusiva del compuesto se COLAPSA cuando el rack es de un solo sentido: no ocupa
        /// espacio ni satura la barra lateral, y el editor legacy queda igual que antes de I-42. Deshabilitarla sin
        /// ocultarla dejaba media pantalla de controles muertos en el caso mas comun.
        /// </summary>
        private void UpdateCompositeEnabled()
        {
            var present = composite.SideBPresent;
            CompositeSection.Visibility = present ? Visibility.Visible : Visibility.Collapsed;
            CompositeSection.IsEnabled = present;
        }

        // ---- Lado activo y presencia --------------------------------------------------------------------------

        private void SideBPresent_Changed(object sender, RoutedEventArgs e)
        {
            if (suppressSync)
            {
                return;
            }

            // Retirar el lado B NO borra su configuracion: queda dormante y reaparece intacta al volver a declararlo.
            composite.SetSideBPresent(SideBPresentCheck.IsChecked == true);
            if (composite.SideBPresent && composite.SideB.Structure.Count == 0)
            {
                // Primera vez que se declara: el lado B nace con las MISMAS ranuras que A, que es la retícula
                // transversal compartida. Sin esto llegaria al resolver sin ninguna bahia.
                composite.SideB.LoadNew();
                composite.SideB.SetFrontCount(Math.Max(1, composite.SideA.Structure.Count));
            }
            if (!composite.SideBPresent)
            {
                SideSelectorBox.SelectedIndex = 0;
            }

            UpdateCompositeEnabled();
            RequestRecompute();
        }

        private void SideSelector_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (suppressSync)
            {
                return;
            }

            var side = SideSelectorBox.SelectedIndex == 1 ? PushBackSide.B : PushBackSide.A;
            if (side == composite.ActiveSide)
            {
                return;
            }

            // Cambiar de lado no toca nada: la configuracion del lado que se abandona queda intacta con su seleccion.
            composite.SetActiveSide(side);
            SideSelectorBox.SelectedIndex = composite.ActiveSide == PushBackSide.B ? 1 : 0;
            RequestRecompute();
        }

        // ---- Interfaz central ---------------------------------------------------------------------------------

        private void Gap_Changed(object sender, RoutedEventArgs e)
        {
            if (suppressSync)
            {
                return;
            }

            // Un campo en error no escribe nada: convertirlo en 0 seria corregir la entrada en silencio, que es
            // exactamente lo que no puede pasar. El control ya muestra su propio motivo.
            if (GapBox.HasError || !GapBox.Value.HasValue)
            {
                return;
            }

            composite.SetGap(GapBox.Value.Value);
            RequestRecompute();
        }

        private void CentralSeparator_Changed(object sender, RoutedEventArgs e)
        {
            if (suppressSync)
            {
                return;
            }

            composite.SetCentralSeparator(CentralSeparatorCheck.IsChecked == true);
            RequestRecompute();
        }

        // ---- Topologia por celda ------------------------------------------------------------------------------

        private void ApplyTopology_Click(object sender, RoutedEventArgs e)
        {
            if (!composite.SideBPresent)
            {
                return;
            }

            var topology = TopologyOptions[Math.Max(0, Math.Min(CellTopologyBox.SelectedIndex, TopologyOptions.Length - 1))];
            var direction = DirectionOptions[Math.Max(0, Math.Min(RunDirectionBox.SelectedIndex, DirectionOptions.Length - 1))];
            var scope = CompositeScope(TopologyScopeBox.SelectedIndex);

            int written;
            using (session.Recompute.Defer())
            {
                written = composite.ApplyTopology(topology, direction, scope);
            }

            SetStatus(
                written > 0
                    ? "Topología aplicada a " + written + (written == 1 ? " celda." : " celdas.")
                    : "Ninguna celda en el alcance.",
                written == 0);
            RequestRecompute();
        }

        /// <summary>Los CINCO alcances de siempre. No hay un segundo modelo de seleccion en I-42.</summary>
        private static DynamicRackCellScope CompositeScope(int index)
        {
            switch (index)
            {
                case 1: return DynamicRackCellScope.Selected;
                case 2: return DynamicRackCellScope.Level;
                case 3: return DynamicRackCellScope.Front;
                case 4: return DynamicRackCellScope.All;
                default: return DynamicRackCellScope.Cell;
            }
        }

        // ---- Estructura efectiva por lado ---------------------------------------------------------------------

        private void ApplyStructure_Click(object sender, RoutedEventArgs e)
        {
            // Un campo VACIO o en error no es una restauracion: restaurar tiene su propio boton y es una accion
            // explicita del usuario. Aplicar sin valor no toca el override almacenado.
            if (StructureOverrideBox.HasError || !StructureOverrideBox.Value.HasValue)
            {
                SetStatus(
                    "Escribe los fondos de estructura del lado activo, o pulsa Restaurar estructura para volver a "
                    + "la propuesta.",
                    true);
                return;
            }

            composite.SetStructureOverride(composite.ActiveSide, (int)StructureOverrideBox.Value.Value);
            RequestRecompute();
        }

        private void RestoreStructure_Click(object sender, RoutedEventArgs e)
        {
            // Restaurar es exactamente eliminar el override: el lado vuelve a seguir la propuesta derivada ACTUAL.
            composite.RestoreStructure(composite.ActiveSide);
            RequestRecompute();
        }

        // ---- Lectura del sistema resuelto ---------------------------------------------------------------------

        /// <summary>Refleja en el panel lo que el resolver acaba de decidir: propuesta, estructura efectiva y hueco.</summary>
        private void RefreshCompositePanel(PushBackSystem system)
        {
            var wasSuppressed = suppressSync;
            suppressSync = true;
            try
            {
                SideBPresentCheck.IsChecked = composite.SideBPresent;
                SideSelectorBox.SelectedIndex = composite.ActiveSide == PushBackSide.B ? 1 : 0;
                GapBox.SetNumber(composite.Gap);
                CentralSeparatorCheck.IsChecked = composite.CentralSeparator;
                var stored = composite.StructureOverride(composite.ActiveSide);
                StructureOverrideBox.SetNumber(stored.HasValue ? (double?)stored.Value : null);

                var side = system?.Composite?.Of(composite.ActiveSide);
                CompositeStructureText.Text = side == null || !side.IsPresent
                    ? "Estructura: —"
                    : string.Format(
                        System.Globalization.CultureInfo.CurrentCulture,
                        "Estructura del lado {0}: propuesta {1}, efectiva {2}{3}.",
                        composite.ActiveSide == PushBackSide.B ? "B" : "A",
                        side.ProposedStructure,
                        side.EffectiveStructure,
                        side.StructureOverride.HasValue ? " (manual)" : " (automática)");
            }
            finally
            {
                suppressSync = wasSuppressed;
                UpdateCompositeEnabled();
            }
        }

        /// <summary>
        /// Los diagnosticos vigentes: los de la INTENCION (entradas invalidas, que se conservan y se declaran sin
        /// convertirse en otro valor) y los del sistema ya resuelto. Una sola lista, con su severidad.
        /// </summary>
        private IReadOnlyList<PushBackCompositeDiagnostic> CompositeDiagnostics()
            => composite.IntentDiagnostics()
                .Concat(PushBackCompositeDiagnostics.Evaluate(lastCompositeSystem))
                .ToList();

        /// <summary>El mensaje de estado: el primer diagnostico que importe, o el texto normal del recalculo.</summary>
        private string CompositeStatusOr(string fallback)
        {
            var diagnostics = CompositeDiagnostics();
            var blocking = diagnostics.FirstOrDefault(diagnostic => diagnostic.IsBlocking);
            if (blocking != null)
            {
                return blocking.Message;
            }

            var warning = diagnostics.FirstOrDefault(
                diagnostic => diagnostic.Severity == PushBackCompositeSeverity.Warning);
            return warning != null ? warning.Message : fallback;
        }

        private bool CompositeHasBlocking(PushBackSystem system)
        {
            lastCompositeSystem = system;
            return CompositeDiagnostics().Any(diagnostic => diagnostic.IsBlocking);
        }

        /// <summary>El ultimo sistema resuelto, para que el estado pueda releer sus diagnosticos sin recomputar.</summary>
        private PushBackSystem lastCompositeSystem;

        /// <summary>Recupera la intencion compuesta de un diseno cargado (save/load, RACKEDITAR, biblioteca).</summary>
        private void LoadCompositeFromDesign(PushBackDesign design)
        {
            composite.SetSideBPresent(design != null && design.IsComposite);
            composite.LoadComposite(design?.Composite);
            if (design?.SideB != null && design.SideB.IsPresent)
            {
                // El lado B se recupera en su propio estado, por el MISMO camino de carga que el lado A.
                var sideDesign = new PushBackDesign
                {
                    Structure = new RackCad.Domain.Systems.Dynamic.DynamicRackDesign
                    {
                        Pallet = design.Structure?.Pallet,
                        PalletsDeep = design.Structure?.PalletsDeep ?? 2,
                        LoadLevels = design.SideB.LoadLevels,
                        FirstLevelHeight = design.SideB.FirstLevelHeight,
                        BeamDepth = design.Structure?.BeamDepth ?? 6.0,
                        HeaderPostCatalogId = design.Structure?.HeaderPostCatalogId,
                        PostPeralte = design.Structure?.PostPeralte ?? 0.0
                    },
                    LegacyHighEndBeamPeralte = design.SideB.LegacyHighEndBeamPeralte,
                    RearTope = design.SideB.RearTope?.DeepCopy() ?? new PushBackRearTopeConfig()
                };

                for (var slot = 0; slot < design.SideB.Fronts.Count; slot++)
                {
                    var front = design.SideB.Fronts[slot];
                    composite.SetSlotPresent(PushBackSide.B, slot, front != null);
                    sideDesign.Structure.Fronts.Add(front ?? new RackCad.Domain.Systems.Dynamic.DynamicRackFrontDesign
                    {
                        PalletCount = 1,
                        LoadLevels = design.SideB.LoadLevels,
                        PalletsDeep = 2,
                        DepthStartPosition = 1
                    });
                    sideDesign.Fronts.Add(design.SideB.FrontConfig(slot) ?? new PushBackFrontConfig());
                }

                composite.SideB.LoadFromDesign(sideDesign, assembler.Resolver);
            }

            UpdateCompositeEnabled();
        }
    }
}
