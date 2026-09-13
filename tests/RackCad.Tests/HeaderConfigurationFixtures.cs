using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.RackFrames;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-53 G3 — fixtures ESTRICTAMENTE COMUNES del nucleo compartido de ID6 REUSE + ID7 BATCH DISTRIBUTION.
    ///
    /// <para>
    /// No conocen ningun sistema: una cabecera rica construida por la fabrica real, sus huellas comparables y un
    /// recorrido del grafo por IDENTIDAD de referencias. El recorrido es lo que hace no trivial la independencia (I1-I6):
    /// una copia superficial comparte al menos una lista u objeto, y el recorrido lo encuentra aunque la propiedad se
    /// agregue manana, porque no enumera propiedades a mano.
    /// </para>
    /// </summary>
    internal static class HeaderConfigurationFixtures
    {
        /// <summary>
        /// Una cabecera deliberadamente NO por defecto: escalares, ambos postes con refuerzo, ambas placas con peralte
        /// propio, horizontales y paneles modificados, derivado construido y Exceptions (estado runtime que el documento
        /// no persiste) no vacias.
        /// </summary>
        internal static RackFrameConfiguration Rich()
        {
            var configuration = new RackFrameConfigurationFactory(JsonRackCatalogProvider.FromBaseDirectory().Load())
                .Build(RackFrameTemplateCatalog.Default, "POSTE_OMEGA_3X3", 132.0, 42.0);

            configuration.Name = "Cabecera I-53";
            configuration.PostPeralte = 3.25;
            configuration.CelosiaStartTroquel = 5;
            configuration.DiagonalStartOffsetTroqueles = 1;
            configuration.DiagonalEndOffsetTroqueles = 3;
            configuration.DiagonalDoubleSpacingTroqueles = 4;
            configuration.HorizontalDoubleOffsetTroqueles = 2;
            configuration.PasoTroquel = 3.0;
            configuration.PanelClear = 50.0;
            configuration.StandardBaselineId = "BASE-53";
            configuration.StandardBaselineVersion = "v3";

            Assert.NotNull(configuration.LeftPost);
            Assert.NotNull(configuration.RightPost);
            Assert.NotNull(configuration.LeftBasePlate);
            Assert.NotNull(configuration.RightBasePlate);
            Assert.True(configuration.Horizontals.Count >= 3, "La fixture necesita al menos tres horizontales.");
            Assert.True(configuration.BracingPanels.Count >= 2, "La fixture necesita al menos dos paneles.");

            configuration.LeftPost.HasReinforcement = true;
            configuration.LeftPost.ReinforcementCatalogId = "POSTE_OMEGA_3X3";
            configuration.LeftPost.ReinforcementHeight = 60.0;
            configuration.RightPost.HasReinforcement = true;
            configuration.RightPost.ReinforcementCatalogId = "POSTE_OMEGA_3X3";
            configuration.RightPost.ReinforcementHeight = 48.0;
            configuration.LeftBasePlate.PeralteOverride = 7.5;
            configuration.RightBasePlate.PeralteOverride = 6.25;

            configuration.BracingPanels[0].Arrangement = BracingPattern.DoubleDiagonal;
            configuration.BracingPanels[1].Arrangement = BracingPattern.XBracing;
            configuration.BracingPanels[1].IsException = true;
            configuration.Horizontals[2].State = FrameComponentState.Manual;
            configuration.Horizontals[2].Notes = "ajuste manual";

            // El derivado, construido como lo tiene un editor cuando copia.
            new BracingPanelMemberBuilder().RefreshPhysicalModel(configuration);
            Assert.NotEmpty(configuration.Members);

            configuration.Exceptions.Add(new FrameExceptionOverride
            {
                ExceptionType = ExceptionType.PatternChange,
                TargetId = configuration.BracingPanels[1].PanelId,
                StandardValue = "SingleDiagonal",
                OverrideValue = "XBracing",
                Reason = "refuerzo por carga"
            });
            configuration.Exceptions.Add(new FrameExceptionOverride
            {
                ExceptionType = ExceptionType.ProfileChange,
                TargetId = configuration.Horizontals[2].Id,
                StandardValue = "PERFIL_A",
                OverrideValue = "PERFIL_B",
                Reason = "cambio de perfil manual"
            });

            return configuration;
        }

        /// <summary>La proyeccion persistida canonica (la del propio store), sin fijar detalles del JSON.</summary>
        internal static string Wire(RackFrameConfiguration configuration)
            => new RackFrameProjectStore().Serialize(configuration);

        /// <summary>Huella del estado runtime <see cref="RackFrameConfiguration.Exceptions"/>, en orden.</summary>
        internal static IReadOnlyList<string> ExceptionsFingerprint(RackFrameConfiguration configuration)
            => configuration.Exceptions
                .Select(exception => exception == null
                    ? "null"
                    : string.Join("|", exception.ExceptionType, exception.TargetId, exception.StandardValue,
                        exception.OverrideValue, exception.Reason))
                .ToList();

        /// <summary>Huella del derivado: miembros de primer nivel y, por panel, elevaciones y pertenencia.</summary>
        internal static IReadOnlyList<string> MembersFingerprint(RackFrameConfiguration configuration)
        {
            var lines = configuration.Members.Select(member => "M " + Member(member)).ToList();
            for (var i = 0; i < configuration.BracingPanels.Count; i++)
            {
                var panel = configuration.BracingPanels[i];
                lines.Add(string.Format(CultureInfo.InvariantCulture, "P{0} {1}-{2}", i, F(panel.StartElevation), F(panel.EndElevation)));
                lines.AddRange(panel.Members.Select(member => "P" + i.ToString(CultureInfo.InvariantCulture) + " " + Member(member)));
            }

            return lines;
        }

        private static string Member(FrameMember member)
            => string.Join("|", member.SourcePanelId, member.SourcePanelIndex.ToString(CultureInfo.InvariantCulture),
                member.MemberType, member.CatalogId, member.ProfileId, member.Quantity.ToString(CultureInfo.InvariantCulture),
                member.MountingFace, member.Origin, F(member.Length), F(member.Angle), member.IsStandard,
                End(member.Start), End(member.End));

        private static string End(FrameMemberEnd end)
            => end == null
                ? "null"
                : string.Join("/", end.Role, end.PostSide, F(end.HorizontalPositionRatio), F(end.Elevation), end.ConnectionPointId);

        private static string F(double value) => value.ToString("F6", CultureInfo.InvariantCulture);

        /// <summary>
        /// Todo objeto y toda coleccion alcanzable desde la configuracion, por IDENTIDAD. Las cadenas y los valores no
        /// cuentan: son inmutables, compartirlos no aliasa nada.
        /// </summary>
        internal static HashSet<object> MutableGraph(object root)
        {
            var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
            var pending = new Stack<object>();
            pending.Push(root);

            while (pending.Count > 0)
            {
                var node = pending.Pop();
                if (node == null || node is string || node.GetType().IsValueType || !visited.Add(node))
                {
                    continue;
                }

                if (node is IEnumerable sequence)
                {
                    foreach (var item in sequence)
                    {
                        pending.Push(item);
                    }

                    continue;
                }

                foreach (var property in node.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (property.CanRead && property.GetIndexParameters().Length == 0)
                    {
                        pending.Push(property.GetValue(node));
                    }
                }
            }

            return visited;
        }

        /// <summary>Falla nombrando el tipo de cada objeto o coleccion que las dos configuraciones comparten.</summary>
        internal static void AssertNoSharedMutableState(RackFrameConfiguration left, RackFrameConfiguration right)
        {
            Assert.NotNull(left);
            Assert.NotNull(right);
            var leftGraph = MutableGraph(left);
            var rightGraph = MutableGraph(right);

            // Guarda contra una prueba vacia: el grafo real de una cabecera rica es grande.
            Assert.True(leftGraph.Count >= 20, "Grafo sospechosamente pequeno: " + leftGraph.Count);
            Assert.True(rightGraph.Count >= 20, "Grafo sospechosamente pequeno: " + rightGraph.Count);

            var shared = leftGraph.Where(rightGraph.Contains).Select(node => node.GetType().Name).ToList();
            Assert.Empty(shared);
        }

        /// <summary>
        /// Muta TODAS las partes del grafo: escalares, postes, placas, listas authored y sus elementos, derivado y
        /// estado runtime. Si algo esta aliado a otra configuracion, alguna de estas escrituras lo delata.
        /// </summary>
        internal static void MutateEverywhere(RackFrameConfiguration configuration)
        {
            Assert.True(configuration.Horizontals.Count >= 3);
            Assert.True(configuration.BracingPanels.Count >= 2);
            Assert.True(configuration.Members.Count >= 2);
            Assert.NotEmpty(configuration.Exceptions);

            configuration.Name = "MUTADA";
            configuration.Height += 17.0;
            configuration.Depth += 3.0;
            configuration.PostPeralte = 9.5;
            configuration.PasoTroquel = 4.0;
            configuration.LeftPost.PostCatalogId = "MUTADO";
            configuration.LeftPost.ReinforcementHeight = 1.0;
            configuration.RightPost.HasReinforcement = !configuration.RightPost.HasReinforcement;
            configuration.LeftBasePlate.PeralteOverride = 99.0;
            configuration.RightBasePlate.PlateCatalogId = "MUTADA";
            configuration.Horizontals[0].Elevation += 5.0;
            configuration.Horizontals[1].Notes = "mutada";
            configuration.Horizontals.RemoveAt(configuration.Horizontals.Count - 1);
            configuration.BracingPanels[0].Arrangement = BracingPattern.NoBracing;
            configuration.BracingPanels[0].Members.Clear();
            configuration.BracingPanels.RemoveAt(configuration.BracingPanels.Count - 1);
            configuration.Members[0].Length = -1.0;
            configuration.Members.RemoveAt(configuration.Members.Count - 1);
            configuration.Exceptions[0].Reason = "MUTADA";
            configuration.Exceptions.Add(new FrameExceptionOverride { Reason = "extra" });
        }

        /// <summary>El valor del campo privado que guarda la copia privada del Snapshot (inspeccion de caja blanca).</summary>
        internal static RackFrameConfiguration PrivateCopyOf(HeaderConfigurationSnapshot snapshot)
        {
            Assert.NotNull(snapshot);
            var field = Assert.Single(
                typeof(HeaderConfigurationSnapshot).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public),
                candidate => candidate.FieldType == typeof(RackFrameConfiguration));
            return (RackFrameConfiguration)field.GetValue(snapshot);
        }
    }

    /// <summary>
    /// I-53 G3 — inspeccion por reflexion y por IL de los tipos nuevos del nucleo compartido. Sustituye a las guardas por
    /// TEXTO de fuente: lo que se comprueba es lo que el compilador emitio (firmas y referencias de los cuerpos), no lo
    /// que dice un comentario.
    /// </summary>
    internal static class SharedFoundationInspection
    {
        internal const string SharedNamespace = "RackCad.Application.Systems.Shared";

        internal const BindingFlags Declared =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        /// <summary>Las raices de los tipos de G3. Toda raiz nueva de G3 se agrega aqui (lo exige C-08).</summary>
        internal static readonly Type[] Roots =
        {
            typeof(HeaderConfigurationSnapshot),
            typeof(HeaderConfigurationCapture),
            typeof(HeaderCaptureFailure),
            typeof(HeaderOmissionReason),
            typeof(HeaderRejectionCode),
            typeof(HeaderWarningSeverity),
            typeof(HeaderOmission<>),
            typeof(HeaderBatchWarning<>),
            typeof(HeaderBatchSignature),
            typeof(HeaderBatchPlan<>),
            typeof(HeaderBatchOutcome<>),
        };

        private static readonly Dictionary<ushort, OpCode> OpCodesByValue = typeof(OpCodes)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Select(field => (OpCode)field.GetValue(null))
            .ToDictionary(opcode => unchecked((ushort)opcode.Value));

        /// <summary>Las raices y todos sus tipos anidados, incluidos los que genera el compilador.</summary>
        internal static IReadOnlyList<Type> TypesWithNested()
        {
            var result = new List<Type>();
            var pending = new Stack<Type>(Roots);
            while (pending.Count > 0)
            {
                var type = pending.Pop();
                if (result.Contains(type))
                {
                    continue;
                }

                result.Add(type);
                foreach (var nested in type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic))
                {
                    pending.Push(nested);
                }
            }

            return result;
        }

        internal static bool IsCompilerGenerated(MemberInfo member)
            => member.Name.IndexOf('<') >= 0
               || member.IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), inherit: false);

        /// <summary>Todo cuerpo declarado por el tipo: metodos, accesores, constructores e inicializador de tipo.</summary>
        internal static IEnumerable<MethodBase> DeclaredCode(Type type)
            => type.GetMethods(Declared).Cast<MethodBase>().Concat(type.GetConstructors(Declared)).Distinct();

        /// <summary>
        /// Los miembros y tipos que referencia el IL de un cuerpo (call, newobj, ldfld, ldtoken...). Lector minimo de IL:
        /// decodifica cada opcode para saltar su operando exacto y resuelve solo los tokens de tipo, campo y metodo.
        /// </summary>
        internal static IReadOnlyList<MemberInfo> ReferencedMembers(MethodBase method)
        {
            var found = new List<MemberInfo>();
            var body = method.GetMethodBody();
            if (body == null)
            {
                return found;
            }

            var il = body.GetILAsByteArray();
            var typeArguments = method.DeclaringType != null && method.DeclaringType.IsGenericType
                ? method.DeclaringType.GetGenericArguments()
                : Type.EmptyTypes;
            var methodArguments = method.IsGenericMethod ? method.GetGenericArguments() : Type.EmptyTypes;

            var position = 0;
            while (position < il.Length)
            {
                ushort value = il[position++];
                if (value == 0xFE)
                {
                    value = (ushort)(0xFE00 | il[position++]);
                }

                if (!OpCodesByValue.TryGetValue(value, out var opcode))
                {
                    throw new InvalidOperationException(
                        "Opcode IL desconocido 0x" + value.ToString("X", CultureInfo.InvariantCulture) + " en " + method);
                }

                switch (opcode.OperandType)
                {
                    case OperandType.InlineNone:
                        break;
                    case OperandType.ShortInlineBrTarget:
                    case OperandType.ShortInlineI:
                    case OperandType.ShortInlineVar:
                        position += 1;
                        break;
                    case OperandType.InlineVar:
                        position += 2;
                        break;
                    case OperandType.InlineBrTarget:
                    case OperandType.InlineI:
                    case OperandType.InlineSig:
                    case OperandType.InlineString:
                    case OperandType.ShortInlineR:
                        position += 4;
                        break;
                    case OperandType.InlineI8:
                    case OperandType.InlineR:
                        position += 8;
                        break;
                    case OperandType.InlineSwitch:
                        position += 4 + 4 * BitConverter.ToInt32(il, position);
                        break;
                    case OperandType.InlineField:
                    case OperandType.InlineMethod:
                    case OperandType.InlineTok:
                    case OperandType.InlineType:
                        found.Add(method.Module.ResolveMember(BitConverter.ToInt32(il, position), typeArguments, methodArguments));
                        position += 4;
                        break;
                    default:
                        throw new InvalidOperationException("Operando IL no soportado: " + opcode.OperandType);
                }
            }

            return found;
        }

        /// <summary>Todos los miembros que referencia el IL de un tipo (sin sus anidados).</summary>
        internal static IReadOnlyList<MemberInfo> ReferencedMembers(Type type)
            => DeclaredCode(type).SelectMany(ReferencedMembers).ToList();

        /// <summary>Los tipos que un tipo toca: su firma completa y lo que referencian sus cuerpos.</summary>
        internal static IReadOnlyList<Type> ReferencedTypes(Type type)
        {
            var roots = new List<Type> { type.BaseType };
            roots.AddRange(type.GetInterfaces());
            if (type.IsGenericTypeDefinition)
            {
                roots.AddRange(type.GetGenericArguments().SelectMany(argument => argument.GetGenericParameterConstraints()));
            }

            roots.AddRange(type.GetFields(Declared).Select(field => field.FieldType));
            roots.AddRange(type.GetProperties(Declared).Select(property => property.PropertyType));
            roots.AddRange(type.GetEvents(Declared).Select(@event => @event.EventHandlerType));
            foreach (var code in DeclaredCode(type))
            {
                roots.AddRange(code.GetParameters().Select(parameter => parameter.ParameterType));
                if (code is MethodInfo method)
                {
                    roots.Add(method.ReturnType);
                }
            }

            roots.AddRange(type.GetCustomAttributesData().Select(attribute => attribute.AttributeType));
            roots.AddRange(ReferencedMembers(type).SelectMany(TypesOf));
            return Expand(roots);
        }

        private static IEnumerable<Type> TypesOf(MemberInfo member)
        {
            switch (member)
            {
                case Type type:
                    return new[] { type };
                case FieldInfo field:
                    return new[] { field.DeclaringType, field.FieldType };
                case MethodBase method:
                    var types = new List<Type> { method.DeclaringType };
                    types.AddRange(method.GetParameters().Select(parameter => parameter.ParameterType));
                    if (method is MethodInfo info)
                    {
                        types.Add(info.ReturnType);
                        if (info.IsGenericMethod)
                        {
                            types.AddRange(info.GetGenericArguments());
                        }
                    }

                    return types;
                default:
                    return new[] { member.DeclaringType };
            }
        }

        /// <summary>Desenvuelve arreglos, referencias, punteros y argumentos genericos; los parametros genericos no son tipos.</summary>
        private static IReadOnlyList<Type> Expand(IEnumerable<Type> roots)
        {
            var result = new List<Type>();
            var seen = new HashSet<Type>();
            var pending = new Stack<Type>(roots.Where(root => root != null));
            while (pending.Count > 0)
            {
                var type = pending.Pop();
                if (type == null || !seen.Add(type))
                {
                    continue;
                }

                if (type.HasElementType)
                {
                    pending.Push(type.GetElementType());
                    continue;
                }

                if (type.IsGenericParameter)
                {
                    foreach (var constraint in type.GetGenericParameterConstraints())
                    {
                        pending.Push(constraint);
                    }

                    continue;
                }

                result.Add(type);
                if (type.IsGenericType)
                {
                    foreach (var argument in type.GetGenericArguments())
                    {
                        pending.Push(argument);
                    }
                }
            }

            return result;
        }

        /// <summary>Las propiedades publicas de instancia que DECLARA un tipo, por nombre.</summary>
        internal static IReadOnlyList<string> PublicInstanceProperties(Type type)
            => type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(property => property.Name)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToList();

        /// <summary>Las formas anidadas que derivan de una base cerrada, por nombre.</summary>
        internal static IReadOnlyList<string> VariantsOf(Type closedBase)
            => closedBase.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic)
                .Where(nested => !IsCompilerGenerated(nested) && Derives(nested, closedBase))
                .Select(nested => nested.Name)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToList();

        private static bool Derives(Type candidate, Type closedBase)
        {
            for (var current = candidate.BaseType; current != null; current = current.BaseType)
            {
                var definition = current.IsGenericType ? current.GetGenericTypeDefinition() : current;
                if (definition == closedBase)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Una base es cerrada si ninguno de sus constructores es accesible fuera de ella.</summary>
        internal static void AssertClosedBase(Type closedBase)
        {
            Assert.True(closedBase.IsAbstract, closedBase.Name + " debe ser abstracta.");
            var constructors = closedBase.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.NotEmpty(constructors);
            Assert.All(constructors, constructor => Assert.True(constructor.IsPrivate, closedBase.Name + " expone un constructor no privado."));
            Assert.All(
                closedBase.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic).Where(nested => Derives(nested, closedBase)),
                variant => Assert.True(variant.IsSealed, variant.Name + " debe ser sealed."));
        }
    }
}
