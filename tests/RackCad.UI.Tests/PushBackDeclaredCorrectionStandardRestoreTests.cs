using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using RackCad.UI.Systems.PushBack;
using Xunit;
using static RackCad.UI.Tests.PushBackDeclaredCorrectionSupport;
using static RackCad.UI.Tests.PushBackFullEditorCharacterizationSupport;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 F4B PB-TC — D-PB-11 (H-PB-28) en la ventana CLASICA de Push Back: tras «Restaurar estandar» y «Confirmar», los seis
    /// parametros avanzados del rack muestran el estandar en sus controles y «Actualizar» escribe lo reiniciado. Sustituye a CT-PB0-05,
    /// que fijaba la perdida; los pasos y las lineas que no cambian son los suyos, copiados.
    ///
    /// <para>Toda ventana se crea con <see cref="PushBackFullEditorCharacterizationSupport.Ventana"/> y su carga va dentro del
    /// <c>try</c>; ninguna se muestra y no se usa el teclado ni el foco real.</para>
    /// </summary>
    [Collection(EditorDiscardPromptCollection.Name)]
    public sealed class PushBackDeclaredCorrectionStandardRestoreTests
    {
        private static string Six(string persisted)
        {
            using (var document = JsonDocument.Parse(persisted))
            {
                var structure = document.RootElement.GetProperty("PushBack").GetProperty("Structure");
                return string.Join(
                    ",",
                    new[] { "ManualHeaderHeightOverride", "DerivedPostReinforced", "DerivedPostReinforcementHeight", "DerivedPostHeight", "SeparatorCountOverride", "SeparatorSpacingOverride" }
                        .Select(name => name + "=" + structure.GetProperty(name).GetRawText()));
            }
        }

        private static string SixControls(RackPushBackSystemWindow w)
            => "RackHeaderHeightBox=[" + Field(w, "RackHeaderHeightBox").Text + "] DerivedPostReinforcedCheck=" + Check(w, "DerivedPostReinforcedCheck").IsChecked
               + " DerivedPostReinforcementHeightBox=[" + Field(w, "DerivedPostReinforcementHeightBox").Text + "] DerivedPostHeightBox=[" + Field(w, "DerivedPostHeightBox").Text
               + "] RackSeparatorCountBox=[" + Field(w, "RackSeparatorCountBox").Text + "] RackSeparatorSpacingBox=[" + Field(w, "RackSeparatorSpacingBox").Text + "]";

        [Fact]
        public void CT_PBTC_14_AStandardRestoreResetsTheAdvancedControlsAndReachesTheUpdate()
        {
            var log = new List<string>();
            Sta(() =>
            {
                foreach (var fixture in new[] { "pb-06-seguridad", "pb-07-combinado" })
                {
                    var w = Ventana();
                    try
                    {
                        Cargar(w, Fixture(fixture));
                        Check(w, "AdvancedModulesToggle").IsChecked = true;
                        log.Add(fixture + " A0 salida: " + Six(Salida(w)));
                        log.Add(fixture + " A0 controles: " + SixControls(w));
                        Click(w, "RestoreAllModulesButton");
                        Click(w, "ConfirmModuleButton");
                        log.Add(fixture + " A1 salida: " + Six(Salida(w)));
                        log.Add(fixture + " A1 controles: " + SixControls(w));
                        Click(w, "UpdateButton");
                        Assert.True(w.InsertRequested);
                        log.Add(fixture + " A2 peticion: " + Six(Persist(w.DesignToInsert)));
                    }
                    finally { Cerrar(w); }
                }
            });

            Assert.Equal(
                new[]
                {
                    "pb-06-seguridad A0 salida: ManualHeaderHeightOverride=150,DerivedPostReinforced=false,DerivedPostReinforcementHeight=null,DerivedPostHeight=null,SeparatorCountOverride=3,SeparatorSpacingOverride=18",
                    "pb-06-seguridad A0 controles: RackHeaderHeightBox=[150] DerivedPostReinforcedCheck=False DerivedPostReinforcementHeightBox=[] DerivedPostHeightBox=[] RackSeparatorCountBox=[3] RackSeparatorSpacingBox=[18]",
                    "pb-06-seguridad A1 salida: ManualHeaderHeightOverride=null,DerivedPostReinforced=true,DerivedPostReinforcementHeight=null,DerivedPostHeight=null,SeparatorCountOverride=null,SeparatorSpacingOverride=null",
                    "pb-06-seguridad A1 controles: RackHeaderHeightBox=[] DerivedPostReinforcedCheck=True DerivedPostReinforcementHeightBox=[] DerivedPostHeightBox=[] RackSeparatorCountBox=[] RackSeparatorSpacingBox=[]",
                    "pb-06-seguridad A2 peticion: ManualHeaderHeightOverride=null,DerivedPostReinforced=true,DerivedPostReinforcementHeight=null,DerivedPostHeight=null,SeparatorCountOverride=null,SeparatorSpacingOverride=null",
                    "pb-07-combinado A0 salida: ManualHeaderHeightOverride=null,DerivedPostReinforced=true,DerivedPostReinforcementHeight=null,DerivedPostHeight=140,SeparatorCountOverride=null,SeparatorSpacingOverride=null",
                    "pb-07-combinado A0 controles: RackHeaderHeightBox=[] DerivedPostReinforcedCheck=True DerivedPostReinforcementHeightBox=[] DerivedPostHeightBox=[140] RackSeparatorCountBox=[] RackSeparatorSpacingBox=[]",
                    "pb-07-combinado A1 salida: ManualHeaderHeightOverride=null,DerivedPostReinforced=true,DerivedPostReinforcementHeight=null,DerivedPostHeight=null,SeparatorCountOverride=null,SeparatorSpacingOverride=null",
                    "pb-07-combinado A1 controles: RackHeaderHeightBox=[] DerivedPostReinforcedCheck=True DerivedPostReinforcementHeightBox=[] DerivedPostHeightBox=[] RackSeparatorCountBox=[] RackSeparatorSpacingBox=[]",
                    "pb-07-combinado A2 peticion: ManualHeaderHeightOverride=null,DerivedPostReinforced=true,DerivedPostReinforcementHeight=null,DerivedPostHeight=null,SeparatorCountOverride=null,SeparatorSpacingOverride=null",
                },
                log);
        }
    }
}
