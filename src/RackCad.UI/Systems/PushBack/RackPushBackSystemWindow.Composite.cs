using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.PushBack;
using RackCad.UI.Controls;

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
            // I-42 (ronda Owner): «Ambos» es una operacion de EDICION —escribe la misma intencion en los dos lados—,
            // no un tercer lado. No existe en el dominio, en el archivo ni en el dibujo.
            SideSelectorBox.ItemsSource = new[] { "Lado A", "Lado B", "Ambos lados" };
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
            UpdateFrontalButtons(present);
        }

        /// <summary>
        /// Los CORTES FRONTALES de un rack compuesto son cuatro: entrada/salida y posterior de cada lado. Con el
        /// compuesto apagado hay dos y se llaman como siempre; al encenderlo, los dos de siempre pasan a decir «A» y
        /// aparecen los dos de B. Ni un boton ambiguo, ni seis botones donde bastan cuatro.
        /// </summary>
        private void UpdateFrontalButtons(bool composed)
        {
            if (FrontalSideBox == null)
            {
                return;
            }

            if (FrontalSideBox.ItemsSource == null)
            {
                FrontalSideBox.ItemsSource = new[] { "Frontal de A", "Frontal de B" };
                FrontalSideBox.SelectedIndex = 0;
            }

            FrontalSideBox.Visibility = composed ? Visibility.Visible : Visibility.Collapsed;
            if (!composed)
            {
                frontalSide = PushBackSide.A;
                FrontalSideBox.SelectedIndex = 0;
            }
        }

        // ---- Lado activo y presencia --------------------------------------------------------------------------

        private void SideBPresent_Changed(object sender, RoutedEventArgs e)
        {
            if (suppressSync)
            {
                return;
            }

            // Retirar el lado B NO borra su configuracion: queda dormante y reaparece intacta al volver a
            // declararlo. Y DECLARARLO es solo declarar la CAPACIDAD: el modelo inicializa el lado, iguala la
            // retícula y lo deja AUSENTE en todos los frentes, que el usuario declara uno por uno. La ventana ya no
            // decide nada de esto — hacerlo aqui ataba el resultado al orden en que se tocaran los controles.
            composite.SetSideBPresent(SideBPresentCheck.IsChecked == true);
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

            var selection = SideSelectorBox.SelectedIndex == 2
                ? PushBackSideSelection.Both
                : SideSelectorBox.SelectedIndex == 1 ? PushBackSideSelection.B : PushBackSideSelection.A;
            if (selection == composite.ActiveSelection)
            {
                return;
            }

            // Cambiar de lado no toca nada: la configuracion del lado que se abandona queda intacta con su seleccion.
            composite.SetActiveSelection(selection);
            SideSelectorBox.SelectedIndex = (int)composite.ActiveSelection;

            // El panel de la celda pertenece al lado que se acaba de elegir: se recarga en el acto, incluido el
            // estado MIXTO de «Ambos». Sin esto la ventana mostraria los valores del lado anterior.
            suppressSync = true;
            try
            {
                RenderPushBackMatrix();
                LoadSelectedFront();
            }
            finally
            {
                suppressSync = false;
            }

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

            // I-42: cambiar la topologia cambia QUE autoridad edita el campo de fondo, asi que el panel de la celda
            // se recarga en el acto. Sin esto el usuario veria la etiqueta anterior sobre el valor nuevo.
            suppressSync = true;
            try
            {
                LoadSelectedFront();
            }
            finally
            {
                suppressSync = false;
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

        // ---- I-42: el fondo propio de la cama CORRIDA ---------------------------------------------------------

        private const string CellFondoLabelText = "Fondo celda";
        private const string CorridaFondoLabelText = "Fondo de cama corrida";

        private const string CellFondoTip =
            "Fondo propio de esta celda. Vacío = hereda el «Fondos frente».";

        private const string CorridaFondoTip =
            "Fondo TOTAL de la cama corrida de esta celda: los fondos que la calle aloja de extremo a extremo. "
            + "Es su propia autoridad — no es el fondo de A, ni el de B, ni su suma —, y los fondos de los dos lados "
            + "se conservan intactos para cuando la celda deje de ser corrida. Vacío = la calle atraviesa el rack.";

        private const string RestoreCellFondoTip =
            "Elimina el fondo propio: las celdas del alcance vuelven a heredar el del frente.";

        private const string RestoreCorridaFondoTip =
            "Elimina el fondo propio de la cama corrida: las celdas corridas del alcance vuelven a atravesar el rack "
            + "entero. Los fondos de A y de B no se tocan.";

        /// <summary>
        /// True cuando el campo de fondo edita la cama CORRIDA de la celda seleccionada en vez del fondo de la celda
        /// del lado activo. Es la topologia de esa celda la que lo decide, no un modo aparte de la ventana.
        /// </summary>
        private bool EditsCorridaDepth()
        {
            if (!composite.SideBPresent)
            {
                return false;
            }

            var matrix = composite.Active.Structure;
            return composite.TopologyAt(matrix.SelectedFrontIndex, matrix.SelectedLevelIndex)
                   == PushBackCellTopology.Corrida;
        }

        /// <summary>
        /// Carga el campo de fondo con la autoridad que corresponde a la celda seleccionada, y dice cual es: la
        /// etiqueta y el tooltip cambian con ella, para que no haya forma de escribir un numero creyendo que
        /// significa otra cosa.
        /// </summary>
        private void LoadCellFondoField(PushBackEditorCell push)
        {
            if (EditsCorridaDepth())
            {
                var matrix = composite.Active.Structure;
                CellFondoLabel.Text = CorridaFondoLabelText;
                SetLiveToolTip(CellFondoOverrideBox, CorridaFondoTip);
                SetLiveToolTip(RestoreCellFondoButton, RestoreCorridaFondoTip);
                CellFondoOverrideBox.SetNumber(
                    composite.CorridaDepthAt(matrix.SelectedFrontIndex, matrix.SelectedLevelIndex));
                return;
            }

            CellFondoLabel.Text = CellFondoLabelText;
            SetLiveToolTip(CellFondoOverrideBox, CellFondoTip);
            SetLiveToolTip(RestoreCellFondoButton, RestoreCellFondoTip);
            CellFondoOverrideBox.SetNumber(push.PalletsDeepOverride);
        }

        /// <summary>
        /// Cambia el tooltip VIGENTE de un control sin pelearse con I-33: la explicacion de «frente en blanco»
        /// guarda el tooltip original para restaurarlo, asi que ese guardado tambien tiene que actualizarse. Sin
        /// esto, la siguiente recarga devolveria el texto viejo sobre el campo ya reapuntado.
        /// </summary>
        private void SetLiveToolTip(Control control, object tip)
        {
            if (control == null)
            {
                return;
            }

            if (blankToolTips.ContainsKey(control))
            {
                blankToolTips[control] = tip;
            }

            if (control.IsEnabled)
            {
                control.ToolTip = tip;
            }
        }

        /// <summary>
        /// Dice QUE se escribio y QUE quedo fuera. Un alcance puede mezclar celdas corridas con celdas que no lo son:
        /// el fondo de una corrida no significa nada en las segundas, asi que no se escribe en ellas — y se dice,
        /// en lugar de dejar creer que la operacion alcanzo a todas.
        /// </summary>
        private void ReportCorridaScope(DynamicRackCellScope scope, int written)
        {
            var targets = composite.CorridaTargets(scope);
            if (written == 0)
            {
                SetStatus("Ninguna celda corrida en el alcance: el fondo de cama corrida no se aplicó.", true);
                return;
            }

            if (targets.Corridas < targets.Total)
            {
                SetStatus(
                    "Fondo de cama corrida aplicado a " + written + " de " + targets.Total
                    + " celdas del alcance; las demás no son corridas y conservan su fondo por lado.",
                    false);
                return;
            }

            if (scope != DynamicRackCellScope.Cell)
            {
                SetStatus("Fondo de cama corrida aplicado a " + written + " celda(s).", false);
            }
        }

        /// <summary>Como se llama la seleccion de edicion vigente, para decirlo en las etiquetas.</summary>
        private string SideLabel()
        {
            switch (composite.ActiveSelection)
            {
                case PushBackSideSelection.B: return "lado B";
                case PushBackSideSelection.Both: return "ambos lados";
                default: return "lado A";
            }
        }

        // ---- I-42: el estado MIXTO de «Ambos lados» -------------------------------------------------------------

        private readonly Dictionary<NumericField, bool> mixedOptional = new Dictionary<NumericField, bool>();

        /// <summary>
        /// I-42 (A3-CELL) — los campos que AHORA MISMO estan mostrando estado mixto.
        ///
        /// <para>
        /// No es un dirty tracking nuevo: es la MISMA decision que toma <see cref="ApplyMixedSideState"/> en cada
        /// pasada, anotada para que la lectura pueda distinguir los dos huecos que en un campo OPCIONAL se ven
        /// exactamente igual — el hueco que pinta el estado mixto («cada lado conserva el suyo») y el hueco que el
        /// usuario deja a proposito («sin override»). En los campos obligatorios esa ambiguedad no existe, y por eso
        /// hasta ahora no hizo falta.
        /// </para>
        /// </summary>
        private readonly HashSet<NumericField> mixedNow = new HashSet<NumericField>();

        /// <summary>¿Este campo esta pintando ahora un estado mixto?</summary>
        private bool IsMixed(NumericField field) => field != null && mixedNow.Contains(field);

        /// <summary>
        /// Con «Ambos lados» seleccionado, un campo cuyo valor DIFIERE entre A y B se muestra VACIO.
        ///
        /// <para>
        /// No se elige el de A ni el de B: mentir sobre cual es el valor vigente es peor que no decirlo. Vacio
        /// significa «cada lado conserva el suyo» —<see cref="ReadCellValues(PushBackEditorState)"/> resuelve el
        /// hueco contra el lado que escribe—, y en cuanto el usuario escribe un numero ese numero se aplica a los
        /// dos. El campo se vuelve opcional mientras dura el estado mixto para que un hueco legitimo no se marque
        /// como error y bloquee el resto de la edicion.
        /// </para>
        /// </summary>
        private void ApplyMixedSideState()
        {
            if (SlotPresentText == null)
            {
                return;
            }

            var mixed = composite.ActiveSelection == PushBackSideSelection.Both && composite.SideBPresent;
            var a = SideFront(PushBackSide.A);
            var b = SideFront(PushBackSide.B);

            SetMixed(PositionsBox, mixed && a?.PalletCount != b?.PalletCount);
            SetMixed(LevelsBox, mixed && a?.LoadLevels != b?.LoadLevels);
            SetMixed(FondosBox, mixed && a?.PalletsDeep != b?.PalletsDeep);
            SetMixed(DepthStartBox, mixed && a?.DepthStartPosition != b?.DepthStartPosition);
            SetMixed(
                FirstLevelHeightBox,
                mixed && Math.Abs((a?.FirstLevelHeight ?? 0.0) - (b?.FirstLevelHeight ?? 0.0)) > 1e-6);

            // I-42 (A1C/H10, contrato del dueño) — MOSTRAR NO ES ESCRIBIR, y la celda tambien.
            //
            // El panel se rellena con los valores del lado que se esta mirando, y en «Ambos» la escritura lee esos
            // mismos controles contra CADA lado: un campo con numero se aplica a los dos. Los cinco campos de arriba
            // ya se vaciaban cuando A y B diferian, pero los de la CELDA no, asi que bastaba con SELECCIONAR
            // «Ambos» —sin tocar nada— para que el alto, el peso y la altura libre de A pisaran los de B. Medido:
            // B pasaba de 72/1500/9 a 60/1000/6 sin ninguna edicion.
            //
            // La regla no cambia: vacio significa «cada lado conserva el suyo», y lo resuelve
            // ReadCellValues(lado) como siempre. Lo unico que se amplia es a que campos alcanza.
            var cellA = SideCell(PushBackSide.A);
            var cellB = SideCell(PushBackSide.B);
            SetMixed(CellPalletFrontBox, mixed && Differs(cellA?.PalletFront, cellB?.PalletFront));
            SetMixed(CellPalletHeightBox, mixed && Differs(cellA?.PalletHeight, cellB?.PalletHeight));
            SetMixed(CellPalletWeightBox, mixed && Differs(cellA?.PalletWeight, cellB?.PalletWeight));
            SetMixed(CellClearBox, mixed && Differs(cellA?.ClearHeight, cellB?.ClearHeight));
            SetMixed(
                CellBeamLengthOverrideBox,
                mixed && Differs(cellA?.BeamLengthOverride, cellB?.BeamLengthOverride));

            // Un desplegable expresa el hueco sin seleccion, y ReadCellValues cae al valor del lado exactamente
            // igual que con un campo numerico vacio.
            SetMixedSelection(
                CellInOutBeamBox,
                mixed && !string.Equals(cellA?.InOutBeamCatalogId, cellB?.InOutBeamCatalogId, StringComparison.OrdinalIgnoreCase));
            SetMixedSelection(CellInOutPeralteBox, mixed && Differs(cellA?.InOutBeamDepth, cellB?.InOutBeamDepth));
            SetMixedSelection(
                CellIntermediateBeamBox,
                mixed && !string.Equals(cellA?.IntermediateBeamCatalogId, cellB?.IntermediateBeamCatalogId, StringComparison.OrdinalIgnoreCase));
            SetMixedSelection(
                CellIntermediatePeralteBox,
                mixed && Differs(cellA?.IntermediateBeamDepth, cellB?.IntermediateBeamDepth));

            var pushA = SidePushCell(PushBackSide.A);
            var pushB = SidePushCell(PushBackSide.B);
            SetMixedSelection(RearPeralteBox, mixed && Differs(pushA?.HighEndBeamPeralte, pushB?.HighEndBeamPeralte));
            SetMixed(
                CellFondoOverrideBox,
                mixed && pushA?.PalletsDeepOverride != pushB?.PalletsDeepOverride);
        }

        /// <summary>Dos medidas opcionales difieren cuando una existe y la otra no, o cuando no coinciden.</summary>
        private static bool Differs(double? left, double? right)
            => left.HasValue != right.HasValue
               || (left.HasValue && Math.Abs(left.Value - right.Value) > 1e-6);

        /// <summary>La CELDA seleccionada de un lado, o null si ese lado no la tiene.</summary>
        private DynamicEditorCell SideCell(PushBackSide side)
        {
            var front = SideFront(side);
            if (front == null || front.Cells.Count == 0)
            {
                return null;
            }

            var level = Math.Max(0, Math.Min(composite.Active.Structure.SelectedLevelIndex, front.Cells.Count - 1));
            return front.Cells[level];
        }

        /// <summary>La celda PUSH BACK seleccionada de un lado (peralte posterior y fondo propio), o null.</summary>
        private PushBackEditorCell SidePushCell(PushBackSide side)
        {
            var state = composite.Of(side);
            var matrix = state.Structure;
            var frontIndex = composite.Active.Structure.SelectedFrontIndex;
            if (frontIndex < 0 || frontIndex >= matrix.Count)
            {
                return null;
            }

            var levels = Math.Max(1, matrix.Fronts[frontIndex].LoadLevels);
            var level = Math.Max(0, Math.Min(composite.Active.Structure.SelectedLevelIndex, levels - 1));
            return state.Cell(frontIndex, level);
        }

        /// <summary>
        /// Pone o quita el estado mixto de un DESPLEGABLE: sin seleccion mientras los dos lados difieran, con el
        /// mismo significado que un campo vacio —«cada lado conserva el suyo»— y el mismo aviso.
        /// </summary>
        private void SetMixedSelection(Selector combo, bool mixed)
        {
            if (combo == null || !mixed)
            {
                return;
            }

            combo.SelectedIndex = -1;
            ToolTipService.SetShowOnDisabled(combo, true);
            combo.ToolTip = "Los dos lados tienen valores distintos. Elige uno para aplicarlo a A y a B; "
                            + "dejalo sin elegir y cada lado conserva el suyo.";
        }

        /// <summary>El frente SELECCIONADO de un lado, o null si ese lado no lo tiene.</summary>
        private DynamicEditorFront SideFront(PushBackSide side)
        {
            var matrix = composite.Of(side).Structure;
            var index = composite.Active.Structure.SelectedFrontIndex;
            return index >= 0 && index < matrix.Count ? matrix.Fronts[index] : null;
        }

        /// <summary>Pone o quita el estado mixto de un campo, conservando su caracter opcional original.</summary>
        private void SetMixed(NumericField field, bool mixed)
        {
            if (field == null)
            {
                return;
            }

            if (!mixedOptional.ContainsKey(field))
            {
                mixedOptional[field] = field.IsOptional;
            }

            if (mixed)
            {
                mixedNow.Add(field);
                field.IsOptional = true;
                field.SetNumber(null);
                ToolTipService.SetShowOnDisabled(field, true);
                field.ToolTip = "Los dos lados tienen valores distintos. Escribe uno para aplicarlo a A y a B; "
                                + "dejalo vacio y cada lado conserva el suyo.";
                return;
            }

            mixedNow.Remove(field);
            field.IsOptional = mixedOptional[field];
        }

        // ---- I-42: presencia de la ranura en el lado activo ----------------------------------------------------

        // ---- I-42: los topes de los DOS lados --------------------------------------------------------------------

        /// <summary>
        /// Refleja en el panel compuesto lo que la CELDA seleccionada tiene ahora: si su ranura existe en este lado y
        /// que topes lleva. La aplicabilidad fisica de cada tope la decide la topologia, asi que la casilla del lado
        /// que hoy no puede materializar ninguno se deshabilita CON SU MOTIVO — pero conserva y sigue mostrando la
        /// intencion almacenada, que es la que vuelve a mandar en cuanto la topologia la admite.
        /// </summary>
        private void LoadCompositeCellPanel()
        {
            if (SlotPresentText == null || !composite.SideBPresent)
            {
                return;
            }

            var matrix = composite.Active.Structure;
            var slot = matrix.SelectedFrontIndex;
            var level = matrix.SelectedLevelIndex;

            var wasSuppressed = suppressSync;
            suppressSync = true;
            try
            {
                // La PRESENCIA por lado es derivada: se informa, no se edita aqui. La decision se toma con la
                // casilla «En blanco» de la cabecera de la columna, que es la unica autoridad visible.
                var present = composite.IsSlotPresent(composite.ActiveSide, slot);
                SlotPresentText.Text = "Frente " + (slot + 1) + " · lado "
                    + (composite.ActiveSide == PushBackSide.A ? "A" : "B") + ": "
                    + (present
                        ? "presente. Marca «En blanco» en su columna para retirarlo de este lado."
                        : "EN BLANCO. Conserva su claro y su estructura, y no lleva ninguna carga en este lado.");

                // La PRESENCIA y el AJUSTE de estructura son de UN lado por definicion: aplicarlos «a los dos» no
                // significa nada —la asimetria es justo lo que expresan—, asi que con «Ambos» se deshabilitan y se
                // dice por que, en vez de escribir en A a escondidas.
                var perSide = composite.ActiveSelection != PushBackSideSelection.Both;
                SetPerSideSensitive(StructureOverrideBox, perSide);
                SetPerSideSensitive(ApplyStructureButton, perSide);
                SetPerSideSensitive(RestoreStructureButton, perSide);

                ApplyMixedSideState();
            }
            finally
            {
                suppressSync = wasSuppressed;
            }
        }

        /// <summary>
        /// Habilita un control que solo tiene sentido con UN lado elegido. Con «Ambos» se deshabilita CON SU MOTIVO:
        /// es informacion, no un control muerto.
        /// </summary>
        private void SetPerSideSensitive(Control control, bool perSide)
        {
            if (control == null)
            {
                return;
            }

            control.IsEnabled = perSide;
            ToolTipService.SetShowOnDisabled(control, true);
            if (!perSide)
            {
                control.ToolTip = "Esto es de UN lado —es lo que expresa que A y B sean distintos—, asi que hay que "
                                  + "elegir «Lado A» o «Lado B» para editarlo.";
            }
        }

        /// <summary>
        /// La casilla de un lado siempre muestra la INTENCION guardada. Se deshabilita cuando esa intencion no es
        /// EFECTIVA hoy, y el tooltip dice las dos cosas por separado: que la intencion sigue viva y por que la
        /// topologia no la materializa. Son dos hechos distintos y confundirlos era la mitad del error 10.
        /// </summary>
        private void SetTopeSensitive(CheckBox box, PushBackTopeSurface surface, PushBackSide side)
        {
            var label = side == PushBackSide.A ? "A" : "B";
            var applicable = surface.AppliesTo(side);
            box.IsEnabled = applicable;
            ToolTipService.SetShowOnDisabled(box, true);
            box.ToolTip = applicable
                ? "EFECTIVO: el lado " + label + " tiene extremo alto en esta celda, y el tope se coloca "
                  + Where(surface) + "."
                : "INTENCION GUARDADA, hoy no efectiva: " + Why(surface) + ", asi que en el lado " + label
                  + " no hay extremo alto donde ponerlo. Lo elegido se conserva y vuelve en cuanto la topologia lo "
                  + "admita.";
        }

        /// <summary>Donde aterriza fisicamente el tope, en terminos que el usuario ve en la planta.</summary>
        private static string Where(PushBackTopeSurface surface)
            => surface.AtInterface
                ? "en la linea INTERIOR, la del centro del rack"
                : "en la linea EXTERIOR del lado alto, al final del recorrido de la calle";

        /// <summary>Por que una celda no admite tope en un lado.</summary>
        private static string Why(PushBackTopeSurface surface)
        {
            switch (surface.Topology)
            {
                case PushBackCellTopology.Corrida:
                    return "es una corrida "
                        + (surface.Direction == PushBackRunDirection.AToB ? "A->B" : "B->A")
                        + " —UNA sola cama que cruza el rack, con un solo extremo alto—";
                case PushBackCellTopology.SoloA:
                    return "la celda solo carga por el lado A";
                case PushBackCellTopology.SoloB:
                    return "la celda solo carga por el lado B";
                default:
                    return "la celda no existe en ese lado";
            }
        }

        /// <summary>
        /// El texto contextual del panel. Dice TRES cosas, que son las tres que el dueño pidio poder predecir sin
        /// conocer la implementacion: que topologia tiene la celda de verdad, que lado es EFECTIVO y cual queda como
        /// intencion dormante, y DONDE va a aparecer la pieza en la planta.
        /// </summary>
        private static string Describe(PushBackTopeSurface surface)
        {
            if (!surface.Exists)
            {
                return "Esta celda no existe en ningun lado: no hay tope que decidir.";
            }

            if (surface.IsIndependentPair)
            {
                return "Encontradas: A y B son camas INDEPENDIENTES y cada una admite su propio tope. Los dos van "
                       + Where(surface) + ".";
            }

            var effective = surface.AppliesToA ? "A" : "B";
            var dormant = surface.AppliesToA ? "B" : "A";
            var head = surface.Topology == PushBackCellTopology.Corrida
                ? "Corrida " + (surface.Direction == PushBackRunDirection.AToB ? "A->B" : "B->A")
                  + ": UNA sola cama cruza el rack"
                : "Solo " + effective + ": una sola cama";

            return head + ", asi que solo el lado " + effective + " es EFECTIVO y su tope va " + Where(surface)
                   + ". Lo que el lado " + dormant + " tenga elegido se CONSERVA como intencion y vuelve a mandar en "
                   + "cuanto la topologia lo admita.";
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
                SideSelectorBox.SelectedIndex = (int)composite.ActiveSelection;
                GapBox.SetNumber(composite.Gap);
                CentralSeparatorCheck.IsChecked = composite.CentralSeparator;
                var stored = composite.StructureOverride(composite.ActiveSide);
                StructureOverrideBox.SetNumber(stored.HasValue ? (double?)stored.Value : null);

                // «Fondos frente» es de un LADO cuando hay dos: decirlo evita que parezca el mismo campo que el
                // ajuste manual de estructura, que es lo que confundio al dueño.
                FondosFrenteLabel.Text = composite.SideBPresent
                    ? "Fondos frente (" + SideLabel() + ")"
                    : "Fondos frente";

                var side = system?.Composite?.Of(composite.ActiveSide);
                var present = side != null && side.IsPresent;
                StructureProposedText.Text = present
                    ? side.ProposedStructure.ToString(System.Globalization.CultureInfo.CurrentCulture) + " fondos"
                    : "—";
                StructureEffectiveText.Text = present
                    ? side.EffectiveStructure.ToString(System.Globalization.CultureInfo.CurrentCulture) + " fondos"
                    : "—";
                CompositeStructureText.Text = !present
                    ? string.Empty
                    : side.StructureOverride.HasValue
                        ? "Lado " + (composite.ActiveSide == PushBackSide.B ? "B" : "A") + ": ajuste MANUAL vigente."
                        : "Lado " + (composite.ActiveSide == PushBackSide.B ? "B" : "A")
                          + ": sigue la propuesta automática.";
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

            // I-42 (A4-MOD-LIFECYCLE / N-1): la COLA persistida —el hueco y la mitad B con sus personalizaciones y
            // sus configuraciones por linea— se aparca al cargar. El lado A se cargo con SU cabeza, y el primer
            // recalculo compuesto devuelve la cola a su sitio: es el mismo mecanismo con el que un lado B dormido
            // vuelve intacto, no un segundo camino de carga.
            var structure = design?.Structure;
            if (structure != null)
            {
                composite.ParkDormantTail(
                    structure.Modules.Where(module => module != null
                        && PushBackCompositeStructure.IsCompositeTailId(module.ModuleId)),
                    structure.HeaderLineOverrides.Where(line => line != null
                        && PushBackCompositeStructure.IsCompositeTailId(line.ModuleId)));
            }

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
                        PostPeralte = design.Structure?.PostPeralte ?? 0.0,
                        // I-42: el lado B se reconstruye con el MISMO datum del documento. Sin el, su matriz volvia
                        // con la semantica historica mientras la estructura compuesta usaba la del documento.
                        FirstLevelDatum = design.Structure?.FirstLevelDatum
                    },
                    LegacyHighEndBeamPeralte = design.SideB.LegacyHighEndBeamPeralte,
                    RearTope = design.SideB.RearTope?.DeepCopy() ?? new PushBackRearTopeConfig(),
                    // I-42 (ronda 7E): el TIPO de defensa del lado B vuelve con el lado B.
                    DefensePieceId = design.SideB.DefensePieceId
                };

                // I-42 (correccion aislada 2B) — el diseno del lado B se arma con sus frentes TAL CUAL vienen: los
                // de una ranura en blanco viajan completos desde que su ausencia se declara aparte, asi que aqui ya
                // no hay que fabricar un relleno que borraba el ancho. Solo un documento ANTERIOR trae la entrada
                // nula, y para ese —y solo para ese— se conserva el relleno, sin inventar nada que no guardara.
                var blank = new List<int>();
                for (var slot = 0; slot < design.SideB.Fronts.Count; slot++)
                {
                    var front = design.SideB.Fronts[slot];
                    if (front == null || design.Composite != null && design.Composite.IsSlotAbsentInB(slot))
                    {
                        blank.Add(slot);
                    }

                    sideDesign.Structure.Fronts.Add(front ?? new RackCad.Domain.Systems.Dynamic.DynamicRackFrontDesign
                    {
                        PalletCount = 1,
                        LoadLevels = design.SideB.LoadLevels,
                        PalletsDeep = 2,
                        DepthStartPosition = 1,
                        IsActive = false
                    });
                    sideDesign.Fronts.Add(design.SideB.FrontConfig(slot) ?? new PushBackFrontConfig());
                }

                composite.SideB.LoadFromDesign(sideDesign, assembler.Resolver);

                // La PRESENCIA se aplica DESPUES de reconstruir la matriz. Aplicarla antes no servia de nada:
                // LoadFromDesign rehace la matriz entera desde el diseno resuelto, asi que al reabrir un rack
                // guardado el lado B resucitaba en las ranuras que el usuario habia dejado en blanco.
                foreach (var slot in blank)
                {
                    composite.SetSlotPresent(PushBackSide.B, slot, false);
                }
            }

            // La retícula transversal es del RACK tambien al VOLVER de un archivo: un diseño cuyo lado B tenga mas o
            // menos ranuras que el A se iguala creciendo, sin recortar ninguno y sin inventar presencia.
            composite.AlignSlotGrid();

            UpdateCompositeEnabled();
        }
    }
}
