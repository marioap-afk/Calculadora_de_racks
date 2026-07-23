using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using RackCad.Domain.Systems;
using RackCad.UI;
using RackCad.UI.Editor;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-18b increment 3b — STA tests for the REAL <see cref="RackMainMenuWindow"/>: Push Back is a visible menu option
    /// (after Dinámico, before Cabecera), wired to <c>DesignPushBack_Click</c>, and the library caption lists it. The menu
    /// still exposes ONE typed <c>InsertionRequest</c> (no per-system Push Back payload props) and still dispatches the
    /// library through the shared <see cref="EditorModuleRegistry.Default"/>. Verified on the real visual tree + reflection,
    /// not on source text; no modal dialog is opened.
    /// </summary>
    public sealed class RackMainMenuPushBackTests
    {
        [Fact]
        public void PushBackButton_Exists_AfterDynamic_BeforeHeader_WithExactTitle()
        {
            var (titles, pbIndex, dynIndex, headerIndex) = StaTestRunner.Run(() =>
            {
                var window = new RackMainMenuWindow(canInsertInAutoCad: true);
                var t = OptionButtons(window).Select(Title).ToList();
                return (t, t.IndexOf("Diseñar sistema Push Back"),
                    t.IndexOf("Diseñar sistema dinámico (Pallet Flow)"), t.IndexOf("Diseñar cabecera"));
            });

            Assert.Contains("Diseñar sistema Push Back", titles); // the exact visible title uses "Push Back"
            Assert.True(pbIndex >= 0);
            Assert.True(dynIndex >= 0 && pbIndex > dynIndex);      // after Dinámico
            Assert.True(headerIndex >= 0 && pbIndex < headerIndex); // before Cabecera
        }

        [Fact]
        public void PushBackButton_IsWiredTo_DesignPushBack_Click()
        {
            var handlers = StaTestRunner.Run(() =>
            {
                var window = new RackMainMenuWindow(canInsertInAutoCad: true);
                var button = OptionButtons(window).First(b => Title(b) == "Diseñar sistema Push Back");
                return ClickHandlerNames(button).ToArray();
            });

            Assert.Contains("DesignPushBack_Click", handlers); // the real Click wiring, read off the button
        }

        [Fact]
        public void Menu_ExposesOneTypedInsertionRequest_NoPerSystemPushBackPayloadProps()
        {
            var props = typeof(RackMainMenuWindow)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(p => p.Name).ToList();

            Assert.Contains("InsertionRequest", props);
            Assert.DoesNotContain(props, n => n.StartsWith("PushBack", StringComparison.Ordinal)); // no PushBack{System,Design}ToInsert / PushBackRackId / PushBackRackName
            Assert.DoesNotContain(props, n => n.EndsWith("ToInsert", StringComparison.Ordinal));   // the single-contract shape (I-15) is preserved

            // The delegate itself exists (a private void handler over the shared launcher).
            Assert.NotNull(typeof(RackMainMenuWindow).GetMethod("DesignPushBack_Click", BindingFlags.Instance | BindingFlags.NonPublic));
        }

        [Fact]
        public void OpenDesignLibrary_UsesTheSharedDefaultRegistry()
        {
            var usesDefault = StaTestRunner.Run(() =>
            {
                var window = new RackMainMenuWindow(canInsertInAutoCad: true);
                var field = typeof(RackMainMenuWindow).GetField("registry", BindingFlags.Instance | BindingFlags.NonPublic);
                return ReferenceEquals(field?.GetValue(window), EditorModuleRegistry.Default);
            });

            Assert.True(usesDefault); // the library dispatch runs through EditorModuleRegistry.Default (no special Push Back branch)
        }

        [Fact]
        public void LibraryCaption_ListsPushBack()
        {
            var caption = StaTestRunner.Run(() =>
            {
                var window = new RackMainMenuWindow(canInsertInAutoCad: true);
                var library = OptionButtons(window).First(b => Title(b) == "Abrir de la biblioteca de diseños");
                return Caption(library);
            });

            Assert.Contains("Push Back", caption);
        }

        [Fact]
        public void AllSixDesignButtons_ArePresent_InOrder()
        {
            var titles = StaTestRunner.Run(() =>
                OptionButtons(new RackMainMenuWindow(canInsertInAutoCad: true)).Select(Title).ToList());

            var expected = new[]
            {
                "Diseñar sistema selectivo",
                "Diseñar sistema dinámico (Pallet Flow)",
                "Diseñar sistema Push Back",
                "Diseñar cabecera",
                "Diseñar cama de rodamiento",
                "Diseñar larguero",
            };
            var design = titles.Where(t => expected.Contains(t)).ToList();
            Assert.Equal(expected, design); // the prior five plus Push Back, in the menu's button order
        }

        // ---- Helpers (real visual tree + reflection) ----

        private static List<Button> OptionButtons(DependencyObject root)
        {
            var buttons = new List<Button>();
            void Walk(DependencyObject node)
            {
                foreach (var child in LogicalTreeHelper.GetChildren(node))
                {
                    if (child is Button button && button.Content is StackPanel) buttons.Add(button); // the menu "option" buttons carry a StackPanel
                    if (child is DependencyObject dependency) Walk(dependency);
                }
            }

            Walk(root);
            return buttons;
        }

        private static string Title(Button button)
            => (button.Content as StackPanel)?.Children.OfType<TextBlock>().FirstOrDefault()?.Text ?? button.Content as string;

        private static string Caption(Button button)
            => (button.Content as StackPanel)?.Children.OfType<TextBlock>().Skip(1).FirstOrDefault()?.Text;

        private static IEnumerable<string> ClickHandlerNames(Button button)
        {
            var storeProperty = typeof(UIElement).GetProperty("EventHandlersStore", BindingFlags.Instance | BindingFlags.NonPublic);
            var store = storeProperty?.GetValue(button);
            if (store == null) return Array.Empty<string>();

            var getHandlers = store.GetType().GetMethod("GetRoutedEventHandlers", new[] { typeof(RoutedEvent) });
            if (getHandlers == null || !(getHandlers.Invoke(store, new object[] { ButtonBase.ClickEvent }) is Array infos))
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
