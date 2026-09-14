using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using RackCad.Application.Expressions;
using RackCad.Application.Units;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-49 — guardas NUEVAS de V6 P28.4 sobre el núcleo <c>RackCad.Application.Expressions</c> y la autoridad neutral de
    /// unidades <c>RackCad.Application.Units</c>.
    ///
    /// <para>
    /// Independencia (G5, extendida en G6 a la autoridad de unidades): ninguno de los dos depende de Project Variables,
    /// persistencia, sistemas, BOM, catálogos, secciones estructurales, Domain, UI, Plugin ni AutoCAD (DR-2, P26.1), y el
    /// núcleo no usa motores externos de expresiones (P1.10). La dirección es de Project Variables hacia el núcleo y del
    /// núcleo hacia las unidades, nunca al revés: la autoridad de unidades no depende de nada, tampoco del núcleo (P9.5).
    /// </para>
    /// <para>
    /// Sin comprobador dimensional (G6): el criterio son las pruebas de comportamiento de P5.6; como defensa secundaria,
    /// ni el núcleo ni las unidades declaran tipos ni códigos dimensionales.
    /// </para>
    /// <para>
    /// Es defensa SECUNDARIA (P28.1). Mira dos cosas distintas —el texto del código y las firmas de los tipos compilados—
    /// para que un comentario o un alias no basten para esquivarla.
    /// </para>
    /// </summary>
    public class ExpressionCoreGuardTests
    {
        private const string CoreNamespace = "RackCad.Application.Expressions";
        private const string UnitsNamespace = "RackCad.Application.Units";

        /// <summary>Lo que ni el núcleo ni la autoridad de unidades pueden nombrar en su código (P28.4, P1.10).</summary>
        private static readonly string[] ForbiddenCoreTokens =
        {
            "ProjectVariables", "Persistence", "Systems", "Bom", "Catalogs", "StructuralSections",
            "RackCad.Domain", "RackCad.UI", "RackCad.Plugin", "Autodesk",
            "NCalc", "DataTable", "System.Linq.Expressions", "Roslyn", "Microsoft.CodeAnalysis",
        };

        /// <summary>Los mismos límites, como namespaces de los tipos que las firmas pueden mencionar.</summary>
        private static readonly string[] ForbiddenNamespaces =
        {
            "RackCad.Application.ProjectVariables", "RackCad.Application.Persistence", "RackCad.Application.Systems",
            "RackCad.Application.Bom", "RackCad.Application.Catalogs", "RackCad.Application.StructuralSections",
            "RackCad.Domain", "RackCad.UI", "RackCad.Plugin", "Autodesk",
            "NCalc", "System.Data", "System.Linq.Expressions", "Microsoft.CodeAnalysis",
        };

        /// <summary>Conceptos de un comprobador dimensional o de tipos de valor que el motor adimensional no tiene (P5.4).</summary>
        private static readonly string[] DimensionalConcepts =
        {
            "ExpressionValueType", "DimensionMismatch", "ResultTypeMismatch", "Dimension", "ValueKind", "UnitMismatch",
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

        private static string Folder(string name)
            => Path.Combine(RepoRoot().FullName, "src", "RackCad.Application", name);

        private static IReadOnlyList<string> Sources(string folderName)
        {
            var folder = Folder(folderName);
            Assert.True(Directory.Exists(folder), "No existe la carpeta: " + folder);

            return Directory
                .GetFiles(folder, "*.cs", SearchOption.AllDirectories)
                .Where(path => !path.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar, StringComparison.Ordinal)
                            && !path.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar, StringComparison.Ordinal))
                .ToList();
        }

        private static IReadOnlyList<string> CoreSources() => Sources("Expressions");

        private static IReadOnlyList<string> UnitsSources() => Sources("Units");

        /// <summary>El CÓDIGO del archivo: sin líneas de comentario, porque la prohibición es sobre lo que se ejecuta.</summary>
        private static string Code(string path)
            => string.Join(
                "\n",
                File.ReadAllLines(path).Where(line => !line.TrimStart().StartsWith("//", StringComparison.Ordinal)));

        /// <summary>Los tipos compilados de un namespace y sus sub-namespaces, incluidos los generados por el compilador.</summary>
        private static IReadOnlyList<Type> TypesOf(string ns)
            => typeof(ExpressionParser).Assembly
                .GetTypes()
                .Where(type => type.Namespace != null
                               && (string.Equals(type.Namespace, ns, StringComparison.Ordinal)
                                   || type.Namespace.StartsWith(ns + ".", StringComparison.Ordinal)))
                .ToList();

        // ================================================================ la barrida mira de verdad

        /// <summary>
        /// Una comprobación de AUSENCIA pasa sola si no mira nada. Antes que nada: el núcleo y las unidades existen, la
        /// barrida encuentra sus archivos y ve en ellos lo que sabemos que está.
        /// </summary>
        [Fact]
        public void LA_BARRIDA_DEL_NUCLEO_MIRA_DE_VERDAD()
        {
            var sources = CoreSources();

            Assert.True(sources.Count >= 5, "La barrida apenas encontró archivos en el núcleo: " + sources.Count);
            Assert.Contains(sources, path => Code(path).Contains("class ExpressionParser", StringComparison.Ordinal));
            Assert.Contains(sources, path => Code(path).Contains("class ExpressionBinder", StringComparison.Ordinal));
            Assert.Contains(sources, path => Code(path).Contains("namespace " + CoreNamespace, StringComparison.Ordinal));
            Assert.True(TypesOf(CoreNamespace).Count >= 5, "La reflexión apenas encontró tipos del núcleo.");

            Assert.Contains(UnitsSources(), path => Code(path).Contains("class LengthUnits", StringComparison.Ordinal));
            Assert.Contains(TypesOf(UnitsNamespace), type => type == typeof(LengthUnits));
        }

        // ================================================================ texto

        [Fact]
        public void EL_NUCLEO_Y_LAS_UNIDADES_NO_NOMBRAN_OTRAS_CAPAS_NI_MOTORES_EXTERNOS()
        {
            foreach (var path in CoreSources().Concat(UnitsSources()))
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

        /// <summary>P9.5: la autoridad de unidades no depende de nada, tampoco del núcleo que la consume.</summary>
        [Fact]
        public void LA_AUTORIDAD_DE_UNIDADES_NO_DEPENDE_DEL_NUCLEO()
        {
            foreach (var path in UnitsSources())
            {
                Assert.False(
                    Code(path).Contains("Expressions", StringComparison.Ordinal),
                    Path.GetFileName(path) + " nombra el núcleo de expresiones");
            }

            var offenders = TypesOf(UnitsNamespace)
                .SelectMany(type => Signatures(type).SelectMany(Expand).Distinct().Select(referenced => (type, referenced)))
                .Where(pair => (pair.referenced.Namespace ?? string.Empty).StartsWith(CoreNamespace, StringComparison.Ordinal))
                .Select(pair => pair.type.FullName + " → " + pair.referenced.FullName)
                .ToList();

            Assert.True(offenders.Count == 0, "La autoridad de unidades depende del núcleo:\n" + string.Join("\n", offenders));
        }

        // ================================================================ firmas compiladas

        [Fact]
        public void LAS_FIRMAS_DEL_NUCLEO_Y_LAS_UNIDADES_NO_MENCIONAN_TIPOS_DE_OTRAS_CAPAS()
        {
            var offenders = new List<string>();

            foreach (var type in TypesOf(CoreNamespace).Concat(TypesOf(UnitsNamespace)))
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

        // ================================================================ sin comprobador dimensional (G6)

        [Fact]
        public void EL_NUCLEO_NO_DECLARA_TIPOS_NI_CODIGOS_DIMENSIONALES()
        {
            foreach (var path in CoreSources().Concat(UnitsSources()))
            {
                var code = Code(path);

                foreach (var concept in DimensionalConcepts)
                {
                    Assert.False(
                        code.Contains(concept, StringComparison.Ordinal),
                        Path.GetFileName(path) + " contiene el concepto dimensional '" + concept + "'");
                }
            }

            const BindingFlags all = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                                     | BindingFlags.Static | BindingFlags.DeclaredOnly;

            var declared = TypesOf(CoreNamespace)
                .Concat(TypesOf(UnitsNamespace))
                .SelectMany(type => new[] { type.Name }
                    .Concat(type.IsEnum ? Enum.GetNames(type) : Array.Empty<string>())
                    .Concat(type.GetMembers(all).Select(member => member.Name)))
                .ToList();

            Assert.True(declared.Count > 100, "La reflexión apenas encontró declaraciones: " + declared.Count);

            foreach (var concept in DimensionalConcepts)
            {
                Assert.DoesNotContain(declared, name => name.Contains(concept, StringComparison.Ordinal));
            }
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
