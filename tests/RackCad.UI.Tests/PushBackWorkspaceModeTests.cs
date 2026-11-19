using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Workspace.Drafts;
using RackCad.Domain.Systems.PushBack;
using RackCad.UI.Shell;
using RackCad.UI.Systems.PushBack;
using Xunit;
using static RackCad.UI.Tests.PushBackDeclaredCorrectionSupport;
using static RackCad.UI.Tests.PushBackFullEditorCharacterizationSupport;
using static RackCad.UI.Tests.PushBackWorkspaceSupport;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 F4B PB-T2b - CT-PBT2B-01, 02 y 07: la ventana de Push Back tiene dos modos explicitos y fijos (CLASSIC, sin cambio
    /// observable, y WORKSPACE, solo por <c>RackPushBackSystemWindow.ForDraft</c>), WORKSPACE carga una COPIA del estado del
    /// borrador sin pasar por un diseño y es la ventana quien posee el BOM y el dialogo de guardado (decisiones D-PBT2-04,
    /// D-PBT2-05 y D-PBT2-10 del paquete del contrato de PB-T2).
    ///
    /// <para>Cada metodo arma una lista de lineas y acaba en UN <c>Assert.Equal</c>. Toda ventana se crea sin mostrar, su carga va
    /// DENTRO del <c>try</c> y se cierra en el <c>finally</c>; no hay teclado ni foco real; el aviso de descarte siempre esta
    /// sustituido y su contador, por ventana, va en el sufijo <c>descartes=&lt;n&gt;</c> de la ultima linea de cada una, leido
    /// tras el <c>Cerrar</c> explicito. Todo paso que llama al API nuevo (<c>ForDraft</c>, <c>ModeForTest</c> y las cargas
    /// publicas en WORKSPACE) pasa por <see cref="PushBackWorkspaceSupport.Paso{T}"/>, que convierte una excepcion en una linea
    /// «EXCEPCION»; las lineas esperadas solo contienen las que fija el paquete.</para>
    ///
    /// <para><b>La sonda del modo.</b> Antes de cualquier gesto de una ventana de WORKSPACE se lee su modo: si no es
    /// <c>Workspace</c>, se deja la linea <c>modo=&lt;modo&gt;</c> y la prueba no hace ningun gesto mas (nunca se ejercita una
    /// ventana CLASSIC como si fuera del borrador) y acaba con su <c>Assert.Equal</c>.</para>
    /// </summary>
    [Collection(EditorDiscardPromptCollection.Name)]
    public sealed class PushBackWorkspaceModeTests
    {
        private static string Ocultos(RackPushBackSystemWindow w)
            => string.Join(",", BotonesDeInsercion.Where(nombre => Btn(w, nombre).Visibility != Visibility.Visible));

        private static string Visibles(RackPushBackSystemWindow w)
            => string.Join(",", new[] { "BomButton", "SaveLibraryButton", "RestoreButton", "CloseButton" }.Where(nombre => Btn(w, nombre).Visibility == Visibility.Visible));

        /// <summary>Cierra la ventana y termina la linea con el contador de avisos de esa ventana, leido tras cerrarla.</summary>
        private static string Fin(RackPushBackSystemWindow w, string linea, PromptProbe avisos)
        {
            CerrarVentana(w);
            return linea + " descartes=" + avisos.Asked;
        }

        // ------------------------------------------------------------------------------------------------ CT-PBT2B-01

        private static string LineaClasica(RackPushBackSystemWindow w)
            => "clasico modo=" + w.ModeForTest + " boton=" + Btn(w, "UpdateButton").Content + " ocultos=[" + Ocultos(w) + "]";

        [Fact]
        public void CT_PBT2B_01_TheModeIsExplicitAndFixedAndClassicDoesNotChange()
        {
            var log = new List<string>();
            var avisos = new PromptProbe(true);
            // El primer Sta del proceso crea el hilo STA e instala en el su aviso de descarte permanente: se arranca ANTES de sustituir
            // el aviso de esta prueba, o el primero que corra en el proceso la dejaria sin su sustituto.
            Sta(() => { });
            using (EditorDiscardPrompt.Substitute(avisos.Ask))
            {
                Sta(() =>
                {
                    // (i) CLASSIC: la ventana de siempre, por S0.Ventana() y por los dos constructores publicos.
                    var clasica = Ventana();
                    try
                    {
                        avisos.Reiniciar();
                        Cargar(clasica, Fixture(DisenoBase));
                        log.Add(Fin(clasica, LineaClasica(clasica), avisos));
                    }
                    finally { CerrarVentana(clasica); }

                    var sinArgumentos = Registrar(new RackPushBackSystemWindow());
                    try
                    {
                        avisos.Reiniciar();
                        log.Add(Fin(sinArgumentos, LineaClasica(sinArgumentos), avisos));
                    }
                    finally { CerrarVentana(sinArgumentos); }

                    var conBooleano = Registrar(new RackPushBackSystemWindow(false, null));
                    try
                    {
                        avisos.Reiniciar();
                        log.Add(Fin(conBooleano, LineaClasica(conBooleano), avisos));
                    }
                    finally { CerrarVentana(conBooleano); }

                    // (ii) WORKSPACE, solo por la factoria tipada, y (iii) sus cargas publicas, que lanzan.
                    var (_, draft) = Borrador(Estado(Fixture(DisenoBase)));
                    var puerto = new PuertoContado(draft);
                    var w = Paso(log, () => VentanaBorrador(draft, null, puerto));
                    try
                    {
                        if (w == null) return;
                        avisos.Reiniciar();
                        var modo = Paso(log, () => w.ModeForTest);
                        log.Add("modo=" + modo + " boton=" + Btn(w, "UpdateButton").Content + " tooltip=" + Btn(w, "UpdateButton").ToolTip
                            + " ocultos=[" + Ocultos(w) + "] visibles=[" + Visibles(w) + "]"
                            + " dueno-de-la-ventana=" + (w.Owner == null ? "<null>" : w.Owner.GetType().Name));
                        if (modo != PushBackEditorMode.Workspace) return;

                        var sobre = Fixture(DisenoBase);
                        Paso(log, () => w.LoadExisting(DisenoDeSobre(sobre), IdBorrador, NombreDeSobre(sobre)));
                        Paso(log, () => w.LoadDesignForNew(DisenoDeSobre(sobre), NombreDeSobre(sobre)));
                        Paso(log, () => w.LoadNew());
                        log.Add(Fin(w, "modo=" + Paso(log, () => w.ModeForTest), avisos));
                    }
                    finally { CerrarVentana(w); }

                    // (iv) La factoria no admite un lanzamiento nulo.
                    var ninguna = Paso(log, () => RackPushBackSystemWindow.ForDraft(null));
                    if (ninguna != null)
                    {
                        Registrar(ninguna);
                        CerrarVentana(ninguna);
                    }
                });
            }

            var esperadas = new List<string>
            {
                "clasico modo=Classic boton=Actualizar ocultos=[] descartes=0",
                "clasico modo=Classic boton=Actualizar ocultos=[] descartes=0",
                "clasico modo=Classic boton=Actualizar ocultos=[] descartes=0",
                "modo=Workspace boton=Aplicar al borrador tooltip=" + AplicarTooltip
                    + " ocultos=[InsertButton,InsertLateralButton,InsertFrontalEntradaButton,InsertFrontalPosteriorButton,InsertPlantaButton,InsertBatchButton]"
                    + " visibles=[BomButton,SaveLibraryButton,RestoreButton,CloseButton] dueno-de-la-ventana=<null>",
                "EXCEPCION InvalidOperationException: " + RechazoCarga,
                "EXCEPCION InvalidOperationException: " + RechazoCarga,
                "EXCEPCION InvalidOperationException: " + RechazoCarga,
                "modo=Workspace descartes=0",
                "EXCEPCION ArgumentNullException: Value cannot be null. (Parameter 'launch')",
            };

            Assert.Equal(esperadas, log);
        }

        // ------------------------------------------------------------------------------------------------ CT-PBT2B-02

        /// <summary>
        /// Una fila de WORKSPACE: una ventana nueva sobre el borrador de un estado de entrada, la sonda del modo y la linea que la
        /// fila lee de la ventana ya cargada, cerrada con el contador de avisos de esa ventana. Devuelve false si la sonda no da
        /// <c>Workspace</c>: la prueba no hace ningun gesto mas.
        /// </summary>
        private static bool Fila(
            List<string> log,
            PromptProbe avisos,
            PushBackEditableState entrada,
            Func<RackPushBackSystemWindow, DraftSession<PushBackEditableState>, PuertoContado, string> lineaDe)
        {
            var (_, draft) = Borrador(entrada);
            var puerto = new PuertoContado(draft);
            var w = Paso(log, () => VentanaBorrador(draft, null, puerto));
            try
            {
                if (w == null) return false;
                avisos.Reiniciar();
                if (!Sondeo(log, w)) return false;
                log.Add(Fin(w, lineaDe(w, draft, puerto), avisos));
                return true;
            }
            finally { CerrarVentana(w); }
        }

        /// <summary>
        /// <c>iguales</c>, <c>mismo-objeto</c> y <c>diseno-igual</c> con un recalculo neutro antes de leer el diseño (§5.0, V23-02):
        /// el diseño de <c>LastComputation</c> del compuesto puede ir una pasada por detras del estado.
        /// </summary>
        private static string Comparacion(RackPushBackSystemWindow w, DraftSession<PushBackEditableState> draft, bool conMismoObjeto)
        {
            Recalcular(w);
            return "iguales=" + Igual(w.EditableStateForTest, draft.CurrentDraftState)
                + (conMismoObjeto ? " mismo-objeto=" + ReferenceEquals(w.EditableStateForTest, draft.CurrentDraftState) : string.Empty)
                + " diseno-igual=" + DisenoIgual(w, draft);
        }

        private static bool DisenoIgual(RackPushBackSystemWindow w, DraftSession<PushBackEditableState> draft)
            => DisenoDe(draft.CurrentDraftState, PushBackSide.A) == DisenoDeVentana(w);

        [Fact]
        public void CT_PBT2B_02_WorkspaceLoadsACopyOfTheDraftStateWithoutGoingThroughADesign()
        {
            var log = new List<string>();
            var esperadas = new List<string>();
            var avisos = new PromptProbe(true);
            // El primer Sta del proceso crea el hilo STA e instala en el su aviso de descarte permanente: se arranca ANTES de sustituir
            // el aviso de esta prueba, o el primero que corra en el proceso la dejaria sin su sustituto.
            Sta(() => { });
            using (EditorDiscardPrompt.Substitute(avisos.Ask))
            {
                Sta(() =>
                {
                    // El corpus C18. Lo esperado de `nombre` sale del estado de entrada, leido antes de abrir la ventana.
                    foreach (var (diseno, estado) in CorpusC18())
                    {
                        var nombre = diseno;
                        esperadas.Add(nombre + " iguales=True mismo-objeto=False diseno-igual=True id=" + IdBorrador
                            + " nombre=" + Nombre(estado.Name) + " peticion=False descartes=0");
                        if (!Fila(log, avisos, estado, (w, draft, puerto) =>
                            nombre + " " + Comparacion(w, draft, true) + " id=" + w.RackId + " nombre=" + Nombre(w.RackName) + " peticion=" + w.InsertRequested))
                        {
                            return;
                        }
                    }

                    // N-PBT2-01 (a): las vistas de cotas tocadas se normalizan, sin cambio de salida.
                    var tocado = Estado(Fixture(DisenoBase));
                    tocado.DimensionsLateral = false;
                    tocado.DimensionViewsTouched = true;
                    esperadas.Add("N-PBT2-01a iguales=False tocado=True->False vistas=->Frontal, Planta diseno-igual=True descartes=0");
                    if (!Fila(log, avisos, tocado, (w, draft, puerto) =>
                    {
                        Recalcular(w);
                        return "N-PBT2-01a iguales=" + Igual(w.EditableStateForTest, draft.CurrentDraftState)
                            + " tocado=" + tocado.DimensionViewsTouched + "->" + w.EditableStateForTest.DimensionViewsTouched
                            + " vistas=" + tocado.LoadedDimensionViews + "->" + w.EditableStateForTest.LoadedDimensionViews
                            + " diseno-igual=" + DisenoIgual(w, draft);
                    }))
                    {
                        return;
                    }

                    // La reapertura con una seleccion que no es la de por defecto. Lo esperado sale del estado de entrada.
                    var filas = new (string Fila, string Lado, PushBackEditableState Entrada)[]
                    {
                        ("ambos-mixto", "Both", EstadoDeVentana(
                            Fixture(DisenoCompuesto),
                            w => Combo(w, "SideSelectorBox").SelectedIndex = 1,
                            w => SetNumberR(w, "CellPalletHeightBox", 55),
                            w => Click(w, "ApplyCellButton"),
                            w => Combo(w, "SideSelectorBox").SelectedIndex = 2)),
                        ("lado-B", "B", EstadoDeVentana(Fixture(DisenoCompuesto), w => Combo(w, "SideSelectorBox").SelectedIndex = 1)),
                        ("celda-1-0", "A", EstadoDeVentana(Fixture("pb-08-redondeo"), w => w.SelectMatrixCell(1, 0, false))),
                    };
                    foreach (var (fila, lado, entrada) in filas)
                    {
                        var indices = Sel(entrada).Substring(Sel(entrada).IndexOf('/') + 1);
                        esperadas.Add(fila + " iguales=True diseno-igual=True seleccion=" + lado + "/" + indices + " descartes=0");
                        if (!Fila(log, avisos, entrada, (w, draft, puerto) =>
                            fila + " " + Comparacion(w, draft, false) + " seleccion=" + Sel(w.EditableStateForTest)))
                        {
                            return;
                        }
                    }

                    // El nombre: la carga no lo normaliza, ni el nulo ni el que lleva espacios.
                    var conNombreNulo = Estado(Fixture(DisenoBase));
                    conNombreNulo.Name = null;
                    esperadas.Add("nombre-nulo iguales=True nombre=<null> descartes=0");
                    if (!Fila(log, avisos, conNombreNulo, (w, draft, puerto) =>
                        "nombre-nulo iguales=" + Igual(w.EditableStateForTest, draft.CurrentDraftState) + " nombre=" + Nombre(w.RackName)))
                    {
                        return;
                    }

                    var conEspacios = Estado(Fixture(DisenoBase));
                    conEspacios.Name = "  PB  Uno  ";
                    esperadas.Add("nombre-con-espacios iguales=True nombre=[  PB  Uno  ] descartes=0");
                    if (!Fila(log, avisos, conEspacios, (w, draft, puerto) =>
                        "nombre-con-espacios iguales=" + Igual(w.EditableStateForTest, draft.CurrentDraftState) + " nombre=" + Nombre(w.RackName)))
                    {
                        return;
                    }

                    // Un estado invalido: nada del `LoadNew` encadenado llega al borrador y «Restaurar» se niega.
                    var invalido = Estado(Fixture(DisenoBase));
                    invalido.Advanced.DerivedPostReinforced = true;
                    invalido.Advanced.DerivedPostReinforcementHeight = 100000;
                    esperadas.Add("estado-invalido valido=False restaurar-habilitado=False tras-restaurar=Todavía no hay un sistema válido que restaurar."
                        + " llamadas-tras-aplicar=0 descartes=0");
                    Fila(log, avisos, invalido, (w, draft, puerto) =>
                    {
                        var valido = w.CurrentInputsAreValid;
                        var restaurar = Btn(w, "RestoreButton").IsEnabled;
                        Click(w, "RestoreButton");
                        var tras = StatusText(w);
                        Click(w, "UpdateButton");
                        return "estado-invalido valido=" + valido + " restaurar-habilitado=" + restaurar + " tras-restaurar=" + tras
                            + " llamadas-tras-aplicar=" + puerto.Llamadas;
                    });
                });
            }

            Assert.Equal(esperadas, log);
        }

        // ------------------------------------------------------------------------------------------------ CT-PBT2B-07

        [Fact]
        public void CT_PBT2B_07_InWorkspaceTheWindowOwnsTheBomAndTheSaveDialog()
        {
            var log = new List<string>();
            var avisos = new PromptProbe(true);
            // El primer Sta del proceso crea el hilo STA e instala en el su aviso de descarte permanente: se arranca ANTES de sustituir
            // el aviso de esta prueba, o el primero que corra en el proceso la dejaria sin su sustituto.
            Sta(() => { });
            using (EditorDiscardPrompt.Substitute(avisos.Ask))
            {
                Sta(() =>
                {
                    CrearCarpetaTemporal();
                    try
                    {
                        var (_, draft) = Borrador(Estado(Fixture(DisenoBase)));
                        var puerto = new PuertoContado(draft);
                        var w = Paso(log, () => VentanaBorrador(draft, null, puerto));
                        try
                        {
                            if (w == null) return;
                            avisos.Reiniciar();
                            if (!Sondeo(log, w)) return;

                            // Cada costura guarda en el acto lo que recibe, sin aserciones; la linea se arma fuera.
                            var bomLlamadas = 0;
                            Window bomDueno = null;
                            var bomEsElCalculado = false;
                            w.BomDialog = (bom, dueno) =>
                            {
                                bomLlamadas++;
                                bomDueno = dueno;
                                bomEsElCalculado = ReferenceEquals(bom, w.LastComputation.Bom);
                            };
                            var libreriaLlamadas = 0;
                            Window libreriaDueno = null;
                            string libreriaNombre = null;
                            w.LibraryPathDialog = (dueno, nombre) =>
                            {
                                libreriaLlamadas++;
                                libreriaDueno = dueno;
                                libreriaNombre = nombre;
                                return null;
                            };

                            // `Bom_Click` recalcula antes de llamar a la costura, asi que `bom-es-el-calculado` se compara en el acto y
                            // la linea se arma justo tras el clic, antes de «Guardar en biblioteca», que recalcula otra vez.
                            Click(w, "BomButton");
                            log.Add("BomDialog llamadas=" + bomLlamadas + " dueno-es-la-ventana=" + ReferenceEquals(bomDueno, w)
                                + " bom-es-el-calculado=" + bomEsElCalculado);

                            BorrarArchivoTemporal();
                            Click(w, "SaveLibraryButton");
                            log.Add(Fin(
                                w,
                                "LibraryPathDialog llamadas=" + libreriaLlamadas + " dueno-es-la-ventana=" + ReferenceEquals(libreriaDueno, w)
                                    + " nombre=" + libreriaNombre + " archivo=" + File.Exists(ArchivoTemporal) + " peticion=" + w.InsertRequested,
                                avisos));
                        }
                        finally { CerrarVentana(w); }
                    }
                    finally { LimpiarArchivoTemporal(); }
                });
            }

            var esperadas = new List<string>
            {
                "BomDialog llamadas=1 dueno-es-la-ventana=True bom-es-el-calculado=True",
                "LibraryPathDialog llamadas=1 dueno-es-la-ventana=True nombre=PB-01 archivo=False peticion=False descartes=0",
            };

            Assert.Equal(esperadas, log);
        }
    }
}
