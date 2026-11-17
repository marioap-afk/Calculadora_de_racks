using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.PushBack;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-64 F4B PB-T0 (CT-PB0-18) — CARACTERIZACION de la perdida (b), en su clase propia (OB-06.3b): el
    /// <c>DimensionStyleName</c> que el plan lleva en cada cota. Application lo conserva —el estilo del diseño llega
    /// tal cual a cada instancia <c>Dimension</c> de los cuatro planes y de cada corte lateral—; la perdida ocurre
    /// despues, en la ventana (que no lo relee de sus controles), y la fijan <c>T19_H1</c> y WLC. Aqui solo se fija lo
    /// que Application hace HOY, sin corregir nada.
    ///
    /// <para>Se ejecuta el producto real (resolver, BOM y planes con el catalogo entregado); sin ventana, sin
    /// AutoCAD, sin escribir en disco. El orden es ordinal y no se fija ninguna coma flotante.</para>
    /// </summary>
    public sealed class PushBackFullEditorCharacterizationDimensionStyleLossTests
    {
        private static readonly RackCatalog Catalog = JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static string Fixture(string name)
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln"))) dir = dir.Parent;
            Assert.True(dir != null, "repo root (RackCad.sln) not found");
            var path = Path.Combine(dir.FullName, "tests", "RackCad.Tests", "Fixtures", "I64", name + ".json");
            Assert.True(File.Exists(path), "missing fixture " + path);
            return File.ReadAllText(path);
        }

        private static PushBackDesign Design(string fixture, bool withoutStyle)
        {
            var embed = new RackEmbedStore().Deserialize(Fixture(fixture));
            var design = new RackProjectStore().Deserialize(embed.Design).PushBackDesign;
            Assert.NotNull(design);
            if (withoutStyle) design.Structure.DimensionStyle = null;
            return design;
        }

        /// <summary>Los planes de una computacion en el orden ordinal de sus claves: cuatro planes y un corte por poste.</summary>
        private static List<KeyValuePair<string, HeaderRunPlan>> Plans(PushBackEditorComputation computation)
        {
            var plans = new List<KeyValuePair<string, HeaderRunPlan>>
            {
                new KeyValuePair<string, HeaderRunPlan>("planta", computation.PlantaPlan),
                new KeyValuePair<string, HeaderRunPlan>("frontal-es", computation.FrontalEntradaSalida),
                new KeyValuePair<string, HeaderRunPlan>("frontal-post", computation.FrontalPosterior),
                new KeyValuePair<string, HeaderRunPlan>("lateral", computation.LateralPlan),
            };
            foreach (var corte in computation.LateralCortes)
            {
                plans.Add(new KeyValuePair<string, HeaderRunPlan>("corte-" + corte.PostIndex, corte.Plan));
            }

            return plans.OrderBy(plan => plan.Key, StringComparer.Ordinal).ToList();
        }

        /// <summary>Por plan: cuantas cotas (Role == Dimension) lleva y el conjunto ordenado de sus nombres de estilo.</summary>
        private static Dictionary<string, (int Count, string[] Styles)> Dimensions(PushBackEditorComputation computation)
        {
            var result = new Dictionary<string, (int, string[])>(StringComparer.Ordinal);
            foreach (var plan in Plans(computation))
            {
                var dimensions = plan.Value.Flatten().Instances.Where(instance => instance.Role == HeaderBlockRole.Dimension).ToList();
                result[plan.Key] = (
                    dimensions.Count,
                    dimensions.Select(dimension => dimension.DimensionStyleName ?? "<null>").Distinct(StringComparer.Ordinal).OrderBy(name => name, StringComparer.Ordinal).ToArray());
            }

            return result;
        }

        private static PushBackEditorComputation Compute(string fixture, bool withoutStyle)
        {
            var computation = new PushBackEditorDesignAssembler(Catalog).BuildFrom(Design(fixture, withoutStyle), PushBackSide.A);
            Assert.True(computation.IsValid, computation.Error);
            return computation;
        }

        /// <summary>El conjunto de nombres de estilo de TODAS las cotas de todos los planes, y cuantas cotas lleva cada plan.</summary>
        private static string Summary(PushBackEditorComputation computation)
        {
            var dimensions = Dimensions(computation);
            var styles = dimensions.Values.SelectMany(entry => entry.Styles).Distinct(StringComparer.Ordinal).OrderBy(name => name, StringComparer.Ordinal);
            return "{" + string.Join(",", styles) + "} " + string.Join("|", dimensions.OrderBy(pair => pair.Key, StringComparer.Ordinal).Select(pair => pair.Key + "=" + pair.Value.Count));
        }

        private const string Pb06Counts = "corte-0=16|corte-1=16|corte-2=16|corte-3=16|frontal-es=11|frontal-post=11|lateral=16|planta=12";
        private const string Pb07Counts = "corte-0=2|corte-1=2|corte-2=2|corte-3=2|corte-4=2|frontal-es=2|frontal-post=2|lateral=2|planta=2";

        [Fact]
        public void CT_PB0_18_TheDimensionStyleNameOfTheDesignReachesEveryDimensionOfEveryPlanAndEveryCorteAsItIs()
        {
            // Los dos diseños del corpus que llevan estilo, tal cual y con el estilo retirado del diseño.
            var pb06 = Compute("pb-06-seguridad", withoutStyle: false);
            var pb06None = Compute("pb-06-seguridad", withoutStyle: true);
            var pb07 = Compute("pb-07-combinado", withoutStyle: false);
            var pb07None = Compute("pb-07-combinado", withoutStyle: true);

            // ANCLAS (T-6): el estilo resuelto y el numero de cotas por plan no cambian con el mutante del estilo.
            Assert.Equal("STANDARD-ISO", pb06.System.Structure.DimensionStyle);
            Assert.Null(pb06None.System.Structure.DimensionStyle);
            Assert.Equal("SIMPLEX", pb07.System.Structure.DimensionStyle);
            Assert.Null(pb07None.System.Structure.DimensionStyle);
            Assert.Equal(Pb06Counts, string.Join("|", Dimensions(pb06).OrderBy(pair => pair.Key, StringComparer.Ordinal).Select(pair => pair.Key + "=" + pair.Value.Count)));
            Assert.Equal(Pb06Counts, string.Join("|", Dimensions(pb06None).OrderBy(pair => pair.Key, StringComparer.Ordinal).Select(pair => pair.Key + "=" + pair.Value.Count)));
            Assert.Equal(Pb07Counts, string.Join("|", Dimensions(pb07).OrderBy(pair => pair.Key, StringComparer.Ordinal).Select(pair => pair.Key + "=" + pair.Value.Count)));
            Assert.Equal(Pb07Counts, string.Join("|", Dimensions(pb07None).OrderBy(pair => pair.Key, StringComparer.Ordinal).Select(pair => pair.Key + "=" + pair.Value.Count)));
            Assert.All(Dimensions(pb06).Values.Concat(Dimensions(pb07).Values), entry => Assert.True(entry.Count > 0));

            // LA SENAL (mutante del estilo): el nombre llega a cada cota exactamente como el diseño lo trae.
            Assert.Equal("{STANDARD-ISO} " + Pb06Counts, Summary(pb06));
            Assert.Equal("{<null>} " + Pb06Counts, Summary(pb06None));
            Assert.Equal("{SIMPLEX} " + Pb07Counts, Summary(pb07));
            Assert.Equal("{<null>} " + Pb07Counts, Summary(pb07None));
        }
    }
}
