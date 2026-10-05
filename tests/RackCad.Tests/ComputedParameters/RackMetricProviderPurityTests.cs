using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using RackCad.Application.ComputedParameters;
using RackCad.Application.Persistence;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-63 G1 / INV-09 — los providers son puros (D-08). UNA sola funcion de guarda,
    /// <see cref="ForbiddenProviderDependencies"/>, sobre el codigo sin comentarios, se aplica a los archivos reales
    /// de provider y a un fixture invalido: la misma funcion debe detectarlo.
    /// </summary>
    public class ComputedParametersRackMetricProviderPurityTests
    {
        private static readonly string[] SixKindTokens =
        {
            RackEmbedDocument.KindSelective,
            RackEmbedDocument.KindDynamic,
            RackEmbedDocument.KindPushBack,
            RackEmbedDocument.KindCantilever,
            RackEmbedDocument.KindCabecera,
            RackEmbedDocument.KindCama,
        };

        /// <summary>Tipos prohibidos en un provider: resolvers, RegistryEvaluation, ExpressionBinder, ProjectVariables*, Autodesk, catalogos y stores.</summary>
        private static readonly Regex[] Forbidden =
        {
            new Regex(@"\b\w*Resolver\b"),
            new Regex(@"\bRegistryEvaluation\b"),
            new Regex(@"\bExpressionBinder\b"),
            new Regex(@"\bProjectVariables?\w*"),
            new Regex(@"\bAutodesk\b"),
            new Regex(@"\b\w*Catalog\w*\b"),
            new Regex(@"\b\w*Store\b"),
        };

        private static string CodeOnly(string source)
        {
            var withoutBlocks = Regex.Replace(source, @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);
            return Regex.Replace(withoutBlocks, @"//[^\n]*", string.Empty);
        }

        /// <summary>La guarda UNICA: los tipos prohibidos que aparecen en el codigo (sin comentarios) de un archivo.</summary>
        private static IReadOnlyList<string> ForbiddenProviderDependencies(string sourceCode)
        {
            var code = CodeOnly(sourceCode);
            return Forbidden
                .SelectMany(pattern => pattern.Matches(code).Cast<Match>().Select(match => match.Value))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToList();
        }

        private static DirectoryInfo RepoRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln")))
            {
                dir = dir.Parent;
            }

            Assert.True(dir != null, "No se encontro la raiz del repositorio (RackCad.sln).");
            return dir;
        }

        private static IReadOnlyList<string> ProviderFiles()
        {
            var folder = Path.Combine(RepoRoot().FullName, "src", "RackCad.Application", "ComputedParameters");
            Assert.True(Directory.Exists(folder), "No existe la carpeta: " + folder);
            return Directory.GetFiles(folder, "RackMetricProvider*.cs").OrderBy(path => path, StringComparer.Ordinal).ToList();
        }

        [Fact]
        public void INV09_ElRegistroTieneUnProviderPorCadaUnoDeLosSeisTokens_ConstruidoConKindDispatch()
        {
            var registry = RackMetricProviderRegistry.Default;

            Assert.IsType<KindDispatch<IRackMetricProvider>>(registry.Dispatch);
            Assert.Equal(
                SixKindTokens.OrderBy(token => token, StringComparer.Ordinal),
                registry.Dispatch.Items.Select(provider => provider.KindToken).OrderBy(token => token, StringComparer.Ordinal));

            foreach (var token in SixKindTokens)
            {
                Assert.True(registry.TryGet(token, out var provider), "Falta el provider de " + token);
                Assert.Equal(token, provider.KindToken);
            }
        }

        [Fact]
        public void INV09_LaGuardaPasaEnCadaArchivoRealDeProvider_Y_DetectaElFixtureInvalido()
        {
            // (a) Los providers reales: los seis estan registrados, existen en archivos, y la guarda no encuentra nada.
            Assert.Equal(SixKindTokens.Length, RackMetricProviderRegistry.Default.Dispatch.Items.Count);

            var files = ProviderFiles();
            Assert.Contains(files, path => Path.GetFileName(path) == "RackMetricProviders.cs");
            Assert.Contains(files, path => Path.GetFileName(path) == "RackMetricProviderRegistry.cs");

            foreach (var path in files)
            {
                Assert.Empty(ForbiddenProviderDependencies(File.ReadAllText(path)));
            }

            // (b) Fixture deliberadamente invalido, evaluado por LA MISMA funcion: llama a un resolver.
            const string invalidProvider = @"
                namespace Fixture
                {
                    internal sealed class BadProvider
                    {
                        public object Compute(object design, object catalog)
                        {
                            // SelectiveGeometryResolver en un comentario no cuenta; la llamada de abajo si.
                            return new SelectiveGeometryResolver().Resolve(design, catalog);
                        }
                    }
                }";

            Assert.Contains("SelectiveGeometryResolver", ForbiddenProviderDependencies(invalidProvider));

            // El comentario solo NO basta para disparar la guarda (opera sobre el codigo sin comentarios).
            Assert.Empty(ForbiddenProviderDependencies("// SelectiveGeometryResolver\ninternal sealed class Ok { }"));
        }

        [Fact]
        public void INV09_ElProviderDeUnKindSinSoporteNoNecesitaPrerrequisito_YEsDeterminista()
        {
            // D-08: pura, sin estado ni cache. Misma entrada, mismos resultados, y sin lectura ni resolucion.
            var registry = RackMetricProviderRegistry.Default;
            Assert.True(registry.TryGet(RackEmbedDocument.KindPushBack, out var provider));

            var input = new RackMetricInput("rack-1", RackEmbedDocument.KindPushBack, prerequisite: null);
            var first = provider.Compute(input);
            var second = provider.Compute(input);

            foreach (var metric in RackMetricIds.RackMetrics)
            {
                Assert.Equal(MetricStatus.NotSupported, first[metric].Status);
                Assert.Equal(first[metric], second[metric]);
            }
        }
    }
}
