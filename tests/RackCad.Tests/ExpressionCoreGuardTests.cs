using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using RackCad.Application.Expressions;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-49 G5 — guarda NUEVA de V6 P28.4: independencia del núcleo <c>RackCad.Application.Expressions</c>.
    ///
    /// <para>
    /// El núcleo no depende de Project Variables, persistencia, sistemas, BOM, catálogos, secciones estructurales,
    /// Domain, UI, Plugin ni AutoCAD (DR-2, P26.1), y no usa motores externos de expresiones (P1.10). La dirección
    /// de dependencia es de Project Variables hacia el núcleo, nunca al revés: es lo que permite que ID23 lo
    /// reutilice sin arrastrar variables.
    /// </para>
    /// <para>
    /// Es defensa SECUNDARIA (P28.1): el criterio de aceptación son las pruebas de comportamiento. Mira dos cosas
    /// distintas —el texto del código y las firmas de los tipos compilados— para que un comentario o un alias no
    /// basten para esquivarla.
    /// </para>
    /// <para>
    /// P28.4 extiende esta frontera también a la autoridad neutral de unidades (P9.5). Esa autoridad no existe en G5:
    /// cuando G6 la cree, entra en esta guarda y en la de capa pura sin AutoCAD, y el lexer pasa a leer los tokens de
    /// unidad de ella en vez de declarar los suyos.
    /// </para>
    /// </summary>
    public class ExpressionCoreGuardTests
    {
        private const string CoreNamespace = "RackCad.Application.Expressions";

        /// <summary>Lo que el núcleo no puede nombrar en su código (P28.4, P1.10).</summary>
        private static readonly string[] ForbiddenCoreTokens =
        {
            "ProjectVariables", "Persistence", "Systems", "Bom", "Catalogs", "StructuralSections",
            "RackCad.Domain", "RackCad.UI", "RackCad.Plugin", "Autodesk",
            "NCalc", "DataTable", "System.Linq.Expressions", "Roslyn", "Microsoft.CodeAnalysis",
        };

        /// <summary>Los mismos límites, como namespaces de los tipos que las firmas del núcleo pueden mencionar.</summary>
        private static readonly string[] ForbiddenNamespaces =
        {
            "RackCad.Application.ProjectVariables", "RackCad.Application.Persistence", "RackCad.Application.Systems",
            "RackCad.Application.Bom", "RackCad.Application.Catalogs", "RackCad.Application.StructuralSections",
            "RackCad.Domain", "RackCad.UI", "RackCad.Plugin", "Autodesk",
            "NCalc", "System.Data", "System.Linq.Expressions", "Microsoft.CodeAnalysis",
        };

        private static DirectoryInfo RepoRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);

            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln")))
            {
                dir = dir.Parent;
            }

            Assert.NotNull(dir);
            return dir;
        }

        private static string CoreFolder()
            => Path.Combine(RepoRoot().FullName, "src", "RackCad.Application", "Expressions");

        private static IReadOnlyList<string> CoreSources()
        {
            var folder = CoreFolder();
            Assert.True(Directory.Exists(folder), "No existe el núcleo de expresiones: " + folder);

            return Directory
                .GetFiles(folder, "*.cs", SearchOption.AllDirectories)
                .Where(path => !path.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar, StringComparison.Ordinal)
                            && !path.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar, StringComparison.Ordinal))
                .ToList();
        }

        /// <summary>El CÓDIGO del archivo: sin líneas de comentario, porque la prohibición es sobre lo que se ejecuta.</summary>
        private static string Code(string path)
            => string.Join(
                "\n",
                File.ReadAllLines(path).Where(line => !line.TrimStart().StartsWith("//", StringComparison.Ordinal)));

        /// <summary>Los tipos compilados del núcleo, incluidos los generados por el compilador y los de sub-namespaces.</summary>
        private static IReadOnlyList<Type> CoreTypes()
            => typeof(ExpressionParser).Assembly
                .GetTypes()
                .Where(type => type.Namespace != null
                               && (string.Equals(type.Namespace, CoreNamespace, StringComparison.Ordinal)
                                   || type.Namespace.StartsWith(CoreNamespace + ".", StringComparison.Ordinal)))
                .ToList();

        // ================================================================ la barrida mira de verdad

        /// <summary>
        /// Una comprobación de AUSENCIA pasa sola si no mira nada. Antes que nada: el núcleo existe, la barrida
        /// encuentra sus archivos y ve en ellos lo que sabemos que está.
        /// </summary>
        [Fact]
        public void LA_BARRIDA_DEL_NUCLEO_MIRA_DE_VERDAD()
        {
            var sources = CoreSources();

            Assert.True(sources.Count >= 5, "La barrida apenas encontró archivos en el núcleo: " + sources.Count);
            Assert.Contains(sources, path => Code(path).Contains("class ExpressionParser", StringComparison.Ordinal));
            Assert.Contains(sources, path => Code(path).Contains("namespace " + CoreNamespace, StringComparison.Ordinal));
            Assert.True(CoreTypes().Count >= 5, "La reflexión apenas encontró tipos del núcleo.");
        }

        // ================================================================ texto

        [Fact]
        public void EL_NUCLEO_NO_NOMBRA_OTRAS_CAPAS_NI_MOTORES_EXTERNOS()
        {
            foreach (var path in CoreSources())
            {
                var code = Code(path);

                foreach (var token in ForbiddenCoreTokens)
                {
                    Assert.False(
                        code.Contains(token, StringComparison.Ordinal),
                        Path.GetFileName(path) + " contiene '" + token + "'");
                }
            }
        }

        // ================================================================ firmas compiladas

        [Fact]
        public void LAS_FIRMAS_DEL_NUCLEO_NO_MENCIONAN_TIPOS_DE_OTRAS_CAPAS()
        {
            var offenders = new List<string>();

            foreach (var type in CoreTypes())
            {
                foreach (var referenced in Signatures(type).SelectMany(Expand).Distinct())
                {
                    var ns = referenced.Namespace ?? string.Empty;

                    if (ForbiddenNamespaces.Any(forbidden => ns == forbidden || ns.StartsWith(forbidden + ".", StringComparison.Ordinal)))
                    {
                        offenders.Add(type.FullName + " → " + referenced.FullName);
                    }
                }
            }

            Assert.True(offenders.Count == 0, "El núcleo depende de otra capa:\n" + string.Join("\n", offenders));
        }

        /// <summary>Todos los tipos que un tipo declara en su superficie, incluidos los miembros no públicos.</summary>
        private static IEnumerable<Type> Signatures(Type type)
        {
            const BindingFlags all = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                                     | BindingFlags.Static | BindingFlags.DeclaredOnly;

            if (type.BaseType != null)
            {
                yield return type.BaseType;
            }

            foreach (var contract in type.GetInterfaces())
            {
                yield return contract;
            }

            foreach (var field in type.GetFields(all))
            {
                yield return field.FieldType;
            }

            foreach (var property in type.GetProperties(all))
            {
                yield return property.PropertyType;
            }

            foreach (var method in type.GetMethods(all))
            {
                yield return method.ReturnType;

                foreach (var parameter in method.GetParameters())
                {
                    yield return parameter.ParameterType;
                }
            }

            foreach (var constructor in type.GetConstructors(all))
            {
                foreach (var parameter in constructor.GetParameters())
                {
                    yield return parameter.ParameterType;
                }
            }
        }

        /// <summary>Un tipo y todo lo que lleva dentro: elemento de arrays, referencias y argumentos genéricos.</summary>
        private static IEnumerable<Type> Expand(Type type)
        {
            if (type == null || type.IsGenericParameter)
            {
                yield break;
            }

            if (type.HasElementType)
            {
                foreach (var inner in Expand(type.GetElementType()))
                {
                    yield return inner;
                }

                yield break;
            }

            yield return type;

            if (type.IsGenericType)
            {
                foreach (var argument in type.GetGenericArguments())
                {
                    foreach (var inner in Expand(argument))
                    {
                        yield return inner;
                    }
                }
            }
        }
    }
}
