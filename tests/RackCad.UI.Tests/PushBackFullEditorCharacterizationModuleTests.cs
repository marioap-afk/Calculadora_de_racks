using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using RackCad.UI.Systems.PushBack;
using Xunit;
using static RackCad.UI.Tests.PushBackFullEditorCharacterizationSupport;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 F4B PB-T0 — CARACTERIZACION de los modulos de un rack CARGADO en la ventana clasica de Push Back (CT-PB0-03,
    /// CT-PB0-04 y CT-PB0-05): escenificar, confirmar y cancelar una edicion; anadir y quitar modulos por
    /// reconstruccion; y los seis parametros avanzados tras «Restaurar estandar» (H-PB-28).
    ///
    /// <para><b>Fija lo que HAY.</b> Lo que contradice una regla de la ventana lleva «INCUMPLIMIENTO CARACTERIZADO» con la
    /// regla citada y NO se corrige (No product cleanup in PB-T0). Ninguna prueba muestra la ventana, ejecuta Insertar,
    /// «Guardar en biblioteca», el BOM o el configurador real, ni teclea: los campos se escriben con «R» y se comprometen
    /// elevando <c>LostFocus</c>. Toda ventana se cierra en un <c>finally</c>.</para>
    /// </summary>
    [Collection(EditorDiscardPromptCollection.Name)]
    public sealed class PushBackFullEditorCharacterizationModuleTests
    {
        private static string Reconciliation(RackPushBackSystemWindow w)
        {
            var r = w.EditorStateForTest.LastModuleReconciliation;
            Assert.NotNull(r);
            return "Preserved=[" + string.Join(",", r.Preserved) + "] Adapted=[" + string.Join(",", r.Adapted) + "] Removed=[" + string.Join(",", r.Removed)
                + "] Incompatible=[" + string.Join(",", r.Incompatible) + "] Restored=[" + string.Join(",", r.Restored) + "]";
        }

        private static string Pending(RackPushBackSystemWindow w) => w.PendingWork().HasPendingWork ? "pendiente" : "vacio";


        private static void StageLength(RackPushBackSystemWindow w, string moduleId, double increment)
        {
            SelectModule(w, moduleId);
            SetNumberR(w, "ModuleLengthBox", ValueOf(w, "ModuleLengthBox").Value + increment);
        }

        /// <summary>CT-PB0-03 sobre un diseño: el editado se confirma, el cancelado se descarta y una reconstruccion lo mueve todo.</summary>
        private static List<string> Stage(string fixture, string edited, string cancelled)
        {
            var log = new List<string>();
            var w = Abrir(Fixture(fixture));
            try
            {
                var s0 = Salida(w);
                log.Add(fixture + " carga " + ModuleSequence(w) + " | " + Reconciliation(w));

                // 1. Escenificar: la edicion queda pendiente y la salida NO cambia.
                StageLength(w, edited, 8.0);
                log.Add("1 salida-igual=" + (Salida(w) == s0) + " " + Pending(w));

                // 2. «Confirmar»: confirma y recalcula; la edicion llega a la salida.
                Click(w, "ConfirmModuleButton");
                var confirmed = Salida(w);
                log.Add("2 " + Pending(w) + " delta=" + string.Join(" ;; ", Delta(s0, confirmed)));

                // 3. Otro modulo, escenificado y cancelado: la salida sigue siendo la confirmada.
                StageLength(w, cancelled, 4.0);
                var pendingBeforeCancel = Pending(w);
                Click(w, "CancelModuleButton");
                log.Add("3 " + pendingBeforeCancel + " salida-igual-a-la-confirmada=" + (Salida(w) == confirmed) + " " + Pending(w));

                // 4. Fondo + 6: fuerza la reconstruccion y la reconciliacion.
                SetNumberR(w, "DepthBox", ValueOf(w, "DepthBox").Value + 6.0);
                log.Add("4 " + ModuleSequence(w) + " | " + Reconciliation(w));
            }
            finally { Cerrar(w); }

            return log;
        }

        [Fact]
        public void CT_PB0_03_StagingConfirmingAndCancellingAModuleEditInALoadedRackMovesTheOutputOnlyOnConfirm()
        {
            var log = new List<string>();
            Sta(() =>
            {
                log.AddRange(Stage("pb-31-v2-modulos", "M2", "M4"));
                log.AddRange(Stage("pb-20-ab", "B:M2", "B:M4"));
            });

            Assert.Equal(
                new[]
                {
                    "pb-31-v2-modulos carga M1/HeaderStart/54/calc/-;M2/Separator/48/calc/-;M3/HeaderIntermediate/48/-/manual;M4/Separator/48/calc/-;M5/Separator/48/calc/-;M6/HeaderIntermediate/48/calc/-;M7/Separator/48/calc/-;M8/HeaderEnd/54/calc/- | Preserved=[M3] Adapted=[] Removed=[] Incompatible=[] Restored=[]",
                    "1 salida-igual=True pendiente",
                    "2 vacio delta=$.PushBack.Structure.Modules[1].IsCalculated: true -> false ;; $.PushBack.Structure.Modules[1].IsManualOverride: false -> true ;; $.PushBack.Structure.Modules[1].Length: 48 -> 56",
                    "3 pendiente salida-igual-a-la-confirmada=True vacio",
                    "4 M1/HeaderStart/60/calc/-;M2/Separator/56/-/manual;M3/HeaderIntermediate/48/-/manual;M4/Separator/54/calc/-;M5/Separator/54/calc/-;M6/HeaderIntermediate/54/calc/-;M7/Separator/54/calc/-;M8/HeaderEnd/60/calc/- | Preserved=[M2,M3] Adapted=[] Removed=[] Incompatible=[] Restored=[]",
                    "pb-20-ab carga M1/HeaderStart/54/calc/-;M2/Separator/48/calc/-;M3/HeaderIntermediate/48/calc/-;M4/Separator/48/calc/-;M5/Separator/48/calc/-;M6/HeaderIntermediate/48/calc/-;M7/Separator/48/calc/-;M8/HeaderEnd/54/calc/-;GAP/Gap/54/-/manual;B:M8/HeaderEnd/54/calc/-;B:M7/Separator/48/calc/-;B:M6/HeaderIntermediate/48/calc/-;B:M5/Separator/48/calc/-;B:M4/Separator/48/calc/-;B:M3/HeaderIntermediate/48/calc/-;B:M2/Separator/48/calc/-;B:M1/HeaderStart/54/calc/- | Preserved=[] Adapted=[] Removed=[] Incompatible=[] Restored=[]",
                    "1 salida-igual=True pendiente",
                    "2 vacio delta=$.PushBack.Structure.Modules[15].IsCalculated: true -> false ;; $.PushBack.Structure.Modules[15].IsManualOverride: false -> true ;; $.PushBack.Structure.Modules[15].Length: 48 -> 56",
                    "3 pendiente salida-igual-a-la-confirmada=True vacio",
                    "4 M1/HeaderStart/60/calc/-;M2/Separator/54/calc/-;M3/HeaderIntermediate/54/calc/-;M4/Separator/54/calc/-;M5/Separator/54/calc/-;M6/HeaderIntermediate/54/calc/-;M7/Separator/54/calc/-;M8/HeaderEnd/60/calc/-;GAP/Gap/54/-/manual;B:M8/HeaderEnd/54/calc/-;B:M7/Separator/48/calc/-;B:M6/HeaderIntermediate/48/calc/-;B:M5/Separator/48/calc/-;B:M4/Separator/48/calc/-;B:M3/HeaderIntermediate/48/calc/-;B:M2/Separator/56/-/manual;B:M1/HeaderStart/54/calc/- | Preserved=[GAP,B:M2] Adapted=[] Removed=[] Incompatible=[] Restored=[]",
                },
                log);
        }

        [Fact]
        public void CT_PB0_04_AddingDepthRebuildsTheModulesAndRestoringTheStandardReturnsEveryModuleToCalculated()
        {
            var log = new List<string>();
            Sta(() =>
            {
                var w = Abrir(Fixture("pb-31-v2-modulos"));
                try
                {
                    log.Add("carga " + ModuleSequence(w));

                    // 1. Fondo + 12 (reconstruye la estructura) y 2. Fondos + 1.
                    SetNumberR(w, "DepthBox", ValueOf(w, "DepthBox").Value + 12.0);
                    log.Add("1 " + ModuleSequence(w) + " | " + Reconciliation(w));
                    SetNumberR(w, "PalletsDeepBox", ValueOf(w, "PalletsDeepBox").Value + 1.0);
                    log.Add("2 " + ModuleSequence(w) + " | " + Reconciliation(w));

                    // 3. M2 + 8 y «Confirmar».
                    StageLength(w, "M2", 8.0);
                    Click(w, "ConfirmModuleButton");
                    log.Add("3 " + ModuleSequence(w) + " | " + Reconciliation(w));

                    // 4. «Restaurar estandar» y «Confirmar»: todos los modulos vuelven a calculados.
                    Click(w, "RestoreAllModulesButton");
                    Click(w, "ConfirmModuleButton");
                    log.Add("4 " + ModuleSequence(w) + " | " + Reconciliation(w));
                }
                finally { Cerrar(w); }
            });

            Assert.Equal(
                new[]
                {
                    "carga M1/HeaderStart/54/calc/-;M2/Separator/48/calc/-;M3/HeaderIntermediate/48/-/manual;M4/Separator/48/calc/-;M5/Separator/48/calc/-;M6/HeaderIntermediate/48/calc/-;M7/Separator/48/calc/-;M8/HeaderEnd/54/calc/-",
                    "1 M1/HeaderStart/66/calc/-;M2/Separator/60/calc/-;M3/HeaderIntermediate/48/-/manual;M4/Separator/60/calc/-;M5/Separator/60/calc/-;M6/HeaderIntermediate/60/calc/-;M7/Separator/60/calc/-;M8/HeaderEnd/66/calc/- | Preserved=[M3] Adapted=[] Removed=[] Incompatible=[] Restored=[]",
                    "2 M1/HeaderStart/66/calc/-;M2/Separator/60/calc/-;M3/HeaderIntermediate/48/-/manual;M4/Separator/60/calc/-;M5/Separator/60/calc/-;M6/HeaderIntermediate/60/calc/-;M7/Separator/60/calc/-;M8/HeaderEnd/66/calc/- | Preserved=[M3] Adapted=[] Removed=[] Incompatible=[] Restored=[]",
                    "3 M1/HeaderStart/66/calc/-;M2/Separator/56/-/manual;M3/HeaderIntermediate/48/-/manual;M4/Separator/60/calc/-;M5/Separator/60/calc/-;M6/HeaderIntermediate/60/calc/-;M7/Separator/60/calc/-;M8/HeaderEnd/66/calc/- | Preserved=[M2,M3] Adapted=[] Removed=[] Incompatible=[] Restored=[]",
                    "4 M1/HeaderStart/66/calc/-;M2/Separator/60/calc/-;M3/HeaderIntermediate/60/calc/-;M4/Separator/60/calc/-;M5/Separator/60/calc/-;M6/HeaderIntermediate/60/calc/-;M7/Separator/60/calc/-;M8/HeaderEnd/66/calc/- | Preserved=[] Adapted=[] Removed=[] Incompatible=[] Restored=[]",
                },
                log);
        }

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

        /// <summary>
        /// CT-PB0-05 (H-PB-28), solo los seis campos avanzados (W:395-400): A0 la carga, A1 tras «Restaurar estandar» y «Confirmar»,
        /// A2 tras Actualizar.
        ///
        /// <para><b>INCUMPLIMIENTO CARACTERIZADO.</b> El ensamblador declara que un «restaurar estandar» es un reset de
        /// TODO el rack, avanzados incluidos (PushBackEditorDesignAssembler.cs:296-299), y A1 lo cumple en la salida. Pero
        /// el portador <c>advanced</c> de la ventana no se reinicia ni se repinta: los seis controles siguen mostrando lo
        /// cargado y Actualizar (A2) vuelve a escribir los valores cargados, de modo que la restauracion se pierde. Se fija
        /// asi y NO se corrige.</para>
        /// </summary>
        [Fact(Skip = "SUSTITUIDA por PB-TC (D-PB-11), registro docs/automation/evidence/I-64-pilot/F4B-DRAFTS/superseding-characterization.md fila SUS-04: la ejecuta PushBackDeclaredCorrectionStandardRestoreTests.CT_PBTC_14_AStandardRestoreResetsTheAdvancedControlsAndReachesTheUpdate")]
        public void CT_PB0_05_TheSixAdvancedParametersAfterRestoreStandardAreResetInTheOutputAndComeBackOnUpdate()
        {
            var log = new List<string>();
            Sta(() =>
            {
                foreach (var fixture in new[] { "pb-06-seguridad", "pb-07-combinado" })
                {
                    var w = Abrir(Fixture(fixture));
                    try
                    {
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
                    "pb-06-seguridad A1 controles: RackHeaderHeightBox=[150] DerivedPostReinforcedCheck=False DerivedPostReinforcementHeightBox=[] DerivedPostHeightBox=[] RackSeparatorCountBox=[3] RackSeparatorSpacingBox=[18]",
                    "pb-06-seguridad A2 peticion: ManualHeaderHeightOverride=150,DerivedPostReinforced=false,DerivedPostReinforcementHeight=null,DerivedPostHeight=null,SeparatorCountOverride=3,SeparatorSpacingOverride=18",
                    "pb-07-combinado A0 salida: ManualHeaderHeightOverride=null,DerivedPostReinforced=true,DerivedPostReinforcementHeight=null,DerivedPostHeight=140,SeparatorCountOverride=null,SeparatorSpacingOverride=null",
                    "pb-07-combinado A0 controles: RackHeaderHeightBox=[] DerivedPostReinforcedCheck=True DerivedPostReinforcementHeightBox=[] DerivedPostHeightBox=[140] RackSeparatorCountBox=[] RackSeparatorSpacingBox=[]",
                    "pb-07-combinado A1 salida: ManualHeaderHeightOverride=null,DerivedPostReinforced=true,DerivedPostReinforcementHeight=null,DerivedPostHeight=null,SeparatorCountOverride=null,SeparatorSpacingOverride=null",
                    "pb-07-combinado A1 controles: RackHeaderHeightBox=[] DerivedPostReinforcedCheck=True DerivedPostReinforcementHeightBox=[] DerivedPostHeightBox=[140] RackSeparatorCountBox=[] RackSeparatorSpacingBox=[]",
                    "pb-07-combinado A2 peticion: ManualHeaderHeightOverride=null,DerivedPostReinforced=true,DerivedPostReinforcementHeight=null,DerivedPostHeight=140,SeparatorCountOverride=null,SeparatorSpacingOverride=null",
                },
                log);
        }
    }
}
