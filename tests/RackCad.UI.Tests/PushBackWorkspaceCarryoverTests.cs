using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using RackCad.Application.Drawing;
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
    /// I-64 F4B PB-T2c - lo que la ventana de Push Back hereda de RACKEDITAR cuando corre en modo WORKSPACE (paquete del contrato de
    /// PB-T2, seccion 5.4): CT-PBT2C-01, el paso 4 de D-PB-06 (los valores exactos y el estilo de cotas llegan AL BORRADOR);
    /// CT-PBT2C-02, el aviso del lado B de D-PB-08; y CT-PBT2C-03, el censo de controles en el camino al borrador (ningun control es
    /// la fuente de verdad). No cambia el producto: son pruebas nuevas sobre lo que ya hace.
    ///
    /// <para>Cada metodo arma una lista de lineas y acaba en UN <c>Assert.Equal</c>. Toda ventana es de WORKSPACE
    /// (<c>ForDraft</c>, por <see cref="PushBackWorkspaceSupport.VentanaBorrador"/>), se crea sin mostrar, su carga va DENTRO del
    /// <c>try</c> y se cierra en el <c>finally</c>; la sonda del modo va antes de cualquier gesto; no hay teclado ni foco real, asi
    /// que un clic es un <c>RaiseEvent</c> y una edicion numerica es <c>SetNumberR</c>, que eleva <c>LostFocus</c> y recalcula. El
    /// aviso de descarte siempre esta sustituido y su contador, por ventana, va en el sufijo <c>descartes=&lt;n&gt;</c> de la
    /// ultima linea de cada una, leido tras el <c>Cerrar</c> explicito. Ninguna prueba escribe en disco ni abre el BOM, el
    /// configurador o «Insertar».</para>
    ///
    /// <para><b>El momento de cada lectura.</b> Todo se lee DEL BORRADOR, justo tras «Aplicar», con
    /// <c>c = Pasada(Copy(draft.CurrentDraftState), A)</c>: «Aplicar» que aplica deja en el borrador una copia NUEVA, y nunca se lee
    /// la peticion clasica, que en WORKSPACE es nula. <c>aplicado</c> es que el borrador cambio de objeto, con el borrador tomado
    /// justo antes del clic en «Aplicar».</para>
    /// </summary>
    [Collection(EditorDiscardPromptCollection.Name)]
    public sealed class PushBackWorkspaceCarryoverTests
    {
        private const string Fondo = "pb-08-redondeo";
        private const string Escala = "Structure.AnnotationScale";
        private const string Altura00 = "Structure.Fronts[0].Levels[0].PalletHeight";

        /// <summary>Una ventana de WORKSPACE con su borrador y su puerto contado.</summary>
        private sealed class Sesion
        {
            public RackPushBackSystemWindow W;
            public DraftSession<PushBackEditableState> Draft;
            public PuertoContado Puerto;
        }

        private static Sesion Nueva(List<string> log, PushBackEditableState estado)
        {
            var (_, draft) = Borrador(estado);
            var puerto = new PuertoContado(draft);
            var w = Paso(log, () => VentanaBorrador(draft, null, puerto));
            return w == null ? null : new Sesion { W = w, Draft = draft, Puerto = puerto };
        }

        /// <summary>El resultado de la pasada de recalculo sobre una COPIA del estado del borrador: el <c>c</c> del paquete.</summary>
        private static PushBackEditorComputation Leida(DraftSession<PushBackEditableState> draft)
            => Pasada(Copy(draft.CurrentDraftState), PushBackSide.A);

        // ------------------------------------------------------------------------------------------------ CT-PBT2C-01

        /// <summary>
        /// Las cotas de una computacion: las instancias de rol <c>Dimension</c> de los cinco planos del rack (copia de CT-PBTC-05,
        /// aplicada a la computacion <c>c</c> del borrador y no a <c>w.LastComputation</c>).
        /// </summary>
        private static IEnumerable<HeaderBlockInstance> Cotas(PushBackEditorComputation computation)
        {
            var plans = new List<HeaderRunPlan> { computation.PlantaPlan, computation.FrontalEntradaSalida, computation.FrontalPosterior, computation.LateralPlan };
            plans.AddRange(computation.LateralCortes.Select(corte => corte.Plan));
            return plans
                .Where(plan => plan != null)
                .SelectMany(plan => plan.Flatten().Instances)
                .Where(instance => instance.Role == HeaderBlockRole.Dimension);
        }

        /// <summary>
        /// Una fila de CT-PBT2C-01: una ventana de WORKSPACE sobre el estado de <paramref name="sobre"/>, la sonda, el gesto neutro
        /// (la casilla escucha <c>Click</c>, asi que el cambio no recalcula solo y lo lee el recalculo de «Aplicar»), «Aplicar» y la
        /// linea que <paramref name="lineaDe"/> lee del borrador. Devuelve false si la sonda no da <c>Workspace</c>.
        /// </summary>
        private static bool Aplicada(List<string> log, PromptProbe avisos, string sobre, Func<Sesion, bool, string> lineaDe)
        {
            var estado = Paso(log, () => Estado(sobre));
            if (estado == null) return false;
            var s = Nueva(log, estado);
            if (s == null) return false;
            var w = s.W;
            try
            {
                avisos.Reiniciar();
                if (!Sondeo(log, w)) return false;

                var casilla = Check(w, "DrawRackNameCheck");
                casilla.IsChecked = !(casilla.IsChecked == true);
                var actual = s.Draft.CurrentDraftState;
                Click(w, "UpdateButton");
                var aplicado = !ReferenceEquals(actual, s.Draft.CurrentDraftState);
                var linea = Paso(log, () => lineaDe(s, aplicado));
                CerrarVentana(w);
                log.Add(linea + " descartes=" + avisos.Asked);
                return true;
            }
            finally { CerrarVentana(w); }
        }

        [Fact]
        public void CT_PBT2C_01_InWorkspaceTheDraftKeepsTheExactValuesAndTheNamedStyle()
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
                    // Por fila F1 a F11: el exacto llega al borrador. Se lee Persist(c.Design), que es DisenoDe(borrador, A).
                    foreach (var fila in FilasF())
                    {
                        var f = fila;
                        if (!Aplicada(log, avisos, VarianteDe(f), (s, aplicado) =>
                            f.Name + " WORKSPACE aplicado=" + aplicado + " peticion=" + s.W.InsertRequested
                            + " diseno=" + Num(DisenoDe(s.Draft.CurrentDraftState, PushBackSide.A), f.Path)))
                        {
                            return;
                        }
                    }

                    // pb-08-redondeo tal cual: la escala y la altura del diseno y la escala del sistema, los tres del borrador.
                    if (!Aplicada(log, avisos, Fixture(Fondo), (s, aplicado) =>
                    {
                        var diseno = DisenoDe(s.Draft.CurrentDraftState, PushBackSide.A);
                        var c = Leida(s.Draft);
                        return Fondo + " WORKSPACE aplicado=" + aplicado + " peticion=" + s.W.InsertRequested
                            + " AnnotationScale=" + Num(diseno, Escala) + " PalletHeight=" + Num(diseno, Altura00)
                            + " sistema.AnnotationScale=" + Texto(c.System.Structure.AnnotationScale);
                    }))
                    {
                        return;
                    }

                    // El estilo de cotas con nombre: del diseno y del sistema del borrador y de cada cota de sus planos.
                    foreach (var fixture in new[] { "pb-06-seguridad", "pb-07-combinado" })
                    {
                        var nombre = fixture;
                        if (!Aplicada(log, avisos, Fixture(nombre), (s, aplicado) =>
                        {
                            var c = Leida(s.Draft);
                            var cotas = Cotas(c).ToList();
                            var estilos = cotas
                                .Select(cota => cota.DimensionStyleName ?? "<null>")
                                .Distinct()
                                .OrderBy(estilo => estilo, StringComparer.Ordinal);
                            return nombre + " WORKSPACE aplicado=" + aplicado
                                + " diseno=" + (c.Design.Structure.DimensionStyle ?? "<null>")
                                + " sistema=" + (c.System.Structure.DimensionStyle ?? "<null>")
                                + " cotas>0=" + (cotas.Count > 0)
                                + " estilos=[" + string.Join(",", estilos) + "]";
                        }))
                        {
                            return;
                        }
                    }
                });
            }

            var esperadas = new List<string>
            {
                "F1 WORKSPACE aplicado=True peticion=False diseno=48.00044 descartes=0",
                "F2 WORKSPACE aplicado=True peticion=False diseno=3.00044 descartes=0",
                "F3 WORKSPACE aplicado=True peticion=False diseno=4.00044 descartes=0",
                "F4 WORKSPACE aplicado=True peticion=False diseno=6 descartes=0",
                "F5 WORKSPACE aplicado=True peticion=False diseno=1.50044 descartes=0",
                "F6 WORKSPACE aplicado=True peticion=False diseno=4.00044 descartes=0",
                "F7 WORKSPACE aplicado=True peticion=False diseno=42.00044 descartes=0",
                "F8 WORKSPACE aplicado=True peticion=False diseno=60.00044 descartes=0",
                "F9 WORKSPACE aplicado=True peticion=False diseno=1000.00044 descartes=0",
                "F10 WORKSPACE aplicado=True peticion=False diseno=6.00044 descartes=0",
                "F11 WORKSPACE aplicado=True peticion=False diseno=100.00044 descartes=0",
                "pb-08-redondeo WORKSPACE aplicado=True peticion=False AnnotationScale=1.23456 PalletHeight=61.23456 sistema.AnnotationScale=1.23456 descartes=0",
                "pb-06-seguridad WORKSPACE aplicado=True diseno=STANDARD-ISO sistema=STANDARD-ISO cotas>0=True estilos=[STANDARD-ISO] descartes=0",
                "pb-07-combinado WORKSPACE aplicado=True diseno=SIMPLEX sistema=SIMPLEX cotas>0=True estilos=[SIMPLEX] descartes=0",
            };

            Assert.Equal(esperadas, log);
        }

        // ------------------------------------------------------------------------------------------------ CT-PBT2C-02

        private const string Grid = "PushBackMatrixGrid";

        private static void Blank(RackPushBackSystemWindow w, int front, bool blank) => EditorWindowTestSupport.SetFrontBlank(w, Grid, front, blank);

        private static void Side(RackPushBackSystemWindow w, int index) => Combo(w, "SideSelectorBox").SelectedIndex = index;

        private static void DeclareB(RackPushBackSystemWindow w, bool present) => Check(w, "SideBPresentCheck").IsChecked = present;

        /// <summary><c>&lt;paso&gt; aviso=&lt;Visibility&gt; compuesto=&lt;IsComposite&gt;</c>. Primero toca el aviso: sin el, falla ahi.</summary>
        private static string Linea(RackPushBackSystemWindow w, string paso)
        {
            var aviso = Aviso(w).Visibility;
            return paso + " aviso=" + aviso + " compuesto=" + w.LastComputation.Design.IsComposite;
        }

        /// <summary>
        /// Una ventana de WORKSPACE de CT-PBT2C-02: la sonda y los gestos, que dejan sus lineas en el registro salvo la ultima, que
        /// devuelven. La ultima se completa tras el <c>Cerrar</c> explicito con el contador de avisos de esa ventana.
        /// </summary>
        private static bool UnaVentana(List<string> log, PromptProbe avisos, string sobre, Func<Sesion, string> gestos)
        {
            var estado = Paso(log, () => Estado(sobre));
            if (estado == null) return false;
            var s = Nueva(log, estado);
            if (s == null) return false;
            try
            {
                avisos.Reiniciar();
                if (!Sondeo(log, s.W)) return false;
                var ultima = Paso(log, () => gestos(s));
                CerrarVentana(s.W);
                log.Add(ultima + " descartes=" + avisos.Asked);
                return true;
            }
            finally { CerrarVentana(s.W); }
        }

        [Fact]
        public void CT_PBT2C_02_InWorkspaceTheSideBNoticeShowsExactlyAsInRackeditar()
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
                    // Los gestos de CT-PBTC-07 sobre pb-01-base (CT-PB0-09), copiados.
                    if (!UnaVentana(log, avisos, Fixture("pb-01-base"), s =>
                    {
                        var w = s.W;
                        log.Add(Linea(w, "pb-01-base carga"));
                        DeclareB(w, true);
                        log.Add(Linea(w, "pb-01-base declarar B"));
                        log.Add("texto=" + Aviso(w).Text);
                        Click(w, "AddFrontButton");
                        log.Add(Linea(w, "pb-01-base anadir frente"));
                        Side(w, 1);
                        Blank(w, 1, false);
                        log.Add(Linea(w, "pb-01-base lado B, ranura 1 de B activa"));
                        Side(w, 0);
                        Blank(w, 1, true);
                        Blank(w, 2, true);
                        log.Add(Linea(w, "pb-01-base ranuras 1 y 2 de A en blanco"));
                        Side(w, 1);
                        Blank(w, 1, true);
                        log.Add(Linea(w, "pb-01-base ranura 1 de B en blanco"));
                        DeclareB(w, false);
                        return Linea(w, "pb-01-base retirar B");
                    }))
                    {
                        return;
                    }

                    // Los gestos de CT-PBTC-07 sobre pb-20-ab (CT-PB0-07), copiados.
                    if (!UnaVentana(log, avisos, Fixture("pb-20-ab"), s =>
                    {
                        var ab = s.W;
                        log.Add(Linea(ab, "pb-20-ab carga"));
                        DeclareB(ab, false);
                        log.Add(Linea(ab, "pb-20-ab retirar B"));
                        DeclareB(ab, true);
                        log.Add(Linea(ab, "pb-20-ab declarar B"));
                        Side(ab, 1);
                        for (var slot = 0; slot < ab.CompositeState.SideB.Structure.Count; slot++) Blank(ab, slot, true);
                        return Linea(ab, "pb-20-ab todas las ranuras de B en blanco");
                    }))
                    {
                        return;
                    }

                    // Declarar B y «Aplicar»: el aviso sigue visible, el boton habilitado y el borrador lleva B aunque el diseno no sea compuesto.
                    UnaVentana(log, avisos, Fixture("pb-01-base"), s =>
                    {
                        var w = s.W;
                        DeclareB(w, true);
                        Click(w, "UpdateButton");
                        var c = Leida(s.Draft);
                        return "pb-01-base declarar B y aplicar aviso=" + Aviso(w).Visibility
                            + " aplicar-habilitado=" + Btn(w, "UpdateButton").IsEnabled
                            + " llamadas=" + s.Puerto.Llamadas
                            + " borrador-B-presente=" + s.Draft.CurrentDraftState.Editor.SideBPresent
                            + " diseno-del-borrador-compuesto=" + c.Design.IsComposite;
                    });
                });
            }

            var esperadas = new List<string>
            {
                "pb-01-base carga aviso=Collapsed compuesto=False",
                "pb-01-base declarar B aviso=Visible compuesto=False",
                "texto=Lado B declarado sin ningún frente activo: así no se guardará. Para conservarlo, elige «Lado B» y desmarca «En blanco» en al menos un frente.",
                "pb-01-base anadir frente aviso=Visible compuesto=False",
                "pb-01-base lado B, ranura 1 de B activa aviso=Collapsed compuesto=True",
                "pb-01-base ranuras 1 y 2 de A en blanco aviso=Collapsed compuesto=True",
                "pb-01-base ranura 1 de B en blanco aviso=Visible compuesto=False",
                "pb-01-base retirar B aviso=Collapsed compuesto=False descartes=1",
                "pb-20-ab carga aviso=Collapsed compuesto=True",
                "pb-20-ab retirar B aviso=Collapsed compuesto=False",
                "pb-20-ab declarar B aviso=Collapsed compuesto=True",
                "pb-20-ab todas las ranuras de B en blanco aviso=Visible compuesto=False descartes=1",
                "pb-01-base declarar B y aplicar aviso=Visible aplicar-habilitado=True llamadas=1 borrador-B-presente=True diseno-del-borrador-compuesto=False descartes=0",
            };

            Assert.Equal(esperadas, log);
        }

        // ------------------------------------------------------------------------------------------------ CT-PBT2C-03

        /// <summary>Un gesto del censo de CT-PBT1C-02: su nombre, el diseno sobre el que corre y sus pasos; cada paso es un gesto del usuario.</summary>
        private sealed class Gesto
        {
            public string Nombre;
            public string Diseno;
            public Action<RackPushBackSystemWindow>[] Pasos;
        }

        private const string Base = "pb-01-base";
        private const string Compuesto = "pb-20-ab";

        private static Gesto G(string nombre, string diseno, params Action<RackPushBackSystemWindow>[] pasos)
            => new Gesto { Nombre = nombre, Diseno = diseno, Pasos = pasos };

        /// <summary>Un clic del usuario en una caja que escucha <c>Click</c>: invierte la caja y eleva el clic real.</summary>
        private static void Pulsar(CheckBox casilla)
        {
            casilla.IsChecked = !(casilla.IsChecked == true);
            casilla.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, casilla));
        }

        /// <summary>La otra opcion de una lista: la primera, o la segunda cuando la primera esta elegida.</summary>
        private static void Otro(ComboBox caja) => caja.SelectedIndex = caja.Items.Count > 1 && caja.SelectedIndex == 0 ? 1 : 0;

        private static void Alternar(CheckBox casilla) => casilla.IsChecked = !(casilla.IsChecked == true);

        private static void Escribir(RackPushBackSystemWindow w, string caja, double valor) => SetNumberR(w, caja, valor);

        /// <summary>Los 49 gestos del censo con <c>Compara</c> = true de <c>Gestos()</c> de CT-PBT1C-02, copiados, en su orden y con su nombre.</summary>
        private static IEnumerable<Gesto> Gestos()
        {
            // Las cajas de todo el rack.
            yield return G("DepthBox=54", Base, w => Escribir(w, "DepthBox", 54));
            yield return G("ToleranceBox=4", Base, w => Escribir(w, "ToleranceBox", 4));
            yield return G("PostPeralteBox=+0.5", Base, w => Escribir(w, "PostPeralteBox", ValueOf(w, "PostPeralteBox").Value + 0.5));
            yield return G("BeamDepthBox=+1", Base, w => Escribir(w, "BeamDepthBox", ValueOf(w, "BeamDepthBox").Value + 1.0));
            yield return G("AnnotationScaleBox=2", Base, w => Escribir(w, "AnnotationScaleBox", 2));
            yield return G("WeightUnitBox=lb", Base, w => Combo(w, "WeightUnitBox").SelectedItem = "lb");
            yield return G("PalletsDeepBox=9", Base, w => Escribir(w, "PalletsDeepBox", 9));
            yield return G("PostBox=otro", Base, w => Otro(Combo(w, "PostBox")));
            yield return G("NumberFrontsCheck", Base, w => Pulsar(Check(w, "NumberFrontsCheck")));
            yield return G("NumberLevelsCheck", Base, w => Pulsar(Check(w, "NumberLevelsCheck")));
            yield return G("DrawRackNameCheck", Base, w => Pulsar(Check(w, "DrawRackNameCheck")));
            yield return G("DimensionsBox=3", Base, w => Combo(w, "DimensionsBox").SelectedIndex = 3);
            yield return G("DimensionsFrontalCheck", Base, w => Alternar(Check(w, "DimensionsFrontalCheck")));
            yield return G("DimensionsLateralCheck", Base, w => Alternar(Check(w, "DimensionsLateralCheck")));
            yield return G("DimensionsPlantaCheck", Base, w => Alternar(Check(w, "DimensionsPlantaCheck")));
            yield return G("NameBox", Base, w => EditorWindowTestSupport.SetText(w, "NameBox", "Rack de prueba"));

            // Las cajas del frente y de la celda.
            yield return G("FondosBox=4", "pb-03-fondos", w => w.SelectMatrixCell(1, 0, false), w => Escribir(w, "FondosBox", 4));
            yield return G("DepthStartBox=2", "pb-03-fondos", w => Escribir(w, "DepthStartBox", 2));
            yield return G("PositionsBox=2", "pb-03-fondos", w => w.SelectMatrixCell(1, 0, false), w => Escribir(w, "PositionsBox", 2));
            yield return G("LevelsBox=2", "pb-03-fondos", w => w.SelectMatrixCell(1, 0, false), w => Escribir(w, "LevelsBox", 2));
            yield return G("FirstLevelHeightBox=5", Base, w => Escribir(w, "FirstLevelHeightBox", 5));
            yield return G("CellPalletFrontBox=40", Base, w => Escribir(w, "CellPalletFrontBox", 40));
            yield return G("CellPalletHeightBox=61.5", Base, w => Escribir(w, "CellPalletHeightBox", 61.5));
            yield return G("CellPalletWeightBox=900", Base, w => Escribir(w, "CellPalletWeightBox", 900));
            yield return G("CellClearBox=7", Base, w => Escribir(w, "CellClearBox", 7));
            yield return G("CellInOutBeamBox=otro", Base, w => Otro(Combo(w, "CellInOutBeamBox")));
            yield return G("CellInOutPeralteBox=otro", Base, w => Otro(Combo(w, "CellInOutPeralteBox")));
            yield return G("CellIntermediateBeamBox=otro", Base, w => Otro(Combo(w, "CellIntermediateBeamBox")));
            yield return G("CellIntermediatePeralteBox=otro", Base, w => Otro(Combo(w, "CellIntermediatePeralteBox")));
            yield return G("CellFondoOverrideBox=6", Base, w => Escribir(w, "CellFondoOverrideBox", 6));
            yield return G("CellDrawPalletCheck", Base, w => Check(w, "CellDrawPalletCheck").IsChecked = true);
            yield return G("CellBeamLengthOverrideBox=100", Base, w => Escribir(w, "CellBeamLengthOverrideBox", 100));
            yield return G("RearPeralteBox=4.5", Base, w => Combo(w, "RearPeralteBox").SelectedItem = 4.5);

            // La parte compuesta.
            yield return G("GapBox=60", Compuesto, w => Escribir(w, "GapBox", 60));
            yield return G("CentralSeparatorCheck", Compuesto, w => Check(w, "CentralSeparatorCheck").IsChecked = true);
            yield return G("StructureOverrideBox=6", Compuesto, w => Escribir(w, "StructureOverrideBox", 6), w => Click(w, "ApplyStructureButton"));
            yield return G(
                "CellTopologyBox+RunDirectionBox+TopologyScopeBox", Compuesto,
                w => Combo(w, "CellTopologyBox").SelectedIndex = 3,
                w => Combo(w, "RunDirectionBox").SelectedIndex = 1,
                w => Combo(w, "TopologyScopeBox").SelectedIndex = 4,
                w => Click(w, "ApplyTopologyButton"));
            yield return G("FrontCountBox=3", Compuesto, w => Escribir(w, "FrontCountBox", 3));
            yield return G("SideBPresentCheck", Base, w => Check(w, "SideBPresentCheck").IsChecked = true);
            yield return G("SideSelectorBox=1", Compuesto, w => Combo(w, "SideSelectorBox").SelectedIndex = 1);
            yield return G("FrontalSideBox=1", Compuesto, w => Combo(w, "FrontalSideBox").SelectedIndex = 1);

            // Las cajas avanzadas y los modulos.
            yield return G("RackHeaderHeightBox=155", "pb-06-seguridad", w => Check(w, "AdvancedModulesToggle").IsChecked = true, w => Escribir(w, "RackHeaderHeightBox", 155));
            yield return G("DerivedPostHeightBox=140", "pb-06-seguridad", w => Check(w, "AdvancedModulesToggle").IsChecked = true, w => Escribir(w, "DerivedPostHeightBox", 140));
            yield return G(
                "DerivedPostReinforcedCheck+DerivedPostReinforcementHeightBox=50", "pb-06-seguridad",
                w => Check(w, "AdvancedModulesToggle").IsChecked = true,
                w => Pulsar(Check(w, "DerivedPostReinforcedCheck")),
                w => Escribir(w, "DerivedPostReinforcementHeightBox", 50));
            yield return G("RackSeparatorCountBox=4", "pb-06-seguridad", w => Check(w, "AdvancedModulesToggle").IsChecked = true, w => Escribir(w, "RackSeparatorCountBox", 4));
            yield return G("RackSeparatorSpacingBox=20", "pb-06-seguridad", w => Check(w, "AdvancedModulesToggle").IsChecked = true, w => Escribir(w, "RackSeparatorSpacingBox", 20));
            yield return G(
                "ModuleLengthBox=+8", "pb-06-seguridad",
                w => SelectModule(w, "M2"),
                w => Escribir(w, "ModuleLengthBox", ValueOf(w, "ModuleLengthBox").Value + 8.0),
                w => Click(w, "ConfirmModuleButton"));
            yield return G(
                "DerivedPostLineHeightBox=137", Base,
                w => Check(w, "AdvancedModulesToggle").IsChecked = true,
                w => PushBackHeaderTestSupport.Target(w, Array.Empty<string>(), new[] { PushBackHeaderTestSupport.Lines(w)[1] }),
                w => Escribir(w, "DerivedPostLineHeightBox", 137),
                w => Click(w, "ApplyDerivedPostLinesButton"),
                w => Click(w, "ConfirmModuleButton"));

            // «Restaurar», tras editar una caja.
            yield return G("Restaurar", Base, w => Escribir(w, "DepthBox", 54), w => Click(w, "RestoreButton"));
        }

        /// <summary>
        /// Un gesto en su ventana de WORKSPACE nueva: la sonda, los pasos, «Aplicar» (el <c>UpdateButton</c>, que en WORKSPACE dice
        /// «Aplicar al borrador») y la fila. <c>aplicado</c> es que el borrador cambio de objeto; <c>diseno-igual</c> se toma tras
        /// «Aplicar», que recalcula antes de aplicar. Devuelve false si la sonda no da <c>Workspace</c>.
        /// </summary>
        private static bool Censo(List<string> log, PromptProbe avisos, Gesto gesto)
        {
            var estado = Paso(log, () => Estado(Fixture(gesto.Diseno)));
            if (estado == null) return false;
            var s = Nueva(log, estado);
            if (s == null) return false;
            var w = s.W;
            try
            {
                avisos.Reiniciar();
                if (!Sondeo(log, w)) return false;

                foreach (var paso in gesto.Pasos)
                {
                    var hacer = paso;
                    Paso(log, () => hacer(w));
                }

                var actual = s.Draft.CurrentDraftState;
                Paso(log, () => Click(w, "UpdateButton"));
                var aplicado = !ReferenceEquals(actual, s.Draft.CurrentDraftState);
                var iguales = Paso(log, () => Igual(s.Draft.CurrentDraftState, w.EditableStateForTest));
                var disenoIgual = Paso(log, () => DisenoDe(s.Draft.CurrentDraftState, PushBackSide.A) == DisenoDeVentana(w));
                var linea = gesto.Nombre + " aplicado=" + aplicado + " igual-a-la-ventana=" + iguales + " diseno-igual=" + disenoIgual;
                CerrarVentana(w);
                log.Add(linea + " descartes=" + avisos.Asked);
                return true;
            }
            finally { CerrarVentana(w); }
        }

        [Fact]
        public void CT_PBT2C_03_InWorkspaceNoControlIsTheSourceOfTruthOnTheWayToTheDraft()
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
                    foreach (var gesto in Gestos())
                    {
                        if (!Censo(log, avisos, gesto)) return;
                    }
                });
            }

            var esperadas = new List<string>
            {
                "DepthBox=54 aplicado=True igual-a-la-ventana=True diseno-igual=True descartes=0",
                "ToleranceBox=4 aplicado=False igual-a-la-ventana=True diseno-igual=True descartes=0",
                "PostPeralteBox=+0.5 aplicado=True igual-a-la-ventana=True diseno-igual=True descartes=0",
                "BeamDepthBox=+1 aplicado=True igual-a-la-ventana=True diseno-igual=True descartes=0",
                "AnnotationScaleBox=2 aplicado=True igual-a-la-ventana=True diseno-igual=True descartes=0",
                "WeightUnitBox=lb aplicado=True igual-a-la-ventana=True diseno-igual=True descartes=0",
                "PalletsDeepBox=9 aplicado=True igual-a-la-ventana=True diseno-igual=True descartes=0",
                "PostBox=otro aplicado=False igual-a-la-ventana=True diseno-igual=True descartes=0",
                "NumberFrontsCheck aplicado=True igual-a-la-ventana=True diseno-igual=True descartes=0",
                "NumberLevelsCheck aplicado=True igual-a-la-ventana=True diseno-igual=True descartes=0",
                "DrawRackNameCheck aplicado=True igual-a-la-ventana=True diseno-igual=True descartes=0",
                "DimensionsBox=3 aplicado=True igual-a-la-ventana=True diseno-igual=True descartes=0",
                "DimensionsFrontalCheck aplicado=True igual-a-la-ventana=True diseno-igual=True descartes=0",
                "DimensionsLateralCheck aplicado=True igual-a-la-ventana=True diseno-igual=True descartes=0",
                "DimensionsPlantaCheck aplicado=True igual-a-la-ventana=True diseno-igual=True descartes=0",
                "NameBox aplicado=True igual-a-la-ventana=True diseno-igual=True descartes=0",
                "FondosBox=4 aplicado=True igual-a-la-ventana=True diseno-igual=True descartes=0",
                "DepthStartBox=2 aplicado=True igual-a-la-ventana=True diseno-igual=True descartes=0",
                "PositionsBox=2 aplicado=True igual-a-la-ventana=True diseno-igual=True descartes=0",
                "LevelsBox=2 aplicado=True igual-a-la-ventana=True diseno-igual=True descartes=0",
                "FirstLevelHeightBox=5 aplicado=True igual-a-la-ventana=True diseno-igual=True descartes=0",
                "CellPalletFrontBox=40 aplicado=True igual-a-la-ventana=True diseno-igual=True descartes=0",
                "CellPalletHeightBox=61.5 aplicado=True igual-a-la-ventana=True diseno-igual=True descartes=0",
                "CellPalletWeightBox=900 aplicado=True igual-a-la-ventana=True diseno-igual=True descartes=0",
                "CellClearBox=7 aplicado=True igual-a-la-ventana=True diseno-igual=True descartes=0",
                "CellInOutBeamBox=otro aplicado=False igual-a-la-ventana=True diseno-igual=True descartes=0",
                "CellInOutPeralteBox=otro aplicado=False igual-a-la-ventana=True diseno-igual=True descartes=0",
                "CellIntermediateBeamBox=otro aplicado=True igual-a-la-ventana=True diseno-igual=True descartes=0",
                "CellIntermediatePeralteBox=otro aplicado=True igual-a-la-ventana=True diseno-igual=True descartes=0",
                "CellFondoOverrideBox=6 aplicado=True igual-a-la-ventana=True diseno-igual=True descartes=0",
                "CellDrawPalletCheck aplicado=True igual-a-la-ventana=True diseno-igual=True descartes=0",
                "CellBeamLengthOverrideBox=100 aplicado=True igual-a-la-ventana=True diseno-igual=True descartes=0",
                "RearPeralteBox=4.5 aplicado=True igual-a-la-ventana=True diseno-igual=True descartes=0",
                "GapBox=60 aplicado=True igual-a-la-ventana=True diseno-igual=True descartes=0",
                "CentralSeparatorCheck aplicado=True igual-a-la-ventana=True diseno-igual=True descartes=0",
                "StructureOverrideBox=6 aplicado=False igual-a-la-ventana=False diseno-igual=False descartes=1",
                "CellTopologyBox+RunDirectionBox+TopologyScopeBox aplicado=True igual-a-la-ventana=True diseno-igual=True descartes=0",
                "FrontCountBox=3 aplicado=True igual-a-la-ventana=True diseno-igual=True descartes=0",
                "SideBPresentCheck aplicado=True igual-a-la-ventana=True diseno-igual=True descartes=0",
                "SideSelectorBox=1 aplicado=False igual-a-la-ventana=True diseno-igual=True descartes=0",
                "FrontalSideBox=1 aplicado=False igual-a-la-ventana=True diseno-igual=True descartes=0",
                "RackHeaderHeightBox=155 aplicado=True igual-a-la-ventana=True diseno-igual=True descartes=0",
                "DerivedPostHeightBox=140 aplicado=True igual-a-la-ventana=True diseno-igual=True descartes=0",
                "DerivedPostReinforcedCheck+DerivedPostReinforcementHeightBox=50 aplicado=True igual-a-la-ventana=True diseno-igual=True descartes=0",
                "RackSeparatorCountBox=4 aplicado=True igual-a-la-ventana=True diseno-igual=True descartes=0",
                "RackSeparatorSpacingBox=20 aplicado=True igual-a-la-ventana=True diseno-igual=True descartes=0",
                "ModuleLengthBox=+8 aplicado=True igual-a-la-ventana=True diseno-igual=True descartes=0",
                "DerivedPostLineHeightBox=137 aplicado=True igual-a-la-ventana=True diseno-igual=True descartes=0",
                "Restaurar aplicado=True igual-a-la-ventana=True diseno-igual=True descartes=0",
            };

            Assert.Equal(esperadas, log);
        }
    }
}
