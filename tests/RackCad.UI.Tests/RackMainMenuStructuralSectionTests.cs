using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using RackCad.UI;
using RackCad.UI.Editor;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-36C — el acceso desde el menu principal al generador de perfiles estructurales, que YA EXISTE.
    ///
    /// Lo que se prueba aqui es un ACCESO, no un generador: el boton existe, esta habilitado, dice lo que
    /// tiene que decir, esta donde tiene que estar, y al pulsarlo deja la accion tipada y cierra la ventana.
    /// Ni el catalogo ni la geometria pasan por aqui.
    ///
    /// La accion NO es un RackInsertionRequest, y eso tambien se comprueba: una seccion no es un rack —no
    /// tiene RackSystemKind, ni payload de diseño, ni round-trip— y meterla en ese contrato obligaria a
    /// inventar un Kind que el host tendria luego que tratar como caso especial.
    ///
    /// Sobre el arbol visual REAL, con reflexion; sin abrir ningun modal. Mismo patron que
    /// <see cref="RackMainMenuPushBackTests"/>.
    /// </summary>
    public sealed class RackMainMenuStructuralSectionTests
    {
        private const string Title = "Generar perfil estructural";

        private const string Caption =
            "Consulta perfiles W, HSS, C y L del catálogo AISC, visualiza su geometría e insértala en el dibujo.";

        // ---- El boton existe y dice lo que debe ----------------------------------------------------------

        [Fact]
        public void TheButtonExists_AndIsEnabled()
        {
            var (exists, enabled) = StaTestRunner.Run(() =>
            {
                var button = OptionButtons(new RackMainMenuWindow(canInsertInAutoCad: true))
                    .FirstOrDefault(b => TitleOf(b) == Title);
                return (button != null, button?.IsEnabled ?? false);
            });

            Assert.True(exists, "Falta el boton '" + Title + "' en el menu principal.");
            Assert.True(enabled, "El boton existe pero esta deshabilitado: el generador YA esta implementado.");
        }

        [Fact]
        public void TheButtonCarriesTheAgreedTextAndDescription()
        {
            var caption = StaTestRunner.Run(() =>
                CaptionOf(OptionButtons(new RackMainMenuWindow(canInsertInAutoCad: true))
                    .First(b => TitleOf(b) == Title)));

            Assert.Equal(Caption, caption);
        }

        [Fact]
        public void TheButtonSitsAfterLargueroAndBeforeTheLibrary()
        {
            var (index, larguero, library) = StaTestRunner.Run(() =>
            {
                var titles = OptionButtons(new RackMainMenuWindow(canInsertInAutoCad: true))
                    .Select(TitleOf).ToList();
                return (titles.IndexOf(Title),
                    titles.IndexOf("Diseñar larguero"),
                    titles.IndexOf("Abrir de la biblioteca de diseños"));
            });

            Assert.True(index >= 0);
            Assert.True(larguero >= 0 && index > larguero, "Debe ir DESPUES de 'Diseñar larguero'.");
            Assert.True(library >= 0 && index < library, "Debe ir ANTES de 'Abrir de la biblioteca de diseños'.");
        }

        [Fact]
        public void TheButtonUsesTheSameMenuButtonStyleAsTheRest()
        {
            // Integracion visual: el mismo estilo que los demas, no uno propio.
            var same = StaTestRunner.Run(() =>
            {
                var buttons = OptionButtons(new RackMainMenuWindow(canInsertInAutoCad: true));
                var mine = buttons.First(b => TitleOf(b) == Title);
                var neighbour = buttons.First(b => TitleOf(b) == "Diseñar larguero");
                return ReferenceEquals(mine.Style, neighbour.Style);
            });

            Assert.True(same);
        }

        [Fact]
        public void TheButtonIsWiredToItsOwnHandler()
        {
            var handlers = StaTestRunner.Run(() =>
                ClickHandlerNames(OptionButtons(new RackMainMenuWindow(canInsertInAutoCad: true))
                    .First(b => TitleOf(b) == Title)).ToArray());

            Assert.Contains("GenerateStructuralSection_Click", handlers);
        }

        // ---- Lo que pasa al pulsarlo ----------------------------------------------------------------------

        [Fact]
        public void ClickingIt_SetsTheTypedAction_AndClosesTheMenu()
        {
            var (action, closed, request) = StaTestRunner.Run(() =>
            {
                var window = new RackMainMenuWindow(canInsertInAutoCad: true);
                var wasClosed = false;
                window.Closed += (_, __) => wasClosed = true;

                OptionButtons(window).First(b => TitleOf(b) == Title)
                    .RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

                return (window.RequestedAction, wasClosed, window.InsertionRequest);
            });

            Assert.Equal(MainMenuAction.GenerateStructuralSection, action);
            Assert.True(closed, "El menu tiene que cerrarse: el flujo pide un punto y necesita el editor libre.");

            // Y NO viaja como insercion de rack: una seccion no tiene Kind, ni payload, ni round-trip.
            Assert.Null(request);
        }

        [Fact]
        public void AFreshMenuAsksForNothing()
        {
            var (action, request) = StaTestRunner.Run(() =>
            {
                var window = new RackMainMenuWindow(canInsertInAutoCad: true);
                return (window.RequestedAction, window.InsertionRequest);
            });

            Assert.Equal(MainMenuAction.None, action);
            Assert.Null(request);
        }

        [Fact]
        public void TheActionIsNotARackInsertionRequest()
        {
            // Guarda de modelado. Si alguien convierte esto en un RackInsertionRequest, tendra que inventarle
            // un RackSystemKind, y el host acabara con una rama especial para un rack que no existe.
            Assert.True(typeof(MainMenuAction).IsEnum);
            Assert.False(typeof(RackInsertionRequest).IsAssignableFrom(typeof(MainMenuAction)));

            var property = typeof(RackMainMenuWindow).GetProperty(
                "RequestedAction", BindingFlags.Instance | BindingFlags.Public);

            Assert.NotNull(property);
            Assert.Equal(typeof(MainMenuAction), property.PropertyType);
            Assert.Null(property.SetMethod?.IsPublic == true ? "publico" : null); // solo la ventana lo fija
        }

        // ---- Lo que NO cambio ------------------------------------------------------------------------------

        [Fact]
        public void TheSixDesignButtonsKeepTheirOrderAndWiring()
        {
            var (titles, handlers) = StaTestRunner.Run(() =>
            {
                var buttons = OptionButtons(new RackMainMenuWindow(canInsertInAutoCad: true));
                return (buttons.Select(TitleOf).ToList(),
                    buttons.ToDictionary(TitleOf, b => ClickHandlerNames(b).FirstOrDefault()));
            });

            var expected = new[]
            {
                "Diseñar sistema selectivo",
                "Diseñar sistema dinámico (Pallet Flow)",
                "Diseñar sistema Push Back",
                "Diseñar cabecera",
                "Diseñar cama de rodamiento",
                "Diseñar larguero",
            };

            Assert.Equal(expected, titles.Where(t => expected.Contains(t)).ToList());

            Assert.Equal("DesignSelective_Click", handlers["Diseñar sistema selectivo"]);
            Assert.Equal("DesignDynamic_Click", handlers["Diseñar sistema dinámico (Pallet Flow)"]);
            Assert.Equal("DesignPushBack_Click", handlers["Diseñar sistema Push Back"]);
            Assert.Equal("DesignHeader_Click", handlers["Diseñar cabecera"]);
            Assert.Equal("DesignFlowBed_Click", handlers["Diseñar cama de rodamiento"]);
            Assert.Equal("DesignLarguero_Click", handlers["Diseñar larguero"]);
            Assert.Equal("OpenDesignLibrary_Click", handlers["Abrir de la biblioteca de diseños"]);
        }

        [Fact]
        public void TheMenuStillExposesExactlyOneInsertionRequest()
        {
            var props = typeof(RackMainMenuWindow)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(p => p.Name).ToList();

            Assert.Contains("InsertionRequest", props);
            Assert.Contains("RequestedAction", props);
            Assert.DoesNotContain(props, n => n.EndsWith("ToInsert", StringComparison.Ordinal));
            Assert.DoesNotContain(props, n => n.Contains("Section", StringComparison.Ordinal)); // sin bolsa de propiedades por seccion
        }

        [Fact]
        public void TheUiStillDoesNotReferenceAutodesk()
        {
            // La direccion de dependencias de AGENTS: el boton informa una accion; quien habla con AutoCAD es
            // el Plugin, y solo despues de que esta ventana se haya cerrado.
            var referenced = typeof(RackMainMenuWindow).Assembly
                .GetReferencedAssemblies().Select(a => a.Name).ToArray();

            Assert.DoesNotContain(referenced, name => name.StartsWith("Ac", StringComparison.Ordinal) ||
                                                      name.Contains("Autodesk", StringComparison.OrdinalIgnoreCase));
        }

        // ---- Helpers (arbol visual real + reflexion) --------------------------------------------------------

        private static List<Button> OptionButtons(DependencyObject root)
        {
            var buttons = new List<Button>();

            void Walk(DependencyObject node)
            {
                foreach (var child in LogicalTreeHelper.GetChildren(node))
                {
                    if (child is Button button && button.Content is StackPanel)
                    {
                        buttons.Add(button);
                    }

                    if (child is DependencyObject dependency)
                    {
                        Walk(dependency);
                    }
                }
            }

            Walk(root);
            return buttons;
        }

        private static string TitleOf(Button button) =>
            (button.Content as StackPanel)?.Children.OfType<TextBlock>().FirstOrDefault()?.Text;

        private static string CaptionOf(Button button) =>
            (button.Content as StackPanel)?.Children.OfType<TextBlock>().Skip(1).FirstOrDefault()?.Text;

        private static IEnumerable<string> ClickHandlerNames(Button button)
        {
            var storeProperty = typeof(UIElement).GetProperty(
                "EventHandlersStore", BindingFlags.Instance | BindingFlags.NonPublic);
            var store = storeProperty?.GetValue(button);

            if (store == null)
            {
                return Array.Empty<string>();
            }

            var getHandlers = store.GetType().GetMethod("GetRoutedEventHandlers", new[] { typeof(RoutedEvent) });

            if (getHandlers == null ||
                !(getHandlers.Invoke(store, new object[] { ButtonBase.ClickEvent }) is Array infos))
            {
                return Array.Empty<string>();
            }

            var names = new List<string>();

            foreach (var info in infos)
            {
                if (info.GetType().GetProperty("Handler")?.GetValue(info) is Delegate handler)
                {
                    names.Add(handler.Method.Name);
                }
            }

            return names;
        }
    }
}
