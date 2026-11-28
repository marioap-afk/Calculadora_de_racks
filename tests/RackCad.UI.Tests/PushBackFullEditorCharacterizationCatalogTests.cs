using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json.Nodes;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.Dynamic;
using RackCad.UI.Systems.PushBack;
using Xunit;
using static RackCad.UI.Tests.PushBackFullEditorCharacterizationSupport;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 F4B PB-T0 — CARACTERIZACION de las dependencias de catalogo y de las sustituciones (c) de la ventana clasica de
    /// Push Back (CT-PB0-17): con un diseño EDITADO en su JSON interior (<c>pb-01-base</c>), la ventana lo abre con
    /// <c>LoadExisting</c> y su salida se compara con el sobre editado, deserializado otra vez.
    ///
    /// <para>Se fija por caso el delta filtrado (sin las lineas de clase (a) y (b), D-PBT0-03), cada linea con su clase y el
    /// <c>archivo:linea</c> del producto que la causa, medido en la base. Abreviaturas de los archivos: W =
    /// src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs; WC = RackPushBackSystemWindow.Composite.cs (misma
    /// carpeta); LOAD = src/RackCad.Application/Systems/PushBack/PushBackEditorState.Load.cs. Una clase nueva se fija tal cual
    /// y se enumera en T-2 de la entrega: no es una parada (OB-06.5).</para>
    ///
    /// <para>Ninguna ventana se muestra y ninguna prueba ejecuta Insertar, «Guardar en biblioteca», el BOM ni el configurador real.</para>
    /// </summary>
    [Collection(EditorDiscardPromptCollection.Name)]
    public sealed class PushBackFullEditorCharacterizationCatalogTests
    {
        private const string MissingBeam = "PB0_VIGA_INEXISTENTE";

        private static readonly RackCatalog Catalog = JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static JsonNode Level(JsonNode inner, int front) => StructureNode(inner)["Fronts"][front]["Levels"][0];

        private static double TooDeep(JsonNode inner, int front)
        {
            var beamId = Level(inner, front)["InOutBeamCatalogId"].GetValue<string>();
            return DynamicRackLevelGeometry.AllowedPeraltes(Catalog, beamId).Max() + 0.5;
        }

        /// <summary>Los casos c-01 a c-15: nombre y edicion del JSON interior de pb-01-base.</summary>
        private static IEnumerable<KeyValuePair<string, Action<JsonNode>>> Cases()
        {
            yield return new KeyValuePair<string, Action<JsonNode>>("c-01 HeaderPostCatalogId vacio", inner => StructureNode(inner)["HeaderPostCatalogId"] = string.Empty);
            yield return new KeyValuePair<string, Action<JsonNode>>("c-02 HeaderPostCatalogId inexistente", inner => StructureNode(inner)["HeaderPostCatalogId"] = "PB0_POSTE_INEXISTENTE");
            yield return new KeyValuePair<string, Action<JsonNode>>("c-03 HighEndBeamPeraltes[0]=7.25", inner => inner["PushBack"]["Fronts"][0]["HighEndBeamPeraltes"][0] = 7.25);
            yield return new KeyValuePair<string, Action<JsonNode>>("c-04 InOutBeamCatalogId inexistente", inner => Level(inner, 0)["InOutBeamCatalogId"] = MissingBeam);
            yield return new KeyValuePair<string, Action<JsonNode>>("c-05 InOutBeamDepth fuera del maximo", inner => Level(inner, 0)["InOutBeamDepth"] = TooDeep(inner, 0));
            yield return new KeyValuePair<string, Action<JsonNode>>("c-06 IntermediateBeamCatalogId inexistente", inner => Level(inner, 0)["IntermediateBeamCatalogId"] = MissingBeam);
            yield return new KeyValuePair<string, Action<JsonNode>>("c-07 InOutBeamDepth fuera del maximo en el frente 1", inner => Level(inner, 1)["InOutBeamDepth"] = TooDeep(inner, 1));
            yield return new KeyValuePair<string, Action<JsonNode>>("c-08 PalletWeightUnit vacia", inner => StructureNode(inner)["PalletWeightUnit"] = string.Empty);
            yield return new KeyValuePair<string, Action<JsonNode>>("c-09 PalletWeightUnit ton", inner => StructureNode(inner)["PalletWeightUnit"] = "ton");
            yield return new KeyValuePair<string, Action<JsonNode>>("c-10 Dimensions 7", inner => StructureNode(inner)["Dimensions"] = 7);
            yield return new KeyValuePair<string, Action<JsonNode>>("c-11 Dimensions -1", inner => StructureNode(inner)["Dimensions"] = -1);
            yield return new KeyValuePair<string, Action<JsonNode>>("c-12 PalletsDeep 1", inner => StructureNode(inner)["PalletsDeep"] = 1);
            yield return new KeyValuePair<string, Action<JsonNode>>("c-13 PalletTolerance 0", inner => StructureNode(inner)["PalletTolerance"] = 0);
            yield return new KeyValuePair<string, Action<JsonNode>>("c-14 BeamDepth 0", inner => StructureNode(inner)["BeamDepth"] = 0);
            yield return new KeyValuePair<string, Action<JsonNode>>("c-15 AnnotationScale 0", inner => StructureNode(inner)["AnnotationScale"] = 0);
            // c-16 (agregado por el Worker, declarado como desviacion): 7 y -1 los normaliza el almacen a 0 ANTES de la ventana
            // (DynamicRackSystemDocument.cs:322,429), asi que nunca llegan al recorte de W:407; el limite valido Detailed (3) si.
            yield return new KeyValuePair<string, Action<JsonNode>>("c-16 Dimensions 3 limite valido", inner => StructureNode(inner)["Dimensions"] = 3);
        }

        /// <summary>
        /// La clase de una linea del delta filtrado y el <c>archivo:linea</c> del producto que la causa (medido en la base:
        /// la comparacion con el piloto Application dice si la causa esta en Application o solo en la ventana).
        /// </summary>
        private static string LabelOf(string line)
        {
            if (line.StartsWith("$.PushBack.Structure.HeaderPostCatalogId: \"\"", StringComparison.Ordinal))
                return "CARGA-APPLICATION:poste-vacio-a-defecto DynamicRackSystemResolver.cs:64-66";
            if (line.StartsWith("$.PushBack.Structure.HeaderPostCatalogId: ", StringComparison.Ordinal))
                return "(c):poste-inexistente-a-defecto RackPushBackSystemWindow.xaml.cs:390,836";
            if (line.StartsWith("$.PushBack.Fronts[0].HighEndBeamPeraltes[0]: ", StringComparison.Ordinal))
                return "CARGA-APPLICATION:peralte-posterior-normalizado PushBackEditorDesignAssembler.cs:155; PushBackEditorState.cs:259-269";
            if (line.EndsWith("InOutBeamCatalogId: \"PB0_VIGA_INEXISTENTE\" -> \"LARGUERO_IN_OUT_C6\"", StringComparison.Ordinal)
                || line.EndsWith("IntermediateBeamCatalogId: \"PB0_VIGA_INEXISTENTE\" -> \"LARGUERO_ESCALON_INFINITO\"", StringComparison.Ordinal))
                return "CARGA-APPLICATION:viga-inexistente-a-defecto DynamicRackLevelGeometry.cs:41,48,200-204";
            if (line.Contains(".InOutBeamDepth: "))
                return "CARGA-APPLICATION:peralte-fuera-de-catalogo DynamicRackLevelGeometry.cs:42-45; DynamicLoadBeamGeometry.cs:52";
            if (line.StartsWith("$.PushBack.Structure.BeamDepth: ", StringComparison.Ordinal))
                return "CARGA-APPLICATION:peralte-de-larguero-0-a-defecto PushBackEditorState.Load.cs:200";
            return "SIN-ETIQUETA";
        }

        [Fact]
        public void CT_PB0_17_CatalogDependenciesAndTheSubstitutionsOfLoadFromModelProduceTheCharacterizedDelta()
        {
            var log = new List<string>();
            Sta(() =>
            {
                foreach (var testCase in Cases())
                {
                    var text = EditInner(Fixture("pb-01-base"), testCase.Value);
                    RackPushBackSystemWindow w;
                    try
                    {
                        w = Abrir(text);
                    }
                    catch (Exception rejected)
                    {
                        // El producto rechaza el documento ANTES de que la ventana pueda mostrarlo (se fija tal cual, sin corregir).
                        log.Add(testCase.Key + " :: RECHAZADO " + rejected.GetType().Name + ": " + rejected.Message);
                        continue;
                    }

                    try
                    {
                        var salida = Salida(w);
                        if (salida.StartsWith("NO-VÁLIDA", StringComparison.Ordinal))
                        {
                            log.Add(testCase.Key + " :: " + salida);
                            continue;
                        }

                        var sobre = Sobre(text);
                        log.Add(testCase.Key + " :: retiradas(a/b)=[" + string.Join(" ;; ", Removed(sobre, salida)) + "]");
                        foreach (var line in Filtered(sobre, salida)) log.Add("   [" + LabelOf(line) + "] " + line);
                    }
                    finally { Cerrar(w); }
                }
            });

            Assert.Equal(
                new[]
                {
                    "c-01 HeaderPostCatalogId vacio :: retiradas(a/b)=[]",
                    "   [CARGA-APPLICATION:poste-vacio-a-defecto DynamicRackSystemResolver.cs:64-66] $.PushBack.Structure.HeaderPostCatalogId: \"\" -> \"POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA\"",
                    "c-02 HeaderPostCatalogId inexistente :: retiradas(a/b)=[]",
                    "   [(c):poste-inexistente-a-defecto RackPushBackSystemWindow.xaml.cs:390,836] $.PushBack.Structure.HeaderPostCatalogId: \"PB0_POSTE_INEXISTENTE\" -> \"POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA\"",
                    "c-03 HighEndBeamPeraltes[0]=7.25 :: retiradas(a/b)=[]",
                    "   [CARGA-APPLICATION:peralte-posterior-normalizado PushBackEditorDesignAssembler.cs:155; PushBackEditorState.cs:259-269] $.PushBack.Fronts[0].HighEndBeamPeraltes[0]: 7.25 -> 3.5",
                    "c-04 InOutBeamCatalogId inexistente :: retiradas(a/b)=[]",
                    "   [CARGA-APPLICATION:viga-inexistente-a-defecto DynamicRackLevelGeometry.cs:41,48,200-204] $.PushBack.Structure.Fronts[0].Levels[0].InOutBeamCatalogId: \"PB0_VIGA_INEXISTENTE\" -> \"LARGUERO_IN_OUT_C6\"",
                    "c-05 InOutBeamDepth fuera del maximo :: retiradas(a/b)=[]",
                    "   [CARGA-APPLICATION:peralte-fuera-de-catalogo DynamicRackLevelGeometry.cs:42-45; DynamicLoadBeamGeometry.cs:52] $.PushBack.Structure.Fronts[0].Levels[0].InOutBeamDepth: 6.5 -> 6",
                    "c-06 IntermediateBeamCatalogId inexistente :: retiradas(a/b)=[]",
                    "   [CARGA-APPLICATION:viga-inexistente-a-defecto DynamicRackLevelGeometry.cs:41,48,200-204] $.PushBack.Structure.Fronts[0].Levels[0].IntermediateBeamCatalogId: \"PB0_VIGA_INEXISTENTE\" -> \"LARGUERO_ESCALON_INFINITO\"",
                    "c-07 InOutBeamDepth fuera del maximo en el frente 1 :: retiradas(a/b)=[]",
                    "   [CARGA-APPLICATION:peralte-fuera-de-catalogo DynamicRackLevelGeometry.cs:42-45; DynamicLoadBeamGeometry.cs:52] $.PushBack.Structure.Fronts[1].Levels[0].InOutBeamDepth: 6.5 -> 6",
                    "c-08 PalletWeightUnit vacia :: retiradas(a/b)=[]",
                    "c-09 PalletWeightUnit ton :: retiradas(a/b)=[]",
                    "c-10 Dimensions 7 :: retiradas(a/b)=[]",
                    "c-11 Dimensions -1 :: retiradas(a/b)=[]",
                    "c-12 PalletsDeep 1 :: RECHAZADO InvalidOperationException: El proyecto no tiene datos válidos para el sistema Push Back (¿archivo vacío o incompleto?).",
                    "c-13 PalletTolerance 0 :: RECHAZADO ArgumentException: La holgura transversal debe ser mayor que cero. (Parameter 'design')",
                    "c-14 BeamDepth 0 :: retiradas(a/b)=[]",
                    "   [CARGA-APPLICATION:peralte-de-larguero-0-a-defecto PushBackEditorState.Load.cs:200] $.PushBack.Structure.BeamDepth: 0 -> 6",
                    "c-15 AnnotationScale 0 :: retiradas(a/b)=[]",
                    "c-16 Dimensions 3 limite valido :: retiradas(a/b)=[]",
                },
                log);
        }
    }
}
