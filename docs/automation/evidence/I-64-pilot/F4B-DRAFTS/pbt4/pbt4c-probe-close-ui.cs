using System;
using System.Collections.Generic;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack;
using RackCad.UI.Shell;
using RackCad.UI.Systems.PushBack;
using Xunit;
using static RackCad.UI.Tests.PushBackDeclaredCorrectionSupport;
using static RackCad.UI.Tests.PushBackFullEditorCharacterizationSupport;
using static RackCad.UI.Tests.PushBackWorkspaceSupport;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// PB-T4c (paquete v5.0), precondicion 6, paso 7: la sonda de CT-PBT4C-09 sobre el GREEN literal. ¿Es alcanzable en WORKSPACE «solo un modulo
    /// escenificado», sin cambios sin aplicar? Lee PendingWork() de ventanas reales sin mostrar, con los ayudantes de PB-T2b sin editarlos, y la
    /// costura de D-S7 (HeaderConfiguratorDialog), sin construir el configurador. Clase temporal: no entra en ningun commit y acaba en Assert.Fail.
    /// </summary>
    [Collection(EditorDiscardPromptCollection.Name)]
    public sealed class TmpPbt4cCloseProbe
    {
        private static string Q(EditorPendingWork work) => work.HasPendingWork ? "[" + work.Question + "]" : "ninguna";

        private static void Header(RackPushBackSystemWindow w)
        {
            SelectModule(w, "M3");
            w.HeaderConfiguratorDialog = argumento =>
            {
                var editada = new RackFrameProjectStore().DeepCopy(argumento);
                editada.PanelClear = 40.0;
                return editada;
            };
            Click(w, "ConfigureModuleHeaderButton");
        }

        private static void Workspace(List<string> log, string label, string fixture, Action<RackPushBackSystemWindow> gestos)
        {
            var (_, draft) = Borrador(Estado(Fixture(fixture)));
            var w = VentanaBorrador(draft, null, new PuertoContado(draft));
            try
            {
                var antes = Q(w.PendingWork());
                gestos(w);
                log.Add(label + " antes=" + antes + " modulo-escenificado=" + (w.EditorStateForTest.ModuleSession?.HasPendingChanges == true)
                        + " pregunta=" + Q(w.PendingWork()));
            }
            catch (Exception ex)
            {
                log.Add(label + " EXCEPCION " + ex.GetType().Name + ": " + ex.Message);
            }
            finally
            {
                CerrarVentana(w);
            }
        }

        [Fact]
        public void Probe()
        {
            var log = new List<string>();
            var avisos = new PromptProbe(true);
            Sta(() => { });
            using (EditorDiscardPrompt.Substitute(avisos.Ask))
            {
                Sta(() =>
                {
                    Workspace(log, "ninguna", DisenoBase, w => { });
                    Workspace(log, "solo-conmutador", "pb-31-v2-modulos", w => Check(w, "AdvancedModulesToggle").IsChecked = true);
                    Workspace(log, "solo-seleccion", "pb-31-v2-modulos", w => SelectModule(w, "M3"));
                    Workspace(log, "cabecera", "pb-31-v2-modulos", Header);
                    Workspace(log, "stage-module-edit", DisenoBase, StageModuleEdit);
                    Workspace(log, "solo-sin-aplicar", DisenoBase, w => SetNumberR(w, "ToleranceBox", 4.5));
                    Workspace(log, "cabecera-y-sin-aplicar", "pb-31-v2-modulos", w => { Header(w); SetNumberR(w, "ToleranceBox", 4.5); });

                    var clasica = Ventana();
                    try
                    {
                        Cargar(clasica, Fixture("pb-31-v2-modulos"));
                        Header(clasica);
                        log.Add("clasico-cabecera modulo-escenificado=" + (clasica.EditorStateForTest.ModuleSession?.HasPendingChanges == true)
                                + " pregunta=" + Q(clasica.PendingWork()));
                    }
                    catch (Exception ex)
                    {
                        log.Add("clasico-cabecera EXCEPCION " + ex.GetType().Name + ": " + ex.Message);
                    }
                    finally
                    {
                        avisos.Answer = true;
                        CerrarVentana(clasica);
                    }
                });
            }

            Assert.Fail(string.Join("\n", log));
        }
    }
}
