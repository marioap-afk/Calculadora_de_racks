using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Selective;
using RackCad.UI.Controls;
using RackCad.UI.Systems.Selective;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-48 gate G4F — la SESION real de RACKEDITAR con las dos propiedades vinculables a la vez.
    ///
    /// <para>
    /// Lo que solo se puede comprobar con la ventana de verdad: que los dos editores conviven con sus propios
    /// estados, que guardar sin tocar nada conserva LOS DOS literales congelados y LOS DOS vinculos, y que el
    /// protocolo C4 de dos fases no aplica una propiedad cuando la otra bloquea.
    /// </para>
    /// <para>
    /// Ese ultimo punto es el que la primera propiedad no podia probar: con un solo editor pendiente, «aplica
    /// todo» y «aplica el primero» son indistinguibles. Con dos, un Stage que falla en el segundo tiene que
    /// impedir que el primero se aplique — y eso se mide.
    /// </para>
    /// <para>
    /// Cada prueba CIERRA su ventana: la suite comparte un hilo STA, asi que una ventana viva —y mas si se
    /// quedo con el foco que C4 devuelve al campo ofensivo— es estado que hereda el test siguiente.
    /// </para>
    /// </summary>
    public class MultiPropertySessionTests
    {
        private const string RackId = "3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f44";
        private const string VarX = "11111111-1111-1111-1111-111111111111";
        private const string VarY = "22222222-2222-2222-2222-222222222222";

        private const string VcToken = ProjectPropertyIds.SelectiveVerticalClearanceToken;
        private const string PtToken = ProjectPropertyIds.SelectivePalletToleranceToken;

        private static PropertyId Vc => ProjectPropertyIds.SelectiveVerticalClearance;

        private static PropertyId Pt => ProjectPropertyIds.SelectivePalletTolerance;

        private static VariableId Id(string guid) => VariableId.Parse(guid);

        private static SelectivePalletDesign Diseno(double clearance = 6.0, double tolerance = 4.0)
        {
            var design = new SelectivePalletDesign
            {
                PostId = "POSTE_A",
                PostPeralte = 3.0,
                VerticalClearance = clearance,
                PalletTolerance = tolerance,
                PalletDepth = 48.0,
            };

            var bay = new SelectiveBayDesign();
            bay.Levels.Add(new SelectiveCell
            {
                Pallet = new Tarima { Frente = 48, Alto = 50 },
                PalletCount = 1,
                BeamId = "BEAM_A",
                BeamPeralte = 4.5,
            });
            design.Bays.Add(bay);
            return design;
        }

        private static SelectivePalletDesignDocument Authored(
            double clearance = 6.0, double tolerance = 4.0, string vcTo = null, string ptTo = null)
        {
            var doc = SelectivePalletDesignDocument.From(Diseno(clearance, tolerance), RackId, "Rack A");
            var bindings = new Dictionary<string, SelectivePropertyValueDocument>();

            if (vcTo != null)
            {
                bindings[VcToken] = SelectivePropertyValueDocument.ToProjectVariable(vcTo);
            }

            if (ptTo != null)
            {
                bindings[PtToken] = SelectivePropertyValueDocument.ToProjectVariable(ptTo);
            }

            if (bindings.Count > 0)
            {
                doc.PropertyValues = bindings;
                doc.SchemaVersion = SelectivePalletDesignDocument.PromotedSchemaVersion;
            }

            return doc;
        }

        private static ProjectVariableDocument Entry(string guid, string name, double value)
            => new ProjectVariableDocument
            {
                VariableId = guid,
                Name = name,
                Type = VariableType.Length.ToString(),
                Definition = new ProjectVariableDefinitionDocument { Kind = "literal", Value = value },
            };

        /// <summary>X = 10, Y = 9. Con <paramref name="homonimas"/>, las dos se llaman igual.</summary>
        private static ProjectVariablesReadResult Registro(bool homonimas = false)
        {
            var document = ProjectVariablesDocument.CreateNew();
            document.Variables = new List<ProjectVariableDocument>
            {
                Entry(VarX, homonimas ? "General" : "Holgura General", 10.0),
                Entry(VarY, homonimas ? "General" : "Tolerancia Estrecha", 9.0),
            };
            return ProjectVariablesReadResult.Readable(document);
        }

        private static RackSelectiveWindow Abrir(SelectivePalletDesignDocument authored, ProjectVariablesReadResult read)
        {
            var open = SelectiveEditorOpen.Resolve(authored, read);
            Assert.True(open.IsOpen);

            var options = LinkedPropertyOptions.ForProperty(Vc, read);
            Assert.True(options.IsUsable);

            var window = SelectiveWindowTestSupport.Open(canInsertInAutoCad: true);
            window.SetProjectVariables(options.Options);
            window.LoadExisting(authored, open.Design, open.LinkedPropertyStates);
            return window;
        }

        /// <summary>Abre, ejecuta y CIERRA. La higiene que el hilo STA compartido exige.</summary>
        private static T Con<T>(
            SelectivePalletDesignDocument authored, ProjectVariablesReadResult read,
            Func<RackSelectiveWindow, T> body)
            => StaTestRunner.Run(() =>
            {
                var window = Abrir(authored, read);

                try
                {
                    return body(window);
                }
                finally
                {
                    window.Close();
                }
            });

        private static T Control<T>(RackSelectiveWindow window, string name)
            where T : System.Windows.FrameworkElement
            => EditorWindowTestSupport.Find<T>(window, element => element.Name == name);

        private static LinkedPropertyEditor Clearance(RackSelectiveWindow window)
            => Control<LinkedPropertyEditor>(window, "ClearanceEditor");

        private static LinkedPropertyEditor Tolerance(RackSelectiveWindow window)
            => Control<LinkedPropertyEditor>(window, "ToleranceEditor");

        // ================================================================ L. abrir con las DOS vinculadas

        /// <summary>
        /// G4F (L). Las dos vinculadas a variables distintas: cada editor ensena SU referencia y conserva SU
        /// literal congelado, y el diseno del editor trabaja sobre los dos efectivos.
        /// </summary>
        [Fact]
        public void ABRIR_CON_LAS_DOS_VINCULADAS_ENSENA_CADA_REFERENCIA_CON_SU_CONGELADO()
        {
            var (textoVc, textoPt, congeladoVc, congeladoPt, efectivoVc, efectivoPt) =
                Con(Authored(clearance: 6.0, tolerance: 4.0, vcTo: VarX, ptTo: VarY), Registro(), window =>
                {
                    var vc = Clearance(window);
                    var pt = Tolerance(window);

                    vc.Session.TryGetEffectiveValue(out var efVc);
                    pt.Session.TryGetEffectiveValue(out var efPt);

                    return (vc.Box.Text, pt.Box.Text,
                        vc.FinalState.CommittedLiteral, pt.FinalState.CommittedLiteral, efVc, efPt);
                });

            Assert.Equal("=Holgura General", textoVc);
            Assert.Equal("=Tolerancia Estrecha", textoPt);

            // Cada congelado es el del AUTHORED, no el de su variable.
            Assert.Equal(6.0, congeladoVc);
            Assert.Equal(4.0, congeladoPt);

            // Y cada efectivo es el de SU variable.
            Assert.Equal(10.0, efectivoVc);
            Assert.Equal(9.0, efectivoPt);
        }

        /// <summary>
        /// G4F (L) — la version multi-propiedad de la prueba 28. Se abre con las dos vinculadas, no se toca
        /// NADA y se guarda: los DOS literales congelados y los DOS vinculos sobreviven, aunque el diseno que
        /// el dibujo consume lleve los efectivos.
        /// </summary>
        [Fact]
        public void GUARDAR_LAS_DOS_VINCULADAS_SIN_TOCAR_NADA_CONSERVA_AMBOS_CONGELADOS_Y_AMBOS_VINCULOS()
        {
            var authored = Authored(clearance: 6.0, tolerance: 4.0, vcTo: VarX, ptTo: VarY);
            var registro = Registro();

            var (salida, finales) = Con(authored, registro, window =>
            {
                EditorWindowTestSupport.ClickNamed(window, "UpdateButton");
                return (window.DesignToInsert, window.LinkedPropertyFinalStates);
            });

            Assert.NotNull(salida);

            // El dibujo refleja los EFECTIVOS.
            Assert.Equal(10.0, salida.VerticalClearance);
            Assert.Equal(9.0, salida.PalletTolerance);

            // Y el documento lo produce el reconciler.
            var reconciled = LinkedPropertyReconciler.Reconcile(
                authored, salida, finales, registro, RackId, "Rack A");

            Assert.True(reconciled.IsSuccess);

            Assert.Equal(6.0, reconciled.Authored.VerticalClearance);
            Assert.Equal(4.0, reconciled.Authored.PalletTolerance);

            Assert.True(reconciled.Authored.TryGetBinding(Vc, out var boundVc));
            Assert.True(reconciled.Authored.TryGetBinding(Pt, out var boundPt));
            Assert.Equal(Id(VarX), boundVc);
            Assert.Equal(Id(VarY), boundPt);

            Assert.Equal(10.0, reconciled.Effective.VerticalClearance);
            Assert.Equal(9.0, reconciled.Effective.PalletTolerance);
        }

        // ================================================================ M. transiciones cruzadas por la ventana

        /// <summary>
        /// G4F (M). En UNA sesion real: la holgura pasa de referencia a literal con Enter, y la tolerancia de
        /// literal a referencia por seleccion explicita. Ninguna pisa a la otra.
        /// </summary>
        [Fact]
        public void DOS_TRANSICIONES_OPUESTAS_EN_LA_MISMA_SESION_REAL()
        {
            var authored = Authored(clearance: 6.0, tolerance: 4.0, vcTo: VarX);
            var registro = Registro();

            var finales = Con(authored, registro, window =>
            {
                var vc = Clearance(window);
                var pt = Tolerance(window);

                // VC: Reference(X) -> Literal(7), que exige Enter explicito.
                vc.Box.Text = "7";
                Assert.True(vc.Session.TryCommitByEnter(out _));

                // PT: Literal(4) -> Reference(Y), que exige seleccion explicita.
                pt.Box.Text = "=Tolerancia";
                Assert.True(pt.Session.TrySelect(Id(VarY), out _));

                return window.LinkedPropertyFinalStates;
            });

            var reconciled = LinkedPropertyReconciler.Reconcile(
                authored, Diseno(clearance: 7.0, tolerance: 9.0), finales, registro, RackId, "Rack A");

            Assert.True(reconciled.IsSuccess);

            // VC: sin vinculo, literal 7.
            Assert.False(reconciled.Authored.TryGetBinding(Vc, out _));
            Assert.Equal(7.0, reconciled.Authored.VerticalClearance);
            Assert.Equal(7.0, reconciled.Effective.VerticalClearance);

            // PT: vinculada a Y, congelado 4, efectivo 9.
            Assert.True(reconciled.Authored.TryGetBinding(Pt, out var bound));
            Assert.Equal(Id(VarY), bound);
            Assert.Equal(4.0, reconciled.Authored.PalletTolerance);
            Assert.Equal(9.0, reconciled.Effective.PalletTolerance);
        }

        // ================================================================ N. C4 con DOS cambios de fuente

        /// <summary>
        /// G4F (N), caso 1. Las DOS con un borrador que cambia la fuente: la frontera bloquea, se evaluan las
        /// dos —el mensaje nombra a ambas— y NINGUNA queda comprometida ni a medias.
        /// </summary>
        [Fact]
        public void DOS_BORRADORES_QUE_CAMBIAN_LA_FUENTE_BLOQUEAN_Y_NO_APLICAN_NINGUNA()
        {
            var (pidioInsertar, vcSigue, ptSigue, status, foco) =
                Con(Authored(clearance: 6.0, tolerance: 4.0, vcTo: VarX, ptTo: VarY), Registro(), window =>
                {
                    var vc = Clearance(window);
                    var pt = Tolerance(window);

                    vc.Box.Text = "7";    // Reference(X) -> DraftLiteral
                    pt.Box.Text = "5";    // Reference(Y) -> DraftLiteral

                    EditorWindowTestSupport.ClickNamed(window, "UpdateButton");

                    return (window.InsertRequested,
                        vc.FinalState.IsReference, pt.FinalState.IsReference,
                        Control<TextBlock>(window, "StatusText").Text,
                        vc.HasFocus || pt.HasFocus);
                });

            Assert.False(pidioInsertar);

            // Las DOS siguen vinculadas: ni una se convirtio en silencio.
            Assert.True(vcSigue);
            Assert.True(ptSigue);

            // La fase 1 evaluo AMBAS, asi que el mensaje nombra a las dos.
            Assert.Contains("Holgura vertical", status);
            Assert.Contains("Tolerancia horizontal", status);

            // Y el usuario acaba dentro de un campo que puede arreglar.
            Assert.True(foco);
        }

        /// <summary>
        /// G4F (N), caso 2 — el que la primera propiedad no podia probar. Una lista para aplicar y la otra
        /// bloqueando: la que estaba lista NO se aplica, porque la fase 2 solo ocurre si TODAS pasaron la fase 1.
        ///
        /// <para>
        /// Mata «C4 aplica el primer editor antes de que falle el segundo».
        /// </para>
        /// </summary>
        [Fact]
        public void UNA_LISTA_Y_OTRA_BLOQUEANDO_NO_APLICA_LA_QUE_ESTABA_LISTA()
        {
            var (pidioInsertar, vcLiteral, ptSigueVinculada) =
                Con(Authored(clearance: 6.0, tolerance: 4.0, ptTo: VarY), Registro(), window =>
                {
                    var vc = Clearance(window);
                    var pt = Tolerance(window);

                    vc.Box.Text = "8";    // Literal -> Literal: CONSERVA la fuente, estaria lista
                    pt.Box.Text = "5";    // Reference(Y) -> DraftLiteral: CAMBIA la fuente, bloquea

                    EditorWindowTestSupport.ClickNamed(window, "UpdateButton");

                    return (window.InsertRequested,
                        vc.FinalState.CommittedLiteral,
                        pt.FinalState.IsReference);
                });

            Assert.False(pidioInsertar);

            // La que estaba lista NO se aplico: sigue en su 6 comprometido, no en el 8 tecleado.
            Assert.Equal(6.0, vcLiteral);

            // Y la otra sigue vinculada.
            Assert.True(ptSigueVinculada);
        }

        /// <summary>
        /// G4F (N), caso 2 (continuacion). Resuelta la que bloqueaba, la frontera aplica las DOS en la fase 2.
        /// </summary>
        [Fact]
        public void RESUELTA_LA_QUE_BLOQUEABA_SE_APLICAN_LAS_DOS()
        {
            var (vcLiteral, ptLiteral, sucios) =
                Con(Authored(clearance: 6.0, tolerance: 4.0, ptTo: VarY), Registro(), window =>
                {
                    var vc = Clearance(window);
                    var pt = Tolerance(window);

                    vc.Box.Text = "8";
                    pt.Box.Text = "5";

                    // Primera frontera: bloquea.
                    EditorWindowTestSupport.ClickNamed(window, "UpdateButton");

                    // El usuario resuelve la ofensiva con Enter explicito.
                    Assert.True(pt.Session.TryCommitByEnter(out _));

                    // Segunda frontera: ahora si.
                    EditorWindowTestSupport.ClickNamed(window, "UpdateButton");

                    return (vc.FinalState.CommittedLiteral, pt.FinalState.CommittedLiteral,
                        vc.IsDirty || pt.IsDirty);
                });

            Assert.Equal(8.0, vcLiteral);
            Assert.Equal(5.0, ptLiteral);
            Assert.False(sucios);
        }

        // ================================================================ O. homonimos con dos editores

        /// <summary>
        /// G4F (O). Dos variables que se llaman IGUAL, una en cada propiedad. La seleccion viaja por identidad,
        /// asi que cada editor se queda con la suya y el round-trip lo conserva.
        /// </summary>
        [Fact]
        public void DOS_HOMONIMAS_UNA_EN_CADA_PROPIEDAD_NO_SE_CONFUNDEN()
        {
            var authored = Authored(clearance: 6.0, tolerance: 4.0);
            var registro = Registro(homonimas: true);

            var finales = Con(authored, registro, window =>
            {
                var vc = Clearance(window);
                var pt = Tolerance(window);

                // Las dos opciones se llaman "General": solo el fragmento de identidad las distingue.
                Assert.Equal(2, vc.Session.Candidates.Count);
                Assert.All(vc.Session.Candidates, o => Assert.Equal("General", o.Name));
                Assert.NotEqual(vc.Session.Candidates[0].DisplayText, vc.Session.Candidates[1].DisplayText);

                Assert.True(vc.Session.TrySelect(Id(VarX), out _));
                Assert.True(pt.Session.TrySelect(Id(VarY), out _));

                return window.LinkedPropertyFinalStates;
            });

            Assert.Equal(Id(VarX), finales[Vc].Source.VariableId);
            Assert.Equal(Id(VarY), finales[Pt].Source.VariableId);

            // Y tras reconciliar + round-trip REAL siguen apuntando a lo suyo.
            var reconciled = LinkedPropertyReconciler.Reconcile(
                authored, Diseno(clearance: 10.0, tolerance: 9.0), finales, registro, RackId, "Rack A");

            Assert.True(reconciled.IsSuccess);

            var store = new SelectivePalletDesignStore();
            var releido = store.Deserialize(store.Serialize(reconciled.Authored));

            Assert.True(releido.TryGetBinding(Vc, out var boundVc));
            Assert.True(releido.TryGetBinding(Pt, out var boundPt));
            Assert.Equal(Id(VarX), boundVc);
            Assert.Equal(Id(VarY), boundPt);

            // Y cada una resuelve a SU valor, aunque compartan nombre.
            Assert.Equal(10.0, reconciled.Effective.VerticalClearance);
            Assert.Equal(9.0, reconciled.Effective.PalletTolerance);
        }

        // ================================================================ J. legacy por la ventana real

        /// <summary>
        /// G4F (J). Un rack sin vinculos abre y se guarda sin ganarlos: los dos editores nacen literales y
        /// <c>PropertyValues</c> sigue vacio.
        /// </summary>
        [Fact]
        public void UN_RACK_SIN_VINCULOS_ABRE_Y_SE_GUARDA_SIN_GANAR_NINGUNO()
        {
            var authored = Authored(clearance: 6.0, tolerance: 4.0);
            var registro = Registro();

            Assert.Null(authored.PropertyValues);

            var (textoVc, textoPt, finales) = Con(authored, registro, window =>
                (Clearance(window).Box.Text, Tolerance(window).Box.Text, window.LinkedPropertyFinalStates));

            Assert.Equal("6", textoVc);
            Assert.Equal("4", textoPt);
            Assert.All(finales.Values, state => Assert.False(state.IsReference));

            var reconciled = LinkedPropertyReconciler.Reconcile(
                authored, Diseno(), finales, registro, RackId, "Rack A");

            Assert.True(reconciled.IsSuccess);
            Assert.False(reconciled.Authored.TryGetBinding(Vc, out _));
            Assert.False(reconciled.Authored.TryGetBinding(Pt, out _));

            // Y el documento original no se muto por haberlo abierto.
            Assert.Null(authored.PropertyValues);
        }
    }
}
