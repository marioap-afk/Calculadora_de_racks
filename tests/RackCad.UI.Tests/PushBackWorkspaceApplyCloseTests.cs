using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Workspace.Drafts;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.UI;
using RackCad.UI.Shell;
using RackCad.UI.Systems.PushBack;
using Xunit;
using static RackCad.UI.Tests.PushBackDeclaredCorrectionSupport;
using static RackCad.UI.Tests.PushBackFullEditorCharacterizationSupport;
using static RackCad.UI.Tests.PushBackWorkspaceSupport;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 F4B PB-T2b - CT-PBT2B-03 a 06: en WORKSPACE ningun gesto anota una peticion clasica y el BOM y «Guardar en biblioteca»
    /// dejan el borrador y su identidad como estaban (CT-03, la UI-STA de OB-01.3 con las cuatro comprobaciones de F4B-E23);
    /// «Aplicar al borrador» entrega UNA copia del estado editado al borrador y no cierra la ventana (CT-04); la misma validacion
    /// sirve a los dos modos y ninguno muta las dos referencias compartidas de D2 (CT-05); y «Cerrar» solo pregunta por lo que no
    /// se aplico (CT-06).
    ///
    /// <para>Cada metodo arma una lista de lineas y acaba en UN <c>Assert.Equal</c>. Toda ventana se crea sin mostrar, con su
    /// carga DENTRO del <c>try</c>, y se cierra en el <c>finally</c>; no hay teclado ni foco real, asi que un clic es un
    /// <c>RaiseEvent</c> y una edicion numerica es <c>SetNumberR</c>, que eleva <c>LostFocus</c> y recalcula. Los diecisiete
    /// fixtures de <c>Fixtures/I64</c> tienen tolerancia 4: las ediciones de tolerancia escriben 4.5, y 5 la segunda de CT-04.
    /// El BOM y «Guardar en biblioteca» solo pasan por las costuras <c>BomDialog</c> y <c>LibraryPathDialog</c>, cuyo codigo no
    /// aserta nada; la unica escritura en disco es <see cref="PushBackWorkspaceSupport.ArchivoTemporal"/>, que se borra antes de
    /// cada guardado y en el <c>finally</c>. El aviso de descarte siempre esta sustituido.</para>
    ///
    /// <para><b>El momento de cada lectura.</b> Cada linea se arma justo tras su ultimo gesto y antes del siguiente; lo que se
    /// compara con el estado de antes de un gesto se toma justo antes de el. «Aplicar» que aplica deja en el borrador una copia
    /// NUEVA, y cada recalculo valido sustituye la computacion de la ventana. La sonda del modo va antes de cualquier gesto de
    /// cada ventana de WORKSPACE.</para>
    /// </summary>
    [Collection(EditorDiscardPromptCollection.Name)]
    public sealed class PushBackWorkspaceApplyCloseTests
    {
        private const string Sin = "peticion=False solicitud=<null>";

        /// <summary>Una ventana de WORKSPACE con su borrador, su puerto contado y el estado del borrador de antes de abrirla.</summary>
        private sealed class Sesion
        {
            public RackPushBackSystemWindow W;
            public DraftSession<PushBackEditableState> Draft;
            public PuertoContado Puerto;
            public PushBackEditableState Antes;
        }

        private static Sesion Nueva(List<string> log, PushBackEditableState estado, RackProject proyecto = null)
        {
            var (_, draft) = Borrador(estado);
            var puerto = new PuertoContado(draft);
            var antes = Copy(draft.CurrentDraftState);
            var w = Paso(log, () => VentanaBorrador(draft, proyecto, puerto));
            return w == null ? null : new Sesion { W = w, Draft = draft, Puerto = puerto, Antes = antes };
        }

        /// <summary>Los campos comunes de CT-03: la peticion clasica, la solicitud, el borrador contra el de antes y las llamadas.</summary>
        private static string Cola(Sesion s)
            => "peticion=" + s.W.InsertRequested + " solicitud=" + Solicitud(s.W) + " borrador-igual=" + Igual(s.Draft.CurrentDraftState, s.Antes)
               + " llamadas=" + s.Puerto.Llamadas;

        /// <summary>Cierra la ventana y termina la linea con el contador de avisos de esa ventana, leido tras cerrarla.</summary>
        private static string Fin(Sesion s, string linea, PromptProbe avisos)
        {
            CerrarVentana(s.W);
            return linea + " descartes=" + avisos.Asked;
        }

        // ------------------------------------------------------------------------------------------------ CT-PBT2B-03

        /// <summary>Pasada 1: pulsar TODOS los botones de la barra, tambien los ocultos, y editar; el estado y el id no se mueven.</summary>
        private static bool Pasada1(List<string> log, PromptProbe avisos)
        {
            var sobre = Fixture(DisenoBase);
            var s = Nueva(log, Estado(sobre), ProyectoConDesconocido(sobre));
            if (s == null) return false;
            var w = s.W;
            try
            {
                avisos.Reiniciar();
                log.Add("modo=" + w.ModeForTest);
                if (w.ModeForTest != PushBackEditorMode.Workspace) return false;

                var bomLlamadas = 0;
                Window bomDueno = null;
                w.BomDialog = (bom, dueno) =>
                {
                    bomLlamadas++;
                    bomDueno = dueno;
                };
                var libreriaLlamadas = 0;
                w.LibraryPathDialog = (dueno, nombre) =>
                {
                    libreriaLlamadas++;
                    return ArchivoTemporal;
                };

                log.Add("carga id=" + w.RackId + " " + Cola(s));
                foreach (var boton in BotonesDeInsercion)
                {
                    Click(w, boton);
                    log.Add(boton + " visible=" + Btn(w, boton).Visibility + " estado=" + StatusText(w) + " " + Cola(s));
                }

                for (var vez = 1; vez <= 2; vez++)
                {
                    if (vez == 2)
                    {
                        Click(w, "RestoreButton");
                        log.Add("RestoreButton " + Cola(s));
                        SetNumberR(w, "ToleranceBox", 4.5);
                        log.Add("ToleranceBox=4.5 " + Cola(s));
                    }

                    Click(w, "BomButton");
                    log.Add("BomButton dialogos=" + bomLlamadas + " dueno-es-la-ventana=" + ReferenceEquals(bomDueno, w) + " " + Cola(s));
                    BorrarArchivoTemporal();
                    Click(w, "SaveLibraryButton");
                    log.Add("SaveLibraryButton dialogos=" + libreriaLlamadas + " archivo=" + File.Exists(ArchivoTemporal)
                        + " campo-desconocido=" + (File.Exists(ArchivoTemporal) && File.ReadAllText(ArchivoTemporal).Contains("CampoFuturoPbt2"))
                        + " id=" + w.RackId + " " + Cola(s));
                }

                Click(w, "UpdateButton");
                log.Add("UpdateButton estado=" + StatusText(w) + " id=" + w.RackId + " abierta=" + !EstaCerradaVentana(w)
                    + " sucio=" + s.Draft.IsDirty + " " + Cola(s));
                Click(w, "CloseButton");
                log.Add("CloseButton cerrada=" + EstaCerradaVentana(w) + " " + Cola(s) + " descartes=" + avisos.Asked);
                return true;
            }
            finally { CerrarVentana(w); }
        }

        /// <summary>Una ventana de la pasada 2 o 3, sobre el estado de su fixture: la sonda y sus gestos.</summary>
        private static bool VentanaPasada(List<string> log, PromptProbe avisos, string fixture, Action<Sesion> gestos)
        {
            var s = Nueva(log, Estado(Fixture(fixture)));
            if (s == null) return false;
            try
            {
                avisos.Reiniciar();
                if (!Sondeo(log, s.W)) return false;
                gestos(s);
                return true;
            }
            finally { CerrarVentana(s.W); }
        }

        private static List<SelectiveSafetySelection> Copia(IReadOnlyList<SelectiveSafetySelection> lista)
            => lista.Select(seleccion => seleccion.DeepCopy()).ToList();

        /// <summary>Pasada 2: los subdialogos con costura. Los gestos son los de los originales (CT-PB0-12, 13, 15 y 16), copiados.</summary>
        private static bool Pasada2(List<string> log, PromptProbe avisos)
        {
            // El configurador de cabeceras (CT-PB0-12 :38): una costura nula no escenifica nada; la costura que devuelve una copia
            // con PanelClear = 40 escenifica y «Confirmar» la aplica.
            if (!VentanaPasada(log, avisos, "pb-31-v2-modulos", s =>
            {
                var w = s.W;
                SelectModule(w, "M3");
                w.HeaderConfiguratorDialog = argumento => null;
                Click(w, "ConfigureModuleHeaderButton");
                log.Add("configurador-nulo " + Cola(s));
                w.HeaderConfiguratorDialog = argumento =>
                {
                    var editada = new RackFrameProjectStore().DeepCopy(argumento);
                    editada.PanelClear = 40.0;
                    return editada;
                };
                Click(w, "ConfigureModuleHeaderButton");
                Click(w, "ConfirmModuleButton");
                log.Add(Fin(s, "configurador-confirmado " + Cola(s), avisos));
            }))
            {
                return false;
            }

            // Seguridad aceptada (CT-PB0-13 (i) :104): la lista que devuelve la costura, sin DEFENSA_MONTACARGAS.
            if (!VentanaPasada(log, avisos, "pb-06-seguridad", s =>
            {
                var w = s.W;
                w.SafetyDialog = recibidas => Copia(recibidas)
                    .Where(seleccion => !string.Equals(seleccion.ElementId, "DEFENSA_MONTACARGAS", StringComparison.Ordinal))
                    .ToList();
                Click(w, "SafetyButton");
                log.Add(Fin(s, "seguridad-aceptada " + Cola(s), avisos));
            }))
            {
                return false;
            }

            // La defensa por poste (CT-PB0-15 :200).
            if (!VentanaPasada(log, avisos, "pb-06-seguridad", s =>
            {
                var w = s.W;
                w.DefenseDialog = seccion => new List<SafetyPostDefense>
                {
                    new SafetyPostDefense { PostIndex = 0, ExitLength = 24.0, EntranceLength = 12.0 },
                };
                w.SafetyDialog = recibidas =>
                {
                    w.DefenseSectionForTest.Configure();
                    return recibidas;
                };
                Click(w, "SafetyButton");
                log.Add(Fin(s, "defensa-por-poste " + Cola(s), avisos));
            }))
            {
                return false;
            }

            // Los topes traseros (CT-PB0-16 :237).
            return VentanaPasada(log, avisos, "pb-05-topes", s =>
            {
                var w = s.W;
                w.RearTopeDialog = (config, niveles) => new SafetyTopeGridWindow.TopeResult
                {
                    Saque = 4.0,
                    OffCells = { new SelectiveGridCell { Frente = 0, Level = 0 } },
                };
                w.SafetyDialog = recibidas =>
                {
                    w.RearTopeSectionForTest.Configure();
                    return recibidas;
                };
                Click(w, "SafetyButton");
                log.Add(Fin(s, "topes-traseros " + Cola(s), avisos));
            });
        }

        /// <summary>Pasada 3: el frontal, el selector de lado y la confirmacion y la cancelacion de modulos, sobre <c>pb-20-ab</c>.</summary>
        private static bool Pasada3(List<string> log, PromptProbe avisos)
            => VentanaPasada(log, avisos, DisenoCompuesto, s =>
            {
                var w = s.W;
                Combo(w, "FrontalSideBox").SelectedIndex = 1;
                log.Add("FrontalSideBox=1 " + Cola(s));
                Combo(w, "SideSelectorBox").SelectedIndex = 1;
                log.Add("SideSelectorBox=1 " + Cola(s));
                Combo(w, "SideSelectorBox").SelectedIndex = 2;
                log.Add("SideSelectorBox=2 " + Cola(s));
                StageModuleEdit(w);
                Click(w, "ConfirmModuleButton");
                log.Add("modulo-confirmado " + Cola(s));
                StageModuleEdit(w);
                Click(w, "CancelModuleButton");
                log.Add(Fin(s, "modulo-cancelado " + Cola(s), avisos));
            });

        [Fact]
        public void CT_PBT2B_03_NoWorkspaceGestureFlagsAClassicRequestAndBomAndLibraryLeaveTheDraftAndItsIdentityAlone()
        {
            var log = new List<string>();
            var avisos = new PromptProbe(true);
            using (EditorDiscardPrompt.Substitute(avisos.Ask))
            {
                Sta(() =>
                {
                    CrearCarpetaTemporal();
                    try
                    {
                        if (!Pasada1(log, avisos)) return;
                        if (!Pasada2(log, avisos)) return;
                        Pasada3(log, avisos);
                    }
                    finally { LimpiarArchivoTemporal(); }
                });
            }

            const string c = Sin + " borrador-igual=True llamadas=0";
            var esperadas = new List<string>
            {
                // Pasada 1.
                "modo=Workspace",
                "carga id=GUID-PBT2-0001 " + c,
                "InsertButton visible=Collapsed estado=" + RechazoClasico + " " + c,
                "InsertLateralButton visible=Collapsed estado=" + RechazoClasico + " " + c,
                "InsertFrontalEntradaButton visible=Collapsed estado=" + RechazoClasico + " " + c,
                "InsertFrontalPosteriorButton visible=Collapsed estado=" + RechazoClasico + " " + c,
                "InsertPlantaButton visible=Collapsed estado=" + RechazoClasico + " " + c,
                "InsertBatchButton visible=Collapsed estado=" + RechazoClasico + " " + c,
                "BomButton dialogos=1 dueno-es-la-ventana=True " + c,
                "SaveLibraryButton dialogos=1 archivo=True campo-desconocido=True id=GUID-PBT2-0001 " + c,
                "RestoreButton " + c,
                "ToleranceBox=4.5 " + c,
                "BomButton dialogos=2 dueno-es-la-ventana=True " + c,
                "SaveLibraryButton dialogos=2 archivo=True campo-desconocido=True id=GUID-PBT2-0001 " + c,
                "UpdateButton estado=" + Aplicado + " id=GUID-PBT2-0001 abierta=True sucio=True " + Sin + " borrador-igual=False llamadas=1",
                "CloseButton cerrada=True " + Sin + " borrador-igual=False llamadas=1 descartes=0",

                // Pasada 2.
                "configurador-nulo " + c,
                "configurador-confirmado " + c + " descartes=1",
                "seguridad-aceptada " + c + " descartes=1",
                "defensa-por-poste " + c + " descartes=1",
                "topes-traseros " + c + " descartes=1",

                // Pasada 3.
                "FrontalSideBox=1 " + c,
                "SideSelectorBox=1 " + c,
                "SideSelectorBox=2 " + c,
                "modulo-confirmado " + c,
                "modulo-cancelado " + c + " descartes=1",
            };

            Assert.Equal(esperadas, log);
        }

        // ------------------------------------------------------------------------------------------------ CT-PBT2B-04

        /// <summary>La ventana de <c>pb-20-ab</c>: carga, edicion, «Aplicar» (una vez, y sin cambios), una segunda aplicacion y un borrador en error.</summary>
        private static bool Aplicar(List<string> log, PromptProbe avisos)
        {
            var s = Nueva(log, Estado(Fixture(DisenoCompuesto)));
            if (s == null) return false;
            var w = s.W;
            try
            {
                avisos.Reiniciar();
                if (!Sondeo(log, w)) return false;
                log.Add("carga llamadas=" + s.Puerto.Llamadas + " sucio=" + s.Draft.IsDirty);

                // La tolerancia de `pb-20-ab` ya es 4: la edicion escribe 4.5, o «Aplicar» no tendria nada que aplicar.
                SetNumberR(w, "ToleranceBox", 4.5);
                log.Add("editar aplicar-habilitado=" + Btn(w, "UpdateButton").IsEnabled + " tooltip=" + Btn(w, "UpdateButton").ToolTip);

                Click(w, "UpdateButton");
                log.Add("aplicar estado=" + StatusText(w) + " llamadas=" + s.Puerto.Llamadas + " sucio=" + s.Draft.IsDirty
                    + " igual-a-la-ventana=" + Igual(s.Draft.CurrentDraftState, w.EditableStateForTest)
                    + " mismo-objeto=" + ReferenceEquals(s.Draft.CurrentDraftState, w.EditableStateForTest)
                    + " compartidos-fuera-de-D2=" + CompartidosFueraDeD2(s.Draft.CurrentDraftState, w.EditableStateForTest)
                    + " diseno-igual=" + (DisenoDe(s.Draft.CurrentDraftState, PushBackSide.A) == DisenoDeVentana(w))
                    + " abierta=" + !EstaCerradaVentana(w) + " peticion=" + w.InsertRequested);

                Click(w, "UpdateButton");
                log.Add("otra-vez estado=" + StatusText(w) + " llamadas=" + s.Puerto.Llamadas);

                SetNumberR(w, "PalletsDeepBox", 3);
                Click(w, "UpdateButton");
                log.Add("segunda llamadas=" + s.Puerto.Llamadas + " igual-a-la-ventana=" + Igual(s.Draft.CurrentDraftState, w.EditableStateForTest)
                    + " diseno-igual=" + (DisenoDe(s.Draft.CurrentDraftState, PushBackSide.A) == DisenoDeVentana(w)));

                // Un borrador en error no admite el estado y no cambia; la tolerancia 5 queda sin aplicar al cerrar.
                s.Draft.EnterError("prueba");
                SetNumberR(w, "ToleranceBox", 5);
                var actual = s.Draft.CurrentDraftState;
                var copiaDeActual = Copy(actual);
                Click(w, "UpdateButton");
                var linea = "error estado=" + StatusText(w) + " llamadas=" + s.Puerto.Llamadas
                    + " sin-cambio=" + (ReferenceEquals(s.Draft.CurrentDraftState, actual) && Igual(s.Draft.CurrentDraftState, copiaDeActual))
                    + " abierta=" + !EstaCerradaVentana(w);
                log.Add(Fin(s, linea, avisos));
                return true;
            }
            finally { CerrarVentana(w); }
        }

        /// <summary>«Aplicar» sin editar no toca el borrador, tampoco con un nombre nulo o con espacios (D-PBT2-06).</summary>
        private static bool AplicarSinEditar(List<string> log, PromptProbe avisos, string etiqueta, string nombre)
        {
            var estado = Estado(Fixture(DisenoBase));
            estado.Name = nombre;
            var s = Nueva(log, estado);
            if (s == null) return false;
            try
            {
                avisos.Reiniciar();
                if (!Sondeo(log, s.W)) return false;
                Click(s.W, "UpdateButton");
                log.Add(Fin(
                    s,
                    etiqueta + " aplicar-sin-editar llamadas=" + s.Puerto.Llamadas + " sucio=" + s.Draft.IsDirty + " estado=" + StatusText(s.W),
                    avisos));
                return true;
            }
            finally { CerrarVentana(s.W); }
        }

        /// <summary>
        /// «Guardar en biblioteca» normaliza el nombre de la ventana (L-PBT2-12): «Cerrar» pregunta, y «Aplicar» lo lleva al
        /// borrador. La costura devuelve el archivo temporal: con un nulo no se llega a <c>SetName</c>.
        /// </summary>
        private static bool NombreTrasLaBiblioteca(List<string> log, PromptProbe avisos, string etiqueta, string nombre)
        {
            var estado = Estado(Fixture(DisenoBase));
            estado.Name = nombre;
            var s = Nueva(log, estado);
            if (s == null) return false;
            var w = s.W;
            try
            {
                avisos.Reiniciar();
                if (!Sondeo(log, w)) return false;
                w.LibraryPathDialog = (dueno, propuesto) => ArchivoTemporal;

                avisos.Answer = false;
                BorrarArchivoTemporal();
                Click(w, "SaveLibraryButton");
                log.Add(etiqueta + "-biblioteca archivo=" + File.Exists(ArchivoTemporal) + " nombre-ventana=" + Nombre(w.RackName)
                    + " llamadas=" + s.Puerto.Llamadas + " sucio=" + s.Draft.IsDirty);

                Click(w, "CloseButton");
                log.Add("cerrar avisos=" + avisos.Asked + " pregunta=" + avisos.Question + " cerrada=" + EstaCerradaVentana(w));

                avisos.Answer = true;
                Click(w, "UpdateButton");
                log.Add("aplicar estado=" + StatusText(w) + " llamadas=" + s.Puerto.Llamadas + " sucio=" + s.Draft.IsDirty
                    + " nombre-borrador=" + Nombre(s.Draft.CurrentDraftState.Name));

                CerrarVentana(w);
                log.Add("cerrada=" + EstaCerradaVentana(w) + " descartes=" + avisos.Asked);
                return true;
            }
            finally
            {
                avisos.Answer = true;
                CerrarVentana(w);
            }
        }

        [Fact]
        public void CT_PBT2B_04_ApplyToDraftHandsTheEditedStateToTheDraftOnceAndKeepsTheWindowOpen()
        {
            var log = new List<string>();
            var avisos = new PromptProbe(true);
            using (EditorDiscardPrompt.Substitute(avisos.Ask))
            {
                Sta(() =>
                {
                    CrearCarpetaTemporal();
                    try
                    {
                        if (!Aplicar(log, avisos)) return;
                        if (!AplicarSinEditar(log, avisos, "nombre-nulo", null)) return;
                        if (!AplicarSinEditar(log, avisos, "nombre-con-espacios", "  PB  Uno  ")) return;
                        if (!NombreTrasLaBiblioteca(log, avisos, "nombre-nulo", null)) return;
                        NombreTrasLaBiblioteca(log, avisos, "nombre-con-espacios", "  PB  Uno  ");
                    }
                    finally { LimpiarArchivoTemporal(); }
                });
            }

            var esperadas = new List<string>
            {
                "carga llamadas=0 sucio=False",
                "editar aplicar-habilitado=True tooltip=" + AplicarTooltip,
                "aplicar estado=" + Aplicado + " llamadas=1 sucio=True igual-a-la-ventana=True mismo-objeto=False"
                    + " compartidos-fuera-de-D2=0 diseno-igual=True abierta=True peticion=False",
                "otra-vez estado=" + SinCambios + " llamadas=1",
                "segunda llamadas=2 igual-a-la-ventana=True diseno-igual=True",
                "error estado=" + NoEditable + " llamadas=3 sin-cambio=True abierta=True descartes=1",
                "nombre-nulo aplicar-sin-editar llamadas=0 sucio=False estado=" + SinCambios + " descartes=0",
                "nombre-con-espacios aplicar-sin-editar llamadas=0 sucio=False estado=" + SinCambios + " descartes=0",
                "nombre-nulo-biblioteca archivo=True nombre-ventana=[] llamadas=0 sucio=False",
                "cerrar avisos=1 pregunta=" + PreguntaSinAplicar + " cerrada=False",
                "aplicar estado=" + Aplicado + " llamadas=1 sucio=True nombre-borrador=[]",
                "cerrada=True descartes=1",
                "nombre-con-espacios-biblioteca archivo=True nombre-ventana=[PB  Uno] llamadas=0 sucio=False",
                "cerrar avisos=1 pregunta=" + PreguntaSinAplicar + " cerrada=False",
                "aplicar estado=" + Aplicado + " llamadas=1 sucio=True nombre-borrador=[PB  Uno]",
                "cerrada=True descartes=1",
            };

            Assert.Equal(esperadas, log);
        }

        // ------------------------------------------------------------------------------------------------ CT-PBT2B-05

        /// <summary>Las dos referencias compartidas de D2, medidas desde el momento en que cada una se adopta, a lo largo de todas las ventanas.</summary>
        private sealed class Vigia
        {
            private readonly List<(object Object, string Before)> items = new List<(object, string)>();

            public int Cuenta => items.Count;

            public void Capturar(RackPushBackSystemWindow w, DraftSession<PushBackEditableState> draft)
            {
                foreach (var estado in new[] { w.EditableStateForTest, draft.CurrentDraftState, draft.InitialDraftState })
                {
                    if (estado == null) continue;
                    foreach (var referencia in SharedRoots(estado))
                    {
                        if (items.Any(item => ReferenceEquals(item.Object, referencia))) continue;
                        items.Add((referencia, Huella(referencia)));
                    }
                }
            }

            public bool Intacta() => items.All(item => Huella(item.Object) == item.Before);
        }

        /// <summary>Un gesto de validacion: su nombre, el diseño sobre el que corre y lo que hace en la ventana.</summary>
        private sealed class GestoDeValidacion
        {
            public string Nombre;
            public string Diseno;
            public Action<RackPushBackSystemWindow> Hacer;
        }

        private static IEnumerable<GestoDeValidacion> Gestos() => new[]
        {
            new GestoDeValidacion
            {
                Nombre = "campo-en-error",
                Diseno = DisenoBase,
                Hacer = w => Field(w, "ToleranceBox").Text = "abc",
            },
            new GestoDeValidacion
            {
                Nombre = "calculo-invalido",
                Diseno = DisenoBase,
                Hacer = w =>
                {
                    Check(w, "AdvancedModulesToggle").IsChecked = true;
                    Check(w, "DerivedPostReinforcedCheck").IsChecked = true;
                    SetNumberR(w, "DerivedPostReinforcementHeightBox", 100000);
                },
            },
            new GestoDeValidacion
            {
                Nombre = "bloqueante",
                Diseno = DisenoCompuesto,
                Hacer = w =>
                {
                    w.CompositeState.SetGap(-1);
                    Recalcular(w);
                },
            },
        };

        private static void ClasicoDeValidacion(List<string> log, PromptProbe avisos, GestoDeValidacion gesto)
        {
            var w = Ventana();
            try
            {
                avisos.Reiniciar();
                Cargar(w, Fixture(gesto.Diseno));
                gesto.Hacer(w);
                Click(w, "UpdateButton");
                var linea = gesto.Nombre + " clasico valido=" + Valido(w) + " bloqueado=" + w.OutputIsBlockedForTest + " estado=" + StatusText(w)
                    + " peticion=" + w.InsertRequested;
                CerrarVentana(w);
                log.Add(linea + " descartes=" + avisos.Asked);
            }
            finally { CerrarVentana(w); }
        }

        private static bool BorradorDeValidacion(List<string> log, PromptProbe avisos, Vigia vigia, GestoDeValidacion gesto)
        {
            var s = Nueva(log, Estado(Fixture(gesto.Diseno)));
            if (s == null) return false;
            var w = s.W;
            try
            {
                avisos.Reiniciar();
                if (!Sondeo(log, w)) return false;
                vigia.Capturar(w, s.Draft);
                gesto.Hacer(w);
                vigia.Capturar(w, s.Draft);
                Click(w, "UpdateButton");
                vigia.Capturar(w, s.Draft);
                log.Add(Fin(
                    s,
                    gesto.Nombre + " borrador valido=" + Valido(w) + " bloqueado=" + w.OutputIsBlockedForTest + " estado=" + StatusText(w)
                        + " llamadas=" + s.Puerto.Llamadas,
                    avisos));
                return true;
            }
            finally { CerrarVentana(w); }
        }

        /// <summary>La cuarta ventana: el BOM, la biblioteca (la costura devuelve nulo), una edicion y «Aplicar», que llega al puerto.</summary>
        private static bool CuartaVentana(List<string> log, PromptProbe avisos, Vigia vigia)
        {
            var s = Nueva(log, Estado(Fixture(DisenoBase)));
            if (s == null) return false;
            var w = s.W;
            try
            {
                avisos.Reiniciar();
                if (!Sondeo(log, w)) return false;
                w.BomDialog = (bom, dueno) => { };
                w.LibraryPathDialog = (dueno, nombre) => null;
                vigia.Capturar(w, s.Draft);

                Click(w, "BomButton");
                vigia.Capturar(w, s.Draft);
                BorrarArchivoTemporal();
                Click(w, "SaveLibraryButton");
                vigia.Capturar(w, s.Draft);

                // `pb-01-base` ya tiene tolerancia 4: con 4, «Aplicar» no llegaria al puerto y `llamadas=1` lo prueba.
                SetNumberR(w, "ToleranceBox", 4.5);
                vigia.Capturar(w, s.Draft);
                Click(w, "UpdateButton");
                vigia.Capturar(w, s.Draft);
                log.Add(Fin(
                    s,
                    "cuarta-ventana llamadas=" + s.Puerto.Llamadas + " huellas-iguales=" + (vigia.Cuenta > 0 && vigia.Intacta()),
                    avisos));
                return true;
            }
            finally { CerrarVentana(w); }
        }

        [Fact]
        public void CT_PBT2B_05_WorkspaceAndClassicShareOneValidationAndNeitherMutatesTheSharedReferences()
        {
            var log = new List<string>();
            var avisos = new PromptProbe(true);
            using (EditorDiscardPrompt.Substitute(avisos.Ask))
            {
                Sta(() =>
                {
                    CrearCarpetaTemporal();
                    try
                    {
                        var vigia = new Vigia();
                        foreach (var gesto in Gestos())
                        {
                            ClasicoDeValidacion(log, avisos, gesto);
                            if (!BorradorDeValidacion(log, avisos, vigia, gesto)) return;
                        }

                        CuartaVentana(log, avisos, vigia);
                    }
                    finally { LimpiarArchivoTemporal(); }
                });
            }

            const string invalidoClasico = "Corrige los datos: no se puede insertar un modelo inválido.";
            const string bloqueante =
                "El hueco (-1\") no es una separacion fisica valida: debe ser cero o positivo. Corrigelo; no se interpreta como otro valor.";
            var esperadas = new List<string>
            {
                "campo-en-error clasico valido=False bloqueado=False estado=" + invalidoClasico + " peticion=False descartes=0",
                "campo-en-error borrador valido=False bloqueado=False estado=" + Invalido + " llamadas=0 descartes=0",
                "calculo-invalido clasico valido=False bloqueado=False estado=" + invalidoClasico + " peticion=False descartes=0",
                "calculo-invalido borrador valido=False bloqueado=False estado=" + Invalido + " llamadas=0 descartes=1",
                "bloqueante clasico valido=True bloqueado=True estado=" + bloqueante + " peticion=False descartes=0",
                "bloqueante borrador valido=True bloqueado=True estado=" + bloqueante + " llamadas=0 descartes=1",
                "cuarta-ventana llamadas=1 huellas-iguales=True descartes=0",
            };

            Assert.Equal(esperadas, log);
        }

        // ------------------------------------------------------------------------------------------------ CT-PBT2B-06

        /// <summary>Una ventana de WORKSPACE sobre su propio borrador: sonda, gestos y, en el <c>finally</c>, «Si» y <c>Cerrar</c>.</summary>
        private static bool Cierre(List<string> log, PromptProbe avisos, Action<Sesion> gestos)
        {
            var s = Nueva(log, Estado(Fixture(DisenoBase)));
            if (s == null) return false;
            try
            {
                avisos.Reiniciar();
                if (!Sondeo(log, s.W)) return false;
                gestos(s);
                return true;
            }
            finally
            {
                // `CloseQuietly` de T-CL no cierra una ventana sin mostrar: se contesta «Si» y se cierra con `Cerrar`.
                avisos.Answer = true;
                CerrarVentana(s.W);
            }
        }

        [Fact]
        public void CT_PBT2B_06_ClosingAsksOnlyForWhatWasNotApplied()
        {
            var log = new List<string>();
            var avisos = new PromptProbe(true);
            using (EditorDiscardPrompt.Substitute(avisos.Ask))
            {
                Sta(() =>
                {
                    // Ventana 1: sin cambios, «Cerrar» cierra sin preguntar.
                    if (!Cierre(log, avisos, s =>
                    {
                        Click(s.W, "CloseButton");
                        log.Add("cerrar-sin-cambios avisos=" + avisos.Asked + " cerrada=" + EstaCerradaVentana(s.W)
                            + " borrador-igual=" + Igual(s.Draft.CurrentDraftState, s.Antes));
                    }))
                    {
                        return;
                    }

                    // Ventana 2: una edicion sin aplicar; «No» la deja abierta, «Si» la cierra y el borrador queda como estaba.
                    if (!Cierre(log, avisos, s =>
                    {
                        SetNumberR(s.W, "ToleranceBox", 4.5);
                        avisos.Answer = false;
                        Click(s.W, "CloseButton");
                        log.Add("cerrar-con-cambios-no avisos=" + avisos.Asked + " pregunta=" + avisos.Question + " cerrada=" + EstaCerradaVentana(s.W));
                        avisos.Answer = true;
                        Click(s.W, "CloseButton");
                        log.Add("cerrar-con-cambios-si avisos=" + avisos.Asked + " cerrada=" + EstaCerradaVentana(s.W)
                            + " borrador-igual=" + Igual(s.Draft.CurrentDraftState, s.Antes) + " llamadas=" + s.Puerto.Llamadas);
                    }))
                    {
                        return;
                    }

                    // Ventana 3: lo editado se aplico, y «Cerrar» no pregunta.
                    if (!Cierre(log, avisos, s =>
                    {
                        SetNumberR(s.W, "ToleranceBox", 4.5);
                        Click(s.W, "UpdateButton");
                        Click(s.W, "CloseButton");
                        log.Add("cerrar-tras-aplicar avisos=" + avisos.Asked + " cerrada=" + EstaCerradaVentana(s.W) + " llamadas=" + s.Puerto.Llamadas);
                    }))
                    {
                        return;
                    }

                    // Ventana 4: un modulo escenificado sin confirmar manda primero, con su texto de siempre.
                    if (!Cierre(log, avisos, s =>
                    {
                        StageModuleEdit(s.W);
                        avisos.Answer = true;
                        Click(s.W, "CloseButton");
                        log.Add("cerrar-con-modulo avisos=" + avisos.Asked + " pregunta=" + avisos.Question + " cerrada=" + EstaCerradaVentana(s.W));
                    }))
                    {
                        return;
                    }

                    // CLASSIC: «Cerrar» no pregunta por lo editado, como siempre (ADR-0029 D8).
                    var clasica = Ventana();
                    try
                    {
                        avisos.Reiniciar();
                        Cargar(clasica, Fixture(DisenoBase));
                        SetNumberR(clasica, "ToleranceBox", 4.5);
                        Click(clasica, "CloseButton");
                        log.Add("clasico-con-cambios avisos=" + avisos.Asked + " cerrada=" + EstaCerradaVentana(clasica)
                            + " peticion=" + clasica.InsertRequested);
                    }
                    finally
                    {
                        avisos.Answer = true;
                        CerrarVentana(clasica);
                    }
                });
            }

            var esperadas = new List<string>
            {
                "cerrar-sin-cambios avisos=0 cerrada=True borrador-igual=True",
                "cerrar-con-cambios-no avisos=1 pregunta=" + PreguntaSinAplicar + " cerrada=False",
                "cerrar-con-cambios-si avisos=2 cerrada=True borrador-igual=True llamadas=0",
                "cerrar-tras-aplicar avisos=0 cerrada=True llamadas=1",
                "cerrar-con-modulo avisos=1 pregunta=" + PreguntaModulo + " cerrada=True",
                "clasico-con-cambios avisos=0 cerrada=True peticion=False",
            };

            Assert.Equal(esperadas, log);
        }
    }
}
