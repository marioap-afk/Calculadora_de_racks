using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Selective;
using RackCad.UI;
using RackCad.UI.Systems.PushBack;
using Xunit;
using static RackCad.UI.Tests.PushBackFullEditorCharacterizationSupport;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 F4B PB-T0 — CARACTERIZACION de los subdialogos de la ventana clasica de Push Back a traves de las costuras que la
    /// ventana YA declara (CT-PB0-12 a CT-PB0-16): el configurador de cabeceras, Seguridad aceptada, las botas por poste, la
    /// defensa por poste y los topes traseros. Se fija lo que la costura devuelve a la ventana y lo que la ventana escribiria
    /// despues, sin abrir ningun dialogo real.
    ///
    /// <para><b>Sin escrituras ni modales.</b> Ninguna prueba asigna <c>HeaderConfiguratorPresenter</c> ni
    /// <c>SafetyWindowDialog</c>, ni crea el configurador real ni <c>SelectiveSafetyWindow</c>;
    /// <c>ConfigureModuleHeaderButton</c> solo se pulsa con <c>HeaderConfiguratorDialog</c> asignado. Ninguna asercion va dentro
    /// de una costura: se captura dentro y se asevera fuera. Solo CT-PB0-14 muestra la ventana (fuera de pantalla y sin activarla),
    /// porque la rejilla de botas se crea con <c>Owner = this</c> antes de su costura. Toda ventana se cierra en un <c>finally</c>.</para>
    /// </summary>
    [Collection(EditorDiscardPromptCollection.Name)]
    public sealed class PushBackFullEditorCharacterizationSubdialogTests
    {
        private const string DefenseId = "DEFENSA_MONTACARGAS";
        private const string LateralProtectorId = "PROTECTOR_LATERAL_BOTA_H_3_16_18";


        /// <summary>El delta de dos salidas de la ventana, como lo fija Support.Describe.</summary>
        private static string DeltaText(string before, string after)
            => Describe(Delta(before, after));

        [Fact]
        public void CT_PB0_12_TheHeaderConfiguratorSeamStagesWhatItReturnsAndNullStagesNothing()
        {
            var log = new List<string>();
            Sta(() =>
            {
                var w = Abrir(Fixture("pb-31-v2-modulos"));
                try
                {
                    var before = Salida(w);
                    RackFrameConfiguration handed = null;
                    var calls = 0;
                    w.HeaderConfiguratorDialog = argument =>
                    {
                        calls++;
                        handed = argument;
                        var edited = new RackFrameProjectStore().DeepCopy(argument);
                        edited.PanelClear = 40.0;
                        return edited;
                    };
                    SelectModule(w, "M3");
                    Click(w, "ConfigureModuleHeaderButton");
                    log.Add("costura llamada=" + calls + " argumento-PanelClear=" + (handed == null ? "<null>" : handed.PanelClear.ToString("R")));
                    log.Add("escenificada=" + (w.PendingHeaderConfigurationForTest != null) + " PanelClear=" + (w.PendingHeaderConfigurationForTest == null ? "<null>" : w.PendingHeaderConfigurationForTest.PanelClear.ToString("R")));
                    log.Add("antes-de-confirmar salida-igual=" + (Salida(w) == before) + " pendiente=" + w.PendingWork().HasPendingWork);
                    Click(w, "ConfirmModuleButton");
                    var after = Salida(w);
                    var delta = Delta(before, after);
                    log.Add("confirmar delta " + Describe(delta));
                    foreach (var line in delta.Where(l => l.Contains("PanelClear") || l.Contains("UseCalculated"))) log.Add("   " + line);
                }
                finally { Cerrar(w); }

                var cancelled = Abrir(Fixture("pb-31-v2-modulos"));
                try
                {
                    var before = Salida(cancelled);
                    var calls = 0;
                    cancelled.HeaderConfiguratorDialog = argument => { calls++; return null; };
                    SelectModule(cancelled, "M3");
                    Click(cancelled, "ConfigureModuleHeaderButton");
                    log.Add("nulo costura llamada=" + calls + " escenificada=" + (cancelled.PendingHeaderConfigurationForTest != null)
                        + " pendiente=" + cancelled.PendingWork().HasPendingWork + " salida-igual=" + (Salida(cancelled) == before));
                }
                finally { Cerrar(cancelled); }
            });

            Assert.Equal(
                new[]
                {
                    "costura llamada=1 argumento-PanelClear=44",
                    "escenificada=True PanelClear=40",
                    "antes-de-confirmar salida-igual=True pendiente=True",
                    "confirmar delta n=149 sha256=baf0e5d5d2d0a394c82d2e81fe2a34b6a5c82f8eab3b874cc2495f15b300e88b contenedores=[$.PushBack.Structure.HeaderLineOverrides[0] x149]",
                    "   $.PushBack.Structure.HeaderLineOverrides[0].Header.PanelClear: null -> 40",
                    "nulo costura llamada=1 escenificada=False pendiente=False salida-igual=True",
                },
                log);
        }

        private static List<SelectiveSafetySelection> Copy(IReadOnlyList<SelectiveSafetySelection> list)
            => list.Select(selection => selection.DeepCopy()).ToList();

        private static string Selections(RackPushBackSystemWindow w)
            => string.Join(",", w.SafetySelections.Select(s => s.ElementId + "/" + s.Side + "/" + (s.AuthoredSide.HasValue ? s.AuthoredSide.Value.ToString() : "<null>")));

        [Fact]
        public void CT_PB0_13_AcceptedSafetyAppliesTheReturnedListAndTheDefenseFamilyIsRecreatedByItsSections()
        {
            var log = new List<string>();
            Sta(() =>
            {
                // (i) Quitar DEFENSA_MONTACARGAS de la lista: hallazgo de caracterizacion (las secciones la recrean).
                var w = Abrir(Fixture("pb-06-seguridad"));
                try
                {
                    var before = Salida(w);
                    var receivedIds = new List<string>();
                    var selectionsBefore = Selections(w);
                    w.SafetyDialog = received =>
                    {
                        receivedIds.AddRange(received.Select(s => s.ElementId));
                        return Copy(received).Where(s => !string.Equals(s.ElementId, DefenseId, StringComparison.Ordinal)).ToList();
                    };
                    Click(w, "SafetyButton");
                    var defense = w.SafetySelections.FirstOrDefault(s => string.Equals(s.ElementId, DefenseId, StringComparison.Ordinal));
                    log.Add("i recibidas=[" + string.Join(",", receivedIds) + "] antes=[" + selectionsBefore + "]");
                    log.Add("i despues=[" + Selections(w) + "] count=" + w.SafetySelections.Count
                        + " defensa=" + (defense == null ? "<ausente>" : "Side=" + defense.Side + " AuthoredSide=" + (defense.AuthoredSide.HasValue ? defense.AuthoredSide.Value.ToString() : "<null>")));
                    log.Add("i delta=" + DeltaText(before, Salida(w)));
                }
                finally { Cerrar(w); }

                // (ii) Anadir a la lista recibida una familia que SI viaja en ella.
                var added = Abrir(Fixture("pb-06-seguridad"));
                try
                {
                    var before = Salida(added);
                    added.SafetyDialog = received =>
                    {
                        var list = Copy(received);
                        list.Add(new SelectiveSafetySelection { ElementId = LateralProtectorId, Quantity = 1, Side = SafetySide.Both });
                        return list;
                    };
                    Click(added, "SafetyButton");
                    log.Add("ii despues=[" + Selections(added) + "]");
                    log.Add("ii delta=" + DeltaText(before, Salida(added)));
                }
                finally { Cerrar(added); }
            });

            Assert.Equal(
                new[]
                {
                    "i recibidas=[PROTECTOR_BOTA_H_3_16_18,DEFENSA_MONTACARGAS] antes=[PROTECTOR_BOTA_H_3_16_18/Left/Left,DEFENSA_MONTACARGAS/Left/Both]",
                    "i despues=[PROTECTOR_BOTA_H_3_16_18/Left/Left,DEFENSA_MONTACARGAS/None/None] count=2 defensa=Side=None AuthoredSide=None",
                    "i delta=n=5 sha256=90e91a115f304efb190642448bc1d70afd683e27bfd159d3b8cc7db7905f9fa5 lineas=[$.PushBack.DefensePieceId: null -> \"DEFENSA_MONTACARGAS\" ;; $.PushBack.Structure.SafetySelections[0].BotaPieceId: null -> \"PROTECTOR_BOTA_H_3_16_18\" ;; $.PushBack.Structure.SafetySelections[0].BotaSidesDeclared: null -> true ;; $.PushBack.Structure.SafetySelections[1].AuthoredSide: 3 -> 0 ;; $.PushBack.Structure.SafetySelections[1].Side: 1 -> 0]",
                    "ii despues=[PROTECTOR_BOTA_H_3_16_18/Left/Left,DEFENSA_MONTACARGAS/Left/Both,PROTECTOR_LATERAL_BOTA_H_3_16_18/Left/Both]",
                    "ii delta=n=31 sha256=da17cbaf04490dad940a587ccebb9b4c1fe26ef4c82f2e48d18322636230b20b contenedores=[$.PushBack.DefensePieceId x1; $.PushBack.Structure.SafetySelections[0] x2; $.PushBack.Structure.SafetySelections[2] x28]",
                },
                log);
        }

        [Fact]
        public void CT_PB0_14_ThePerPostBootGridResultReachesTheOutputThroughTheBootSection()
        {
            var log = new List<string>();
            Sta(() =>
            {
                var w = AbrirFueraDePantalla(Fixture("pb-06-seguridad"));
                try
                {
                    var before = Salida(w);
                    var calls = 0;
                    w.BootPerPostWindowDialog = dialog =>
                    {
                        calls++;
                        dialog.SetForTest(0, BootPlacement.Rear);
                        dialog.BuildResultForTest();
                        return true;
                    };
                    w.SafetyDialog = received =>
                    {
                        w.BootSectionForTest.Configure();
                        return received;
                    };
                    Click(w, "SafetyButton");
                    log.Add("costura llamada=" + calls);
                    log.Add("delta=" + DeltaText(before, Salida(w)));
                }
                finally { Cerrar(w); }
            });

            Assert.Equal(
                new[]
                {
                    "costura llamada=1",
                    "delta=n=6 sha256=5418b2b5810265fcfa9210a3c80e6e7a070113528b652689cb022bb0782f5240 lineas=[$.PushBack.DefensePieceId: null -> \"DEFENSA_MONTACARGAS\" ;; $.PushBack.Structure.SafetySelections[0].BotaPieceId: null -> \"PROTECTOR_BOTA_H_3_16_18\" ;; $.PushBack.Structure.SafetySelections[0].BotaPosts[0].PostIndex: 1 -> 0 ;; $.PushBack.Structure.SafetySelections[0].BotaPosts[1].Placement: <absent> -> 2 ;; $.PushBack.Structure.SafetySelections[0].BotaPosts[1].PostIndex: <absent> -> 1 ;; $.PushBack.Structure.SafetySelections[0].BotaSidesDeclared: null -> true]",
                },
                log);
        }

        [Fact]
        public void CT_PB0_15_ThePerPostDefenseRecordsReachTheOutputThroughTheDefenseSection()
        {
            var log = new List<string>();
            Sta(() =>
            {
                var w = Abrir(Fixture("pb-06-seguridad"));
                try
                {
                    var before = Salida(w);
                    var calls = 0;
                    w.DefenseDialog = section =>
                    {
                        calls++;
                        return new List<SafetyPostDefense> { new SafetyPostDefense { PostIndex = 0, ExitLength = 24.0, EntranceLength = 12.0 } };
                    };
                    w.SafetyDialog = received =>
                    {
                        w.DefenseSectionForTest.Configure();
                        return received;
                    };
                    Click(w, "SafetyButton");
                    log.Add("costura llamada=" + calls);
                    log.Add("delta=" + DeltaText(before, Salida(w)));
                }
                finally { Cerrar(w); }
            });

            Assert.Equal(
                new[]
                {
                    "costura llamada=1",
                    "delta=n=8 sha256=aeea76d4d1eabc004430edd36bca927322109560979b25a930e8ad3451bf214c lineas=[$.PushBack.DefensePieceId: null -> \"DEFENSA_MONTACARGAS\" ;; $.PushBack.Structure.SafetySelections[0].BotaPieceId: null -> \"PROTECTOR_BOTA_H_3_16_18\" ;; $.PushBack.Structure.SafetySelections[0].BotaSidesDeclared: null -> true ;; $.PushBack.Structure.SafetySelections[1].DefensaPosts[0].EntranceAuto: <absent> -> null ;; $.PushBack.Structure.SafetySelections[1].DefensaPosts[0].EntranceLength: <absent> -> 12 ;; $.PushBack.Structure.SafetySelections[1].DefensaPosts[0].ExitAuto: <absent> -> null ;; $.PushBack.Structure.SafetySelections[1].DefensaPosts[0].ExitLength: <absent> -> 24 ;; $.PushBack.Structure.SafetySelections[1].DefensaPosts[0].PostIndex: <absent> -> 0]",
                },
                log);
        }

        [Fact]
        public void CT_PB0_16_TheRearTopeDialogResultReachesTheOutputInASingleSidedRack()
        {
            var log = new List<string>();
            Sta(() =>
            {
                var w = Abrir(Fixture("pb-05-topes"));
                try
                {
                    var before = Salida(w);
                    var calls = 0;
                    w.RearTopeDialog = (config, levels) =>
                    {
                        calls++;
                        return new SafetyTopeGridWindow.TopeResult
                        {
                            Saque = 4.0,
                            OffCells = { new SelectiveGridCell { Frente = 0, Level = 0 } },
                        };
                    };
                    w.SafetyDialog = received =>
                    {
                        w.RearTopeSectionForTest.Configure();
                        return received;
                    };
                    Click(w, "SafetyButton");
                    log.Add("costura llamada=" + calls);
                    log.Add("delta=" + DeltaText(before, Salida(w)));
                }
                finally { Cerrar(w); }
            });

            Assert.Equal(
                new[]
                {
                    "costura llamada=1",
                    "delta=n=35 sha256=647fe0acdbb0c6c3ed65c135804b909c2555e98ab4fa82a071fab1292594632d contenedores=[$.PushBack.DefensePieceId x1; $.PushBack.RearTopeOffCells[0] x1; $.PushBack.RearTopeOffCells[1] x2; $.PushBack.RearTopeOffCells[2] x2; $.PushBack.RearTopeSaque x1; $.PushBack.Structure.SafetySelections[0] x28]",
                },
                log);
        }
    }
}
