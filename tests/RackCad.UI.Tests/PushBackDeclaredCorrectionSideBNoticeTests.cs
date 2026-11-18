using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RackCad.UI.Shell;
using RackCad.UI.Systems.PushBack;
using Xunit;
using static RackCad.UI.Tests.PushBackDeclaredCorrectionSupport;
using static RackCad.UI.Tests.PushBackFullEditorCharacterizationSupport;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 F4B PB-TC — D-PB-08 en la ventana CLASICA de Push Back: un aviso de SOLO TEXTO en la banda de estado mientras el lado B
    /// esta declarado y ninguna de sus ranuras esta activa (el diseño sale entonces sin lado B). CT-PBTC-07 fija cuando se ve, con el
    /// literal que eligio el Owner; CT-PBTC-08 fija que es texto, que esta en la banda de estado y que no bloquea ninguna accion.
    ///
    /// <para>Los gestos de CT-07 son los de CT-PB0-09 (<c>pb-01-base</c>) y CT-PB0-07 (<c>pb-20-ab</c>), copiados. Toda ventana se crea
    /// con <see cref="PushBackFullEditorCharacterizationSupport.Ventana"/> y su carga va dentro del <c>try</c>; ninguna se muestra.</para>
    /// </summary>
    [Collection(EditorDiscardPromptCollection.Name)]
    public sealed class PushBackDeclaredCorrectionSideBNoticeTests
    {
        private const string Grid = "PushBackMatrixGrid";

        private static void Blank(RackPushBackSystemWindow w, int front, bool blank) => EditorWindowTestSupport.SetFrontBlank(w, Grid, front, blank);

        private static void Side(RackPushBackSystemWindow w, int index) => Combo(w, "SideSelectorBox").SelectedIndex = index;

        private static void DeclareB(RackPushBackSystemWindow w, bool present) => Check(w, "SideBPresentCheck").IsChecked = present;

        /// <summary><c>&lt;paso&gt; aviso=&lt;Visibility&gt; compuesto=&lt;IsComposite&gt;</c>. Primero toca el aviso: sin el, falla ahi.</summary>
        private static string Paso(RackPushBackSystemWindow w, string paso)
        {
            var aviso = Aviso(w).Visibility;
            return paso + " aviso=" + aviso + " compuesto=" + w.LastComputation.Design.IsComposite;
        }

        [Fact]
        public void CT_PBTC_07_TheSideBNoticeShowsExactlyWhileSideBIsDeclaredWithoutAnActiveFront()
        {
            var log = new List<string>();
            Sta(() =>
            {
                var w = Ventana();
                try
                {
                    Cargar(w, Fixture("pb-01-base"));
                    log.Add(Paso(w, "pb-01-base carga"));
                    DeclareB(w, true);
                    log.Add(Paso(w, "pb-01-base declarar B"));
                    log.Add("texto=" + Aviso(w).Text);
                    Click(w, "AddFrontButton");
                    log.Add(Paso(w, "pb-01-base anadir frente"));
                    Side(w, 1);
                    Blank(w, 1, false);
                    log.Add(Paso(w, "pb-01-base lado B, ranura 1 de B activa"));
                    Side(w, 0);
                    Blank(w, 1, true);
                    Blank(w, 2, true);
                    log.Add(Paso(w, "pb-01-base ranuras 1 y 2 de A en blanco"));
                    Side(w, 1);
                    Blank(w, 1, true);
                    log.Add(Paso(w, "pb-01-base ranura 1 de B en blanco"));
                    DeclareB(w, false);
                    log.Add(Paso(w, "pb-01-base retirar B"));
                }
                finally { Cerrar(w); }

                var ab = Ventana();
                try
                {
                    Cargar(ab, Fixture("pb-20-ab"));
                    log.Add(Paso(ab, "pb-20-ab carga"));
                    DeclareB(ab, false);
                    log.Add(Paso(ab, "pb-20-ab retirar B"));
                    DeclareB(ab, true);
                    log.Add(Paso(ab, "pb-20-ab declarar B"));
                    Side(ab, 1);
                    for (var slot = 0; slot < ab.CompositeState.SideB.Structure.Count; slot++) Blank(ab, slot, true);
                    log.Add(Paso(ab, "pb-20-ab todas las ranuras de B en blanco"));
                }
                finally { Cerrar(ab); }
            });

            Assert.Equal(
                new[]
                {
                    "pb-01-base carga aviso=Collapsed compuesto=False",
                    "pb-01-base declarar B aviso=Visible compuesto=False",
                    "texto=Lado B declarado sin ningún frente activo: así no se guardará. Para conservarlo, elige «Lado B» y desmarca «En blanco» en al menos un frente.",
                    "pb-01-base anadir frente aviso=Visible compuesto=False",
                    "pb-01-base lado B, ranura 1 de B activa aviso=Collapsed compuesto=True",
                    "pb-01-base ranuras 1 y 2 de A en blanco aviso=Collapsed compuesto=True",
                    "pb-01-base ranura 1 de B en blanco aviso=Visible compuesto=False",
                    "pb-01-base retirar B aviso=Collapsed compuesto=False",
                    "pb-20-ab carga aviso=Collapsed compuesto=True",
                    "pb-20-ab retirar B aviso=Collapsed compuesto=False",
                    "pb-20-ab declarar B aviso=Collapsed compuesto=True",
                    "pb-20-ab todas las ranuras de B en blanco aviso=Visible compuesto=False",
                },
                log);
        }

        [Fact]
        public void CT_PBTC_08_TheSideBNoticeIsTextOnlyAndBlocksNoAction()
        {
            var log = new List<string>();
            Sta(() =>
            {
                var w = Ventana();
                try
                {
                    Cargar(w, Fixture("pb-01-base"));
                    DeclareB(w, true);
                    var shell = (RackEditorVisualShell)w.FindName("Shell");
                    var aviso = Aviso(w);
                    log.Add("aviso=" + aviso.Visibility + " control=" + ((DependencyObject)aviso is Control) + " enfocable=" + aviso.Focusable
                        + " actualizar-habilitado=" + Btn(w, "UpdateButton").IsEnabled);
                    log.Add("lugar estado=" + EditorWindowTestSupport.Descendants((DependencyObject)shell.StatusContent).Contains(aviso)
                        + " barra-lateral=" + EditorWindowTestSupport.Descendants((DependencyObject)shell.SidePanelContent).Contains(aviso));
                    Click(w, "UpdateButton");
                    log.Add("peticion=" + w.InsertRequested + " updateOnly=" + w.UpdateOnly + " compuesto=" + w.DesignToInsert.IsComposite);
                }
                finally { Cerrar(w); }
            });

            Assert.Equal(
                new[]
                {
                    "aviso=Visible control=False enfocable=False actualizar-habilitado=True",
                    "lugar estado=True barra-lateral=False",
                    "peticion=True updateOnly=True compuesto=False",
                },
                log);
        }
    }
}
