using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using RackCad.Application.Persistence;
using RackCad.UI.Editor;
using RackCad.UI.Systems.PushBack;
using Xunit;
using static RackCad.UI.Tests.PushBackFullEditorCharacterizationSupport;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 F4B PB-T0 — CARACTERIZACION del estado editable de la ventana clasica de Push Back frente al sobre que la abrio
    /// (CT-PB0-01), de su segunda pasada al pulsar «Actualizar» (CT-PB0-02) y de «renombrar» (CT-PB0-11).
    ///
    /// <para><b>Que se compara.</b> <c>Salida</c> es lo que la ventana ESCRIBIRIA (su <c>LastComputation.Design</c>
    /// persistido) y <c>Sobre</c> es el diseño interior DESERIALIZADO OTRA VEZ desde el texto del sobre, nunca el objeto que
    /// recibio la ventana. Las lineas de clase (a) (redondeo a «0.###» de los controles) y (b) (perdida del estilo de cotas)
    /// se RETIRAN antes de comparar: solo las fijan CT-PB0-18 a CT-PB0-20 (D-PBT0-03), y aqui se listan las retiradas.
    /// Cada linea que queda lleva su clase (c), (d), CARGA-APPLICATION u OTRA con el <c>archivo:linea</c> del producto que la
    /// causa, medido en la base.</para>
    ///
    /// <para>Fija lo que HAY y no corrige nada. Ninguna ventana se muestra y ninguna prueba ejecuta Insertar, «Guardar en
    /// biblioteca», el BOM ni el configurador real: «Actualizar» no abre ningun dialogo ni escribe (H-08), y la peticion se
    /// LEE del contrato publico de la ventana.</para>
    /// </summary>
    [Collection(EditorDiscardPromptCollection.Name)]
    public sealed class PushBackFullEditorCharacterizationStateTests
    {
        private const string Grid = "PushBackMatrixGrid";

        /// <summary>X-01: pb-21-cola con B declarado y la ranura 0 de B activa, reabierto desde su LastComputation.Design con el mismo id y nombre.</summary>
        private sealed class X01
        {
            public string Sobre;
            public string Salida;
            public RackPushBackSystemWindow SecondWindow;
        }


        /// <summary>
        /// La clase de cada contenedor que queda tras el filtro, con el archivo:linea del producto que lo causa (INV-PBT0-04).
        /// Abreviaturas: PBCEA = src/RackCad.Application/Systems/PushBack/PushBackCompositeEditorAssembler.cs; CS = PushBackCompositeEditorState.cs
        /// (misma carpeta); DA = PushBackEditorDesignAssembler.cs (misma carpeta); WC = src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.Composite.cs.
        /// El unico caso del corpus es pb-21-cola: un rack de UN sentido que conserva en el documento la cola del lado B.
        /// </summary>
        private static string Label(string group)
        {
            if (group.StartsWith("$.PushBack.Structure.Modules[", StringComparison.Ordinal))
            {
                // La cola (GAP y B:*) se aparca al cargar y no viaja a un diseño de un solo sentido.
                return "CARGA-APPLICATION:cola-aparcada (PBCEA:53-64; DA:279-287; WC:838-842)";
            }

            if (group == "$.PushBack.SideB")
            {
                // BuildSideB devuelve null mientras el lado B no esta declarado.
                return "CARGA-APPLICATION:lado-B-no-declarado (CS:1026-1029; PBCEA:52-65)";
            }

            if (group == "$.PushBack.Composite.AbsentSlotsB[0]")
            {
                // La ventana siempre declara las ranuras ausentes de B; el documento antiguo trae null.
                return "OTRA:ranuras-ausentes-de-B-siempre-declaradas (PBCEA:81-87)";
            }

            return "SIN-ETIQUETA";
        }

        /// <summary>Las lineas descriptivas de un diseño: composicion, lineas retiradas (a)/(b) y delta filtrado por contenedor, con su clase.</summary>
        private static List<string> Describe(string name, string sobre, string salida)
        {
            var lines = new List<string>
            {
                name + " salida-compuesto=" + IsComposite(salida) + " sobre-compuesto=" + IsComposite(sobre),
                name + " lineas-retiradas(a/b)=[" + string.Join(" ;; ", Removed(sobre, salida)) + "]",
            };
            var filtered = Filtered(sobre, salida);
            foreach (var group in filtered.GroupBy(Group))
            {
                lines.Add(name + " " + group.Key + " x" + group.Count() + " => " + Label(group.Key));
            }

            lines.Add(name + " delta-filtrado=" + filtered.Count + " sha256=" + Sha(filtered));
            return lines;
        }

        private static X01 BuildX01()
        {
            var result = new X01();
            string id;
            string name;
            RackCad.Domain.Systems.PushBack.PushBackDesign design;
            var w1 = Abrir(Fixture("pb-21-cola"));
            try
            {
                Check(w1, "SideBPresentCheck").IsChecked = true;
                Combo(w1, "SideSelectorBox").SelectedIndex = 1;
                EditorWindowTestSupport.SetFrontBlank(w1, Grid, 0, false);
                design = w1.LastComputation.Design;
                id = w1.RackId;
                name = w1.RackName;
                result.Sobre = Persist(new RackProjectStore().Deserialize(Persist(design)).PushBackDesign);
            }
            finally { Cerrar(w1); }

            result.SecondWindow = Ventana();
            result.SecondWindow.LoadExisting(design, id, name);
            result.Salida = Salida(result.SecondWindow);
            return result;
        }

        [Fact]
        public void CT_PB0_01_TheWindowStateAfterLoadingDiffersFromTheEnvelopeOnlyInTheCharacterizedWays()
        {
            var log = new List<string>();
            Sta(() =>
            {
                foreach (var fixture in new[] { "pb-20-ab", "pb-21-cola", "pb-31-v2-modulos" })
                {
                    var text = Fixture(fixture);
                    var w = Abrir(text);
                    try
                    {
                        log.AddRange(Describe(fixture, Sobre(text), Salida(w)));
                    }
                    finally { Cerrar(w); }
                }

                var x01 = BuildX01();
                try
                {
                    log.AddRange(Describe("X-01", x01.Sobre, x01.Salida));
                }
                finally { Cerrar(x01.SecondWindow); }
            });

            Assert.Equal(
                new[]
                {
                    "pb-20-ab salida-compuesto=True sobre-compuesto=True",
                    "pb-20-ab lineas-retiradas(a/b)=[]",
                    "pb-20-ab delta-filtrado=0 sha256=e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855",
                    "pb-21-cola salida-compuesto=False sobre-compuesto=False",
                    "pb-21-cola lineas-retiradas(a/b)=[]",
                    "pb-21-cola $.PushBack.Composite.AbsentSlotsB[0] x1 => OTRA:ranuras-ausentes-de-B-siempre-declaradas (PBCEA:81-87)",
                    "pb-21-cola $.PushBack.SideB x10 => CARGA-APPLICATION:lado-B-no-declarado (CS:1026-1029; PBCEA:52-65)",
                    "pb-21-cola $.PushBack.Structure.Modules[10] x8 => CARGA-APPLICATION:cola-aparcada (PBCEA:53-64; DA:279-287; WC:838-842)",
                    "pb-21-cola $.PushBack.Structure.Modules[11] x154 => CARGA-APPLICATION:cola-aparcada (PBCEA:53-64; DA:279-287; WC:838-842)",
                    "pb-21-cola $.PushBack.Structure.Modules[12] x8 => CARGA-APPLICATION:cola-aparcada (PBCEA:53-64; DA:279-287; WC:838-842)",
                    "pb-21-cola $.PushBack.Structure.Modules[13] x8 => CARGA-APPLICATION:cola-aparcada (PBCEA:53-64; DA:279-287; WC:838-842)",
                    "pb-21-cola $.PushBack.Structure.Modules[14] x154 => CARGA-APPLICATION:cola-aparcada (PBCEA:53-64; DA:279-287; WC:838-842)",
                    "pb-21-cola $.PushBack.Structure.Modules[15] x8 => CARGA-APPLICATION:cola-aparcada (PBCEA:53-64; DA:279-287; WC:838-842)",
                    "pb-21-cola $.PushBack.Structure.Modules[16] x154 => CARGA-APPLICATION:cola-aparcada (PBCEA:53-64; DA:279-287; WC:838-842)",
                    "pb-21-cola $.PushBack.Structure.Modules[8] x8 => CARGA-APPLICATION:cola-aparcada (PBCEA:53-64; DA:279-287; WC:838-842)",
                    "pb-21-cola $.PushBack.Structure.Modules[9] x154 => CARGA-APPLICATION:cola-aparcada (PBCEA:53-64; DA:279-287; WC:838-842)",
                    "pb-21-cola delta-filtrado=667 sha256=0b54e8ff5c8ad08d262bdfd8e30db37eb0dd40b19797940f8a5fd85b04eed875",
                    "pb-31-v2-modulos salida-compuesto=False sobre-compuesto=False",
                    "pb-31-v2-modulos lineas-retiradas(a/b)=[]",
                    "pb-31-v2-modulos delta-filtrado=0 sha256=e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855",
                    "X-01 salida-compuesto=True sobre-compuesto=True",
                    "X-01 lineas-retiradas(a/b)=[]",
                    "X-01 delta-filtrado=0 sha256=e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855",
                },
                log);
        }

        /// <summary>La segunda pasada de una ventana ya abierta: habilitacion del boton, resultado y, con peticion, su contenido.</summary>
        private static List<string> SecondPass(string name, RackPushBackSystemWindow w)
        {
            var s1 = Salida(w);
            var enabled = Btn(w, "UpdateButton").IsEnabled;
            var lines = new List<string>();
            if (!enabled)
            {
                lines.Add(name + " habilitado=False DESHABILITADO: " + StatusText(w));
                return lines;
            }

            Click(w, "UpdateButton");
            if (!w.InsertRequested)
            {
                lines.Add(name + " habilitado=True " + (w.OutputIsBlockedForTest ? "BLOQUEADO: " : "INVÁLIDO: ") + StatusText(w));
                return lines;
            }

            var request = Assert.IsType<PushBackInsertionRequest>(w.InsertionRequest);
            var delta = s1.StartsWith("NO-VÁLIDA", StringComparison.Ordinal) ? new List<string> { s1 } : Filtered(s1, Persist(w.DesignToInsert));
            lines.Add(name + " habilitado=True PETICIÓN updateOnly=" + w.UpdateOnly + " vistas=" + request.Views.Count + " delta=[" + string.Join(" ;; ", delta) + "]");
            return lines;
        }

        [Fact]
        public void CT_PB0_02_TheSecondPassOfUpdateEqualsTheFirstPassOutputForEveryDesignOfTheCorpusAndX01()
        {
            var log = new List<string>();
            Sta(() =>
            {
                foreach (var fixture in CorpusNames())
                {
                    var w = Abrir(Fixture(fixture));
                    try
                    {
                        log.AddRange(SecondPass(fixture, w));
                    }
                    finally { Cerrar(w); }
                }

                var x01 = BuildX01();
                try
                {
                    log.AddRange(SecondPass("X-01", x01.SecondWindow));
                }
                finally { Cerrar(x01.SecondWindow); }
            });

            Assert.Equal(
                new[]
                {
                    "pb-01-base habilitado=True PETICIÓN updateOnly=True vistas=0 delta=[]",
                    "pb-02-modulos habilitado=True PETICIÓN updateOnly=True vistas=0 delta=[]",
                    "pb-03-fondos habilitado=True PETICIÓN updateOnly=True vistas=0 delta=[]",
                    "pb-04-blanco habilitado=True PETICIÓN updateOnly=True vistas=0 delta=[]",
                    "pb-05-topes habilitado=True PETICIÓN updateOnly=True vistas=0 delta=[]",
                    "pb-06-seguridad habilitado=True PETICIÓN updateOnly=True vistas=0 delta=[]",
                    "pb-07-combinado habilitado=True PETICIÓN updateOnly=True vistas=0 delta=[]",
                    "pb-08-redondeo habilitado=True PETICIÓN updateOnly=True vistas=0 delta=[]",
                    "pb-10-sin-datum habilitado=True PETICIÓN updateOnly=True vistas=0 delta=[]",
                    "pb-11-sin-i41 habilitado=True PETICIÓN updateOnly=True vistas=0 delta=[]",
                    "pb-12-poste-nulo habilitado=True PETICIÓN updateOnly=True vistas=0 delta=[]",
                    "pb-13-sideb-no-presente habilitado=True PETICIÓN updateOnly=True vistas=0 delta=[]",
                    "pb-14-peralte-heredado habilitado=True PETICIÓN updateOnly=True vistas=0 delta=[]",
                    "pb-15-sin-composite habilitado=True PETICIÓN updateOnly=True vistas=0 delta=[]",
                    "pb-20-ab habilitado=True PETICIÓN updateOnly=True vistas=0 delta=[]",
                    "pb-21-cola habilitado=True PETICIÓN updateOnly=True vistas=0 delta=[]",
                    "pb-31-v2-modulos habilitado=True PETICIÓN updateOnly=True vistas=0 delta=[]",
                    "X-01 habilitado=True PETICIÓN updateOnly=True vistas=0 delta=[]",
                },
                log);
        }

        [Fact]
        public void CT_PB0_11_RenamingTrimsTheNameAndAnEmptyOrBlankNameIsKeptAsItIs()
        {
            var log = new List<string>();
            Sta(() =>
            {
                var id = IdOf(Fixture("pb-01-base"));
                foreach (var typed in new[] { "  Renombrado PB0  ", string.Empty, "   " })
                {
                    var w = Abrir(Fixture("pb-01-base"));
                    try
                    {
                        EditorWindowTestSupport.SetText(w, "NameBox", typed);
                        Click(w, "UpdateButton");
                        var request = w.InsertionRequest as PushBackInsertionRequest;
                        log.Add("[" + typed + "] insertRequested=" + w.InsertRequested + " RackName=[" + w.RackName + "] request.RackName=["
                            + (request == null ? "<sin peticion>" : request.RackName) + "] RackId-igual-al-del-sobre=" + (w.RackId == id)
                            + " SystemToInsert.Name=[" + (w.SystemToInsert == null ? "<sin sistema>" : w.SystemToInsert.Name) + "]");
                    }
                    finally { Cerrar(w); }
                }
            });

            Assert.Equal(
                new[]
                {
                    "[  Renombrado PB0  ] insertRequested=True RackName=[Renombrado PB0] request.RackName=[Renombrado PB0] RackId-igual-al-del-sobre=True SystemToInsert.Name=[]",
                    "[] insertRequested=True RackName=[] request.RackName=[] RackId-igual-al-del-sobre=True SystemToInsert.Name=[]",
                    "[   ] insertRequested=True RackName=[] request.RackName=[] RackId-igual-al-del-sobre=True SystemToInsert.Name=[]",
                },
                log);
        }
    }
}
