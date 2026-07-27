using System;
using System.IO;
using System.Linq;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// PB-004, consumidores (I-32) — el guardia que mantiene el override en OPT-IN.
    ///
    /// El contexto de elevaciones es seguro mientras nadie lo construya fuera de Push Back: con <c>null</c> los
    /// builders compartidos se comportan exactamente como siempre, y el centinela de firmas
    /// (<see cref="DynamicNullOverrideGoldenTests"/>) lo mide. Pero ese centinela solo prueba la ruta que ejercita;
    /// si mañana un camino Selectivo o Dinámico construyera un contexto, el override dejaría de ser opt-in por una
    /// vía que ninguna prueba de dibujo recorre.
    ///
    /// Esto se lee como TEXTO, igual que el resto de guardias de fuente del repo, y fija dos cosas: quién puede
    /// fabricar un contexto, y que el tipo del contexto sea de verdad neutral.
    /// </summary>
    public class ElevationOverrideSourceGuardTests
    {
        private static DirectoryInfo RepoRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln")))
            {
                dir = dir.Parent;
            }

            Assert.True(dir != null, "No se localizó la raíz del repo (RackCad.sln) desde el directorio de salida.");
            return dir;
        }

        private static string[] ProductionSources()
            => Directory.GetFiles(Path.Combine(RepoRoot().FullName, "src"), "*.cs", SearchOption.AllDirectories)
                .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
                    && !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
                .ToArray();

        /// <summary>
        /// Solo Push Back fabrica contextos. Cualquier otro fichero que llame a <c>PushBackElevations.Context</c> o
        /// a <c>RackLevelElevations.From</c> estaría metiendo el override en un sistema que no lo pidió.
        /// </summary>
        [Fact]
        public void OnlyPushBackBuildsAnElevationContext()
        {
            var offenders = ProductionSources()
                .Where(path =>
                {
                    var name = Path.GetFileName(path);
                    if (name.StartsWith("PushBack", StringComparison.Ordinal)
                        || string.Equals(name, "RackLevelElevations.cs", StringComparison.Ordinal))
                    {
                        return false;
                    }

                    var text = File.ReadAllText(path);
                    return text.Contains("PushBackElevations.Context(", StringComparison.Ordinal)
                        || text.Contains("RackLevelElevations.From(", StringComparison.Ordinal);
                })
                .Select(path => Path.GetFileName(path))
                .ToList();

            Assert.True(offenders.Count == 0,
                "un fichero que no es de Push Back construye un contexto de elevaciones, así que el override dejó " +
                "de ser opt-in: " + string.Join(", ", offenders));
        }

        /// <summary>
        /// Y quien SÍ lo fabrica es exactamente quien debe: los dos builders de vista que consumen los builders
        /// compartidos. Si esta lista se queda corta, es que hay una vista dibujando sobre elevaciones distintas de
        /// las de sus propias piezas.
        /// </summary>
        [Fact]
        public void TheTwoPushBackViewBuilders_DoBuildOne()
        {
            foreach (var name in new[] { "PushBackSystemLateralBuilder.cs", "PushBackSystemFrontalBuilder.cs" })
            {
                var path = ProductionSources().Single(candidate => string.Equals(Path.GetFileName(candidate), name, StringComparison.Ordinal));
                Assert.Contains("PushBackElevations.Context(", File.ReadAllText(path), StringComparison.Ordinal);
            }
        }

        /// <summary>
        /// El contexto es NEUTRAL: ni nombres ni tipos de ningún sistema concreto. Es lo que le permite vivir en los
        /// builders compartidos sin arrastrar Push Back hasta ellos.
        /// </summary>
        [Fact]
        public void TheContextType_MentionsNoConcreteSystem()
        {
            var path = ProductionSources()
                .Single(candidate => string.Equals(Path.GetFileName(candidate), "RackLevelElevations.cs", StringComparison.Ordinal));
            var text = File.ReadAllText(path);

            foreach (var word in new[] { "PushBack", "Selective", "Dynamic" })
            {
                Assert.False(text.Contains(word, StringComparison.Ordinal),
                    $"el contexto de elevaciones menciona «{word}»; tiene que ser neutral");
            }
        }
    }
}
