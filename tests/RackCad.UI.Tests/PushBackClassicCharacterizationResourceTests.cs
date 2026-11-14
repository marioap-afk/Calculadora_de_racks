using System;
using System.Linq;
using System.Windows;
using RackCad.UI.Shell;
using RackCad.UI.Systems.PushBack;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// F4-T1B (I-64) — CARACTERIZACION de los recursos de la ventana CLASICA de Push Back
    /// (<see cref="RackPushBackSystemWindow"/>): el diccionario fusionado, los tres tokens del ritmo de la barra de
    /// acciones, el estilo del shell y que el shell no necesite su respaldo. CT-15 del paquete v5.
    ///
    /// <para>Los recursos se comparan por EQUIVALENCIA y no por identidad (§9 I-6 del paquete): F5 puede mover la
    /// declaracion de sitio sin cambiar lo que la ventana resuelve. La plantilla del shell se aplica SIN mostrar la
    /// ventana, asi que estas pruebas no necesitan la coleccion del aviso de descarte.</para>
    /// </summary>
    public sealed class PushBackClassicCharacterizationResourceTests
    {
        private static string[] SettersOf(Style style)
            => style.Setters.OfType<Setter>().Select(setter => setter.Property.Name).ToArray();

        [Fact]
        public void TheWindowMergesAppStylesDeclaresTheThreeActionRhythmTokensAndUsesTheShellWindowStyle()
        {
            StaTestRunner.Run(() =>
            {
                var window = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                try
                {
                    // Los tres tokens locales del ritmo de la barra de acciones (I-42).
                    Assert.Equal(new Thickness(0, 0, 6, 4), (Thickness)window.Resources["ActionGap"]);

                    // Ancla (ningun mutante permitido las cambia): el resto de los tokens, el diccionario fusionado y el
                    // estilo, que se compara por equivalencia con el EditorShellWindowStyle que resuelve la ventana.
                    Assert.Equal(new Thickness(0, 0, 16, 4), (Thickness)window.Resources["ActionGroupGap"]);
                    Assert.Equal(new Thickness(0, 0, 0, 4), (Thickness)window.Resources["ActionLast"]);
                    Assert.Single(window.Resources.MergedDictionaries);
                    Assert.Equal("/RackCad.UI;component/Themes/AppStyles.xaml", window.Resources.MergedDictionaries[0].Source.OriginalString);
                    var resolved = Assert.IsType<Style>(window.TryFindResource("EditorShellWindowStyle"));
                    Assert.NotNull(window.Style);
                    Assert.Equal(typeof(Window), window.Style.TargetType);
                    Assert.Equal(SettersOf(resolved), SettersOf(window.Style));
                }
                finally { window.Close(); }
            });
        }

        [Fact]
        public void TheShellDoesNotActivateItsResourceFallbackOnceItsTemplateIsApplied()
        {
            StaTestRunner.Run(() =>
            {
                var window = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                try
                {
                    var shell = Assert.IsType<RackEditorVisualShell>(window.FindName("Shell"));
                    Assert.Same(shell, window.Content);

                    shell.ApplyTemplate();

                    // El shell solo fusiona ShellResources.Shared cuando el token ShellZoneSpacing NO resuelve: con la
                    // ventana aportando AppStyles, sus recursos propios quedan sin diccionarios fusionados.
                    Assert.NotNull(shell.TryFindResource("ShellZoneSpacing"));
                    Assert.Empty(shell.Resources.MergedDictionaries);
                }
                finally { window.Close(); }
            });
        }
    }
}
