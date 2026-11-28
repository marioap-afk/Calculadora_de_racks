using System.Windows;
using System.Windows.Controls;
using RackCad.Application.StructuralSections;
using RackCad.Application.StructuralSections.Geometry;
using RackCad.UI.Controls;
using RackCad.UI.Shell;

namespace RackCad.UI.StructuralSections
{
    /// <summary>
    /// The minimum surface that lets a person SEE a catalogued section: pick one, give it a length, choose a
    /// view and check it (owner decision 22).
    ///
    /// It is NOT a member configurator and must not become one (decision 23). There is no role, no material,
    /// no loads, no levels, no arms, no connectors, no fabrication, no saving to a library, no persistence and
    /// no round-trip editing. Every one of those belongs to I-37.
    ///
    /// Built in code rather than XAML, like the other dialogs of the repository, and thin on purpose: all the
    /// logic lives in <see cref="StructuralSectionInspectorState"/>, which is testable without a dispatcher.
    ///
    /// T7 per (I-64, séptima tanda): its controls were extracted to <see cref="StructuralSectionInspectorControls"/>, so the page «Perfil
    /// estructural» of the RackCad window shows the very same ones over its own state. What this window adds is unchanged: its title and
    /// size, its shell, its two actions with their keyboard roles, its initial focus and its output contract (<see cref="Result"/> and
    /// <see cref="AcceptedSection"/>).
    /// </summary>
    public sealed class StructuralSectionInspectorWindow : Window
    {
        private readonly StructuralSectionInspectorControls _controls;
        private Button _accept;

        public StructuralSectionInspectorWindow(StructuralSectionCatalog catalog)
        {
            _controls = new StructuralSectionInspectorControls(new StructuralSectionInspectorState(catalog));
            _controls.Refreshing += UpdateInsertAvailability;

            Title = "Secciones estructurales";

            // I-39C: el tamano sale de los tokens del ARQUETIPO B, no de cuatro numeros repetidos aqui. Una ventana
            // construida en codigo no aplica el estilo de ventana por DynamicResource como hacen las de XAML, asi que
            // lee los mismos tokens del diccionario compartido. Los valores son los que esta ventana ya usaba —de
            // ella salieron cuando I-39A escribio el contrato—, de modo que su tamano observable NO cambia.
            Width = ShellResources.Get("BoundedEditorInitialWidth", 1040.0);
            Height = ShellResources.Get("BoundedEditorInitialHeight", 680.0);
            MinWidth = ShellResources.Get("BoundedEditorMinWidth", 820.0);
            MinHeight = ShellResources.Get("BoundedEditorMinHeight", 520.0);
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            Content = BuildLayout();

            // ADR-0029 D9, I-39C: el foco inicial es DETERMINISTA y declarado, y no recae en una accion destructiva
            // ni bloqueada. Sin declararlo, el foco caia donde el arbol visual del shell lo pusiera —y la plantilla
            // acopla la barra de acciones ARRIBA en el DockPanel, antes de la zona de parametros—. Aqui apunta a la
            // caja de busqueda, que es por donde empieza el trabajo de esta ventana.
            System.Windows.Input.FocusManager.SetFocusedElement(this, _controls.SearchBox);

            Loaded += (_, __) => _controls.Activate();
        }

        /// <summary>What the caller materialises when the user accepts. Null when they cancelled.</summary>
        public StructuralSectionRepresentationPlan Result { get; private set; }

        /// <summary>The section the user accepted, for reporting.</summary>
        public StructuralSectionDefinition AcceptedSection { get; private set; }

        /// <summary>The inspector's state, exposed for tests and for a caller that wants to preset it.</summary>
        public StructuralSectionInspectorState State => _controls.State;

        internal static string FamilyLabel(StructuralSectionFamily family) => StructuralSectionInspectorControls.FamilyLabel(family);

        private UIElement BuildLayout()
        {
            var actions = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            // I-39C: primer consumidor productivo de EditorAction. Antes esta ventana armaba sus dos botones a mano
            // porque la fabrica comun no sabia declarar accion por defecto ni de cancelacion, y sustituirlos habria
            // borrado Enter y Escape en silencio. Ahora la descripcion lleva el rol de teclado, asi que el contrato
            // que la caracterizacion de I-39A fijo se conserva y ademas los botones dejan de tener chrome propio:
            // usan los estilos compartidos, como los de las otras cinco ventanas del arquetipo.
            _accept = EditorActions.Button(
                new EditorAction("Insertar", isPrimary: true, isDefault: true),
                (_, __) => Accept());

            var cancel = EditorActions.Button(
                new EditorAction("Cerrar", isCancel: true),
                (_, __) => { Result = null; DialogResult = false; });

            _accept.MinWidth = 110;
            cancel.MinWidth = 110;

            actions.Children.Add(_accept);
            actions.Children.Add(cancel);

            // I-39A: the inspector is the first non-Cantilever consumer of the bounded-editor shell (archetype B of
            // ADR-0029). Its own controls are hosted AS THEY ARE — same instances, same handlers, same order — so
            // nothing about search, filtering, families, defaults, geometry authority or the output contract moves.
            //
            // Three zones stay empty on purpose. `Header`: the inspector never had one. `SectionPicker`: its search
            // and its list are part of the capture panel, and splitting them out would invert the visual order the
            // characterization pins. `BomSummary`: the inspector produces no BOM and must not start producing one
            // (owner decision 23 of I-36D).
            //
            // What the shell changes is composition, and that IS the point: diagnostics and actions leave the
            // preview column and become bands that span the window, always outside the parameters scroll, so they
            // stay reachable however long the capture panel grows.
            return new RackBoundedEditorShell
            {
                Parameters = _controls.Parameters,
                Preview = _controls.Preview,
                Diagnostics = _controls.Information,
                Actions = actions
            };
        }

        /// <summary>Rebuilds the plan and repaints. The single place the preview is refreshed.</summary>
        public void Refresh() => _controls.Refresh();

        /// <summary>
        /// I-39C paga la deuda que I-39A midió y dejó asignada al arquetipo: <c>Insertar</c> no se deshabilitaba
        /// nunca y sin selección era un no-op SILENCIOSO — ADR-0029 D6 llama a eso «una violación mayor», porque una
        /// acción habilitada sin efecto es peor que una bloqueada sin motivo. Ahora se apaga con el motivo visible.
        ///
        /// <para>La otra mitad de aquella medición —que una longitud o una rotación inválidas no bloquean— se revisó
        /// y NO se cambia: es correcta. D5 exige que una entrada inválida no sobrescriba en silencio un valor
        /// aplicado válido, y eso es justo lo que la ventana hace: el campo se pinta en ámbar y el estado conserva el
        /// último valor válido, así que lo que se inserta es un valor aplicado, no basura.</para>
        /// </summary>
        private void UpdateInsertAvailability()
        {
            if (_accept == null)
            {
                return;
            }

            _accept.IsEnabled = State.HasSelection;
            _accept.ToolTip = State.HasSelection
                ? null
                : "Elige una sección de la lista para poder insertarla.";
        }

        private void Accept()
        {
            if (!State.HasSelection)
            {
                return;
            }

            Result = State.BuildPlan();
            AcceptedSection = State.Selected;
            DialogResult = true;
        }
    }
}
