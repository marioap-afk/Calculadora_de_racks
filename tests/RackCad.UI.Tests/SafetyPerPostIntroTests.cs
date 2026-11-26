using System.Linq;
using System.Windows.Controls;
using RackCad.Domain.Systems.Selective;
using RackCad.UI;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// The per-post side editor of the safety dialog («Por poste…») says which general side «(por defecto)» follows. Found in the
    /// AutoCAD smoke of the entrega 2 of the Selectivo of the RackCad window (I-64): with a general «Ambas» it said «(Derecha)», because
    /// the labels start with «Ninguno» and the text read them one place too early.
    /// </summary>
    public sealed class SafetyPerPostIntroTests
    {
        private static readonly string[] SideLabels = { "Ninguno", "Izquierda", "Derecha", "Ambas" };
        private static readonly string[] BootLabels = { "Ninguno", "Entrada/Salida", "Posterior", "Ambas" };

        private static string Intro(SafetySide general, string[] labels)
        {
            var window = new SafetyPerPostWindow("Protector bota tipo H", 3, general, Enumerable.Empty<SafetyPostSide>(), labels);
            try
            {
                return EditorWindowTestSupport.FindAll<TextBlock>(window)
                    .Select(t => t.Text)
                    .Single(text => text != null && text.Contains("lado por poste"));
            }
            finally
            {
                window.Close();
            }
        }

        [Theory]
        [InlineData(SafetySide.Left, "Izquierda")]
        [InlineData(SafetySide.Right, "Derecha")]
        [InlineData(SafetySide.Both, "Ambas")]
        public void TheDefaultNamesTheGeneralSide(SafetySide general, string expected)
        {
            StaTestRunner.Run(() =>
            {
                Assert.EndsWith("\"(por defecto)\" usa la selección general (" + expected + ").", Intro(general, SideLabels));
            });
        }

        [Theory]
        [InlineData(SafetySide.Left, "Entrada/Salida")]
        [InlineData(SafetySide.Right, "Posterior")]
        [InlineData(SafetySide.Both, "Ambas")]
        public void WithThePlacementNamesTheDefaultNamesTheGeneralPlacement(SafetySide general, string expected)
        {
            StaTestRunner.Run(() =>
            {
                Assert.EndsWith("\"(por defecto)\" usa la selección general (" + expected + ").", Intro(general, BootLabels));
            });
        }

        [Fact]
        public void WithoutAGeneralSideTheDefaultIsNone()
        {
            StaTestRunner.Run(() =>
            {
                Assert.EndsWith("\"(por defecto)\" = no lleva.", Intro(SafetySide.None, SideLabels));
            });
        }
    }
}
