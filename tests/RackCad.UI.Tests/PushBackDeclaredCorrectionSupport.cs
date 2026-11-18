using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Controls;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.PushBack.Pilot;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Shared;
using RackCad.UI.Systems.PushBack;
using Xunit;
using static RackCad.UI.Tests.PushBackFullEditorCharacterizationSupport;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 F4B PB-TC — los ayudantes comunes de las pruebas de las correcciones declaradas de la ventana CLASICA de Push Back
    /// (paquete del contrato, §4.1). Solo ejecucion: usa los ayudantes de PB-T0 (<see cref="PushBackFullEditorCharacterizationSupport"/>),
    /// que no se editan, y las costuras internas que la ventana ya declara. No contiene pruebas.
    ///
    /// <para><b>Ventanas.</b> Toda ventana se crea con <see cref="PushBackFullEditorCharacterizationSupport.Ventana"/> y su carga va
    /// DENTRO del <c>try</c>, con <see cref="PushBackFullEditorCharacterizationSupport.Cerrar"/> en el <c>finally</c>, tambien si la
    /// carga lanza (D-PBTC-08). Nunca <c>Abrir</c> ni <c>AbrirFueraDePantalla</c>; ninguna ventana se muestra; sin teclado ni foco
    /// real (D-PBTC-09).</para>
    ///
    /// <para><b>Sin escrituras.</b> Ninguna prueba ejecuta Insertar, «Guardar en biblioteca», el BOM ni el configurador real, ni
    /// abre un <c>MessageBox</c>: la salida se LEE de <c>LastComputation.Design</c> o de la peticion de «Actualizar».</para>
    /// </summary>
    internal static class PushBackDeclaredCorrectionSupport
    {
        private const double Increment = 0.00044;

        /// <summary>Una fila F de CT-PB0-19: sobre <c>pb-01-base</c>, el valor exacto es la base mas 0,00044.</summary>
        internal sealed class FilaF
        {
            public string Name;
            public string Path;
            public double Base;

            public double Exacto => Base + Increment;
        }

        /// <summary>Las once filas F1 a F11 (paquete §4.1), copiadas de CT-PB0-19 sin referenciarla.</summary>
        internal static IReadOnlyList<FilaF> FilasF() => new[]
        {
            new FilaF { Name = "F1", Path = "Structure.PalletDepth", Base = 48 },
            new FilaF { Name = "F2", Path = "Structure.PostPeralte", Base = 3 },
            new FilaF { Name = "F3", Path = "Structure.PalletTolerance", Base = 4 },
            new FilaF { Name = "F4", Path = "Structure.BeamDepth", Base = 6 },
            new FilaF { Name = "F5", Path = "Structure.AnnotationScale", Base = 1.5 },
            new FilaF { Name = "F6", Path = "Structure.Fronts[0].FirstLevelHeight", Base = 4 },
            new FilaF { Name = "F7", Path = "Structure.Fronts[0].Levels[0].PalletFront", Base = 42 },
            new FilaF { Name = "F8", Path = "Structure.Fronts[0].Levels[0].PalletHeight", Base = 60 },
            new FilaF { Name = "F9", Path = "Structure.Fronts[0].Levels[0].PalletWeight", Base = 1000 },
            new FilaF { Name = "F10", Path = "Structure.Fronts[0].Levels[0].ClearHeight", Base = 6 },
            new FilaF { Name = "F11", Path = "Structure.Fronts[0].Levels[0].BeamLengthOverride", Base = 100 },
        };

        /// <summary>El sobre de <c>pb-01-base</c> con el campo de la fila puesto a su valor exacto.</summary>
        internal static string VarianteDe(FilaF fila, string fixture = "pb-01-base")
            => EditInner(Fixture(fixture), inner => Poner(inner, fila.Path, fila.Exacto));

        // ---- Cargas ----------------------------------------------------------------------------------------------

        /// <summary>Cargar(w, t): deserializa el sobre y llama a LoadExisting (RACKEDITAR). Va DENTRO del try de la prueba.</summary>
        internal static void Cargar(RackPushBackSystemWindow w, string t)
        {
            var embed = new RackEmbedStore().Deserialize(t);
            var project = new RackProjectStore().Deserialize(embed.Design);
            w.LoadExisting(project.PushBackDesign, embed.Id, embed.Name, project);
        }

        /// <summary>CargarBiblioteca(w, t): lo mismo con LoadDesignForNew (la biblioteca). Va DENTRO del try de la prueba.</summary>
        internal static void CargarBiblioteca(RackPushBackSystemWindow w, string t)
        {
            var embed = new RackEmbedStore().Deserialize(t);
            var project = new RackProjectStore().Deserialize(embed.Design);
            w.LoadDesignForNew(project.PushBackDesign, embed.Name, project);
        }

        // ---- Numeros del JSON persistido -------------------------------------------------------------------------

        /// <summary>El numero escrito con «R» y la cultura invariante.</summary>
        internal static string Texto(double value) => value.ToString("R", CultureInfo.InvariantCulture);

        private static JsonNode Navegar(JsonNode root, string path)
        {
            var node = root;
            foreach (var segment in path.Split('.'))
            {
                var bracket = segment.IndexOf('[');
                var name = bracket < 0 ? segment : segment.Substring(0, bracket);
                node = node?[name];
                while (bracket >= 0 && node != null)
                {
                    var close = segment.IndexOf(']', bracket);
                    node = node[int.Parse(segment.Substring(bracket + 1, close - bracket - 1), CultureInfo.InvariantCulture)];
                    bracket = segment.IndexOf('[', close);
                }
            }

            return node;
        }

        /// <summary>Num(json, ruta): el numero en esa ruta (bajo <c>PushBack</c>) del JSON persistido, con «R», o <c>&lt;null&gt;</c>.</summary>
        internal static string Num(string json, string ruta)
        {
            var node = Navegar(JsonNode.Parse(json)["PushBack"], ruta);
            return node == null ? "<null>" : Texto(node.GetValue<double>());
        }

        /// <summary>Pone <paramref name="value"/> en la ruta (bajo <c>PushBack</c>) del JSON interior de un sobre.</summary>
        internal static void Poner(JsonNode inner, string path, double value)
        {
            var cut = path.LastIndexOf('.');
            var parent = Navegar(inner["PushBack"], path.Substring(0, cut));
            parent[path.Substring(cut + 1)] = value;
        }

        // ---- El piloto y la paridad ------------------------------------------------------------------------------

        /// <summary>PilotoEscribe(t, catalog): lo que el piloto escribe, copia de WLC:84-91 sin referenciarla.</summary>
        internal static string PilotoEscribe(string t, RackCatalog catalog)
        {
            var materialized = PushBackPilotMaterializer.Materialize(t, catalog);
            Assert.Equal(PushBackPilotMaterializationKind.Admitted, materialized.Kind);
            var composition = PushBackPilotMaterializer.ComposeForWrite(materialized.State, materialized.State, catalog);
            Assert.True(composition.IsComposed, composition.Reason);
            return Persist(new PushBackCompositeEditorAssembler(catalog).BuildDesign(composition.Editor, composition.Inputs));
        }

        /// <summary>
        /// La linea <c>&lt;nombre&gt; admitida=&lt;b&gt; delta=[…]</c> de una fila: la ventana carga el sobre, y su diseño se
        /// compara con lo que escribe el piloto. La ventana se crea, se carga y se cierra aqui, con la carga dentro del <c>try</c>.
        /// </summary>
        internal static string Paridad(string nombre, string t)
        {
            var w = Ventana();
            try
            {
                Cargar(w, t);
                var admitida = w.CurrentInputsAreValid && w.LastComputation != null && w.LastComputation.IsValid;
                var delta = admitida
                    ? Delta(Persist(w.LastComputation.Design), PilotoEscribe(t, w.Session.Catalog))
                    : new List<string> { "NO-ADMITIDA: " + StatusText(w) };
                return nombre + " admitida=" + admitida + " delta=[" + string.Join(" ;; ", delta) + "]";
            }
            finally { Cerrar(w); }
        }

        // ---- El estilo de cotas ----------------------------------------------------------------------------------

        /// <summary>EstiloPb(): copia de <c>PushBackDimensionViewsWindowTests.cs:65-92</c> con política nula y estilo «ESTILO_PB».</summary>
        internal static PushBackDesign EstiloPb()
        {
            var design = new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                    PalletsDeep = 6,
                    LoadLevels = 2,
                    FirstLevelHeight = 6.0,
                    BeamDepth = 4.0,
                    Dimensions = DimensionDetail.Standard,
                    DimensionStyle = "ESTILO_PB",
                    DimensionViews = null
                }
            };
            design.Structure.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 2, PalletsDeep = 6, DepthStartPosition = 1 });
            design.Structure.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 2, PalletsDeep = 3, DepthStartPosition = 4 });
            var f0 = new PushBackFrontConfig();
            f0.HighEndBeamPeraltes.Add(5.0);
            f0.HighEndBeamPeraltes.Add(4.0);
            design.Fronts.Add(f0);
            var f1 = new PushBackFrontConfig();
            f1.HighEndBeamPeraltes.Add(4.5);
            f1.HighEndBeamPeraltes.Add(4.5);
            design.Fronts.Add(f1);
            return design;
        }

        // ---- El aviso de D-PB-08 ---------------------------------------------------------------------------------

        /// <summary>Aviso(w): el TextBlock del aviso. Es la primera sentencia que toca el aviso.</summary>
        internal static TextBlock Aviso(Window w) => Assert.IsType<TextBlock>(w.FindName("SideBEmptyNoticeText"));
    }
}
