using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.RackFrames;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-53 (ID6 REUSE + ID7 BATCH DISTRIBUTION) G3 — la FORMA compartida de la operacion: Plan, Outcome, firma, motivos
    /// Omitted y codigos Rejected. Pruebas C-05, C-06, C-07, C-08 y C-10 de la matriz congelada (Proposal V2 §13.2;
    /// contrato §3.4-§3.6, §3.9, §3.13).
    ///
    /// <para>
    /// La direccion de estas pruebas es un tipo NEUTRAL de prueba: el nucleo es generico sobre la direccion de cada
    /// sistema y no conoce fondos, postes ni modulos. Aqui no hay resolucion de destinos, ni precedencia, ni PREPARE o
    /// MUTATE: eso lo implementa cada sistema en G4/G6. Lo que se fija es que el PLAN de preparacion y el OUTCOME final
    /// son dos cosas separadas, cerradas e inmutables, y que <c>Applied</c> solo puede ser los Targets preparados.
    /// </para>
    /// </summary>
    public class HeaderBatchContractTests
    {
        /// <summary>Una direccion de prueba sin significado de sistema; por referencia, para poder exigir identidad.</summary>
        private sealed class Direccion
        {
            public Direccion(string clave)
            {
                Clave = clave;
            }

            public string Clave { get; }

            public override string ToString() => Clave;
        }

        private static readonly Direccion A = new Direccion("A");
        private static readonly Direccion B = new Direccion("B");
        private static readonly Direccion C = new Direccion("C");
        private static readonly Direccion D = new Direccion("D");
        private static readonly Direccion E = new Direccion("E");
        private static readonly Direccion F = new Direccion("F");

        private static readonly HeaderRejectionCode[] SevenCodes =
        {
            HeaderRejectionCode.StaleTargets,
            HeaderRejectionCode.SourceNotFound,
            HeaderRejectionCode.SourceUnusable,
            HeaderRejectionCode.NoTargets,
            HeaderRejectionCode.MalformedTarget,
            HeaderRejectionCode.NoApplicableTargets,
            HeaderRejectionCode.DestinationInvalid,
        };

        public static IEnumerable<object[]> AllRejectionCodes() => SevenCodes.Select(code => new object[] { code });

        private static HeaderBatchSignature Firma(params string[] components) => new HeaderBatchSignature(components);

        private static HeaderOmission<Direccion> Omision(Direccion address, HeaderOmissionReason reason)
            => new HeaderOmission<Direccion>(address, reason);

        private static HeaderBatchWarning<Direccion> Aviso(Direccion address, HeaderWarningSeverity severity, string message)
            => new HeaderBatchWarning<Direccion>(address, severity, message);

        private static HeaderBatchPlan<Direccion>.Prepared Preparado(params HeaderBatchWarning<Direccion>[] warnings)
            => new HeaderBatchPlan<Direccion>.Prepared(
                new[] { C, A, B },
                new[] { Omision(D, HeaderOmissionReason.AbsentInScope), Omision(E, HeaderOmissionReason.IsSource) },
                warnings,
                Firma("topologia", "generacion=7"));

        private static void AssertSameSequence<T>(IReadOnlyList<T> expected, IReadOnlyList<T> actual)
            where T : class
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.Count, actual.Count);
            for (var i = 0; i < expected.Count; i++)
            {
                Assert.Same(expected[i], actual[i]);
            }
        }

        // ===== C-05 — Plan: Rejected | Prepared, sin Applied ============================================================

        [Theory]
        [MemberData(nameof(AllRejectionCodes))]
        public void C05_UN_PLAN_RECHAZADO_LLEVA_EXACTAMENTE_SU_CODIGO(HeaderRejectionCode code)
        {
            HeaderBatchPlan<Direccion> plan = new HeaderBatchPlan<Direccion>.Rejected(code);

            var rejected = Assert.IsType<HeaderBatchPlan<Direccion>.Rejected>(plan);
            Assert.Equal(code, rejected.Code);
        }

        [Fact]
        public void C05_UN_PLAN_PREPARADO_CONSERVA_TARGETS_OMITIDOS_AVISOS_Y_FIRMA_EN_EL_ORDEN_DEL_CONSUMIDOR()
        {
            // Orden deliberadamente NO alfabetico: el nucleo no reordena; el orden determinista lo decide el consumidor.
            var targets = new[] { C, A, B };
            var omitted = new[]
            {
                Omision(F, HeaderOmissionReason.NotPhysicallyPresent),
                Omision(D, HeaderOmissionReason.AbsentInScope),
                Omision(E, HeaderOmissionReason.IsSource),
            };
            var warnings = new[]
            {
                Aviso(B, HeaderWarningSeverity.Informative, "altura por encima de la resuelta"),
                Aviso(A, HeaderWarningSeverity.Severe, "altura por debajo del nivel superior"),
            };

            var plan = new HeaderBatchPlan<Direccion>.Prepared(targets, omitted, warnings, Firma("topologia", "generacion=7"));

            AssertSameSequence(targets, plan.Targets);
            AssertSameSequence(omitted, plan.Omitted);
            AssertSameSequence(warnings, plan.Warnings);
            Assert.Equal(new[] { HeaderOmissionReason.NotPhysicallyPresent, HeaderOmissionReason.AbsentInScope, HeaderOmissionReason.IsSource },
                plan.Omitted.Select(omission => omission.Reason));
            Assert.Equal(new[] { F, D, E }, plan.Omitted.Select(omission => omission.Address));
            Assert.Equal(new[] { B, A }, plan.Warnings.Select(warning => warning.Address));
            Assert.Equal(new[] { HeaderWarningSeverity.Informative, HeaderWarningSeverity.Severe }, plan.Warnings.Select(warning => warning.Severity));
            Assert.Equal("altura por debajo del nivel superior", plan.Warnings[1].Message);
            Assert.Equal(Firma("topologia", "generacion=7"), plan.Signature);
        }

        [Fact]
        public void C05_UN_PLAN_PREPARADO_ES_INMUTABLE_Y_NO_DEPENDE_DE_LAS_LISTAS_DEL_CONSUMIDOR()
        {
            var targets = new List<Direccion> { A, B };
            var omitted = new List<HeaderOmission<Direccion>> { Omision(C, HeaderOmissionReason.IsSource) };
            var warnings = new List<HeaderBatchWarning<Direccion>> { Aviso(A, HeaderWarningSeverity.Informative, "aviso") };

            var plan = new HeaderBatchPlan<Direccion>.Prepared(targets, omitted, warnings, Firma("t"));

            // El consumidor sigue tocando SUS listas: el plan de un gesto no se mueve.
            targets.Add(C);
            targets[0] = D;
            omitted.Clear();
            warnings.Add(Aviso(B, HeaderWarningSeverity.Severe, "tarde"));

            AssertSameSequence(new[] { A, B }, plan.Targets);
            Assert.Single(plan.Omitted);
            Assert.Single(plan.Warnings);
            Assert.False(plan.RequiresConfirmation);

            // Y nadie puede escribir a traves del plan, ni con un cast.
            var writableTargets = Assert.IsAssignableFrom<IList<Direccion>>(plan.Targets);
            Assert.Throws<NotSupportedException>(() => writableTargets.Add(E));
            Assert.Throws<NotSupportedException>(() => writableTargets[0] = E);
            var writableOmitted = Assert.IsAssignableFrom<IList<HeaderOmission<Direccion>>>(plan.Omitted);
            Assert.Throws<NotSupportedException>(() => writableOmitted.Clear());
            var writableWarnings = Assert.IsAssignableFrom<IList<HeaderBatchWarning<Direccion>>>(plan.Warnings);
            Assert.Throws<NotSupportedException>(() => writableWarnings.RemoveAt(0));
            AssertSameSequence(new[] { A, B }, plan.Targets);
        }

        [Fact]
        public void C05_LA_CONFIRMACION_SE_EXIGE_SI_Y_SOLO_SI_HAY_UN_AVISO_SEVERO()
        {
            Assert.False(Preparado().RequiresConfirmation);
            Assert.False(Preparado(Aviso(A, HeaderWarningSeverity.Informative, "uno"), Aviso(B, HeaderWarningSeverity.Informative, "dos")).RequiresConfirmation);
            Assert.True(Preparado(Aviso(A, HeaderWarningSeverity.Severe, "uno")).RequiresConfirmation);
            Assert.True(Preparado(Aviso(A, HeaderWarningSeverity.Informative, "uno"), Aviso(C, HeaderWarningSeverity.Severe, "dos")).RequiresConfirmation);
        }

        [Fact]
        public void C05_UNA_DIRECCION_DE_VALOR_IGUAL_A_SU_DEFAULT_ES_UN_DESTINO_VALIDO()
        {
            // Una direccion de un sistema puede ser un valor cuyo default es una posicion real (el indice 0): el nucleo no
            // debe confundirla con "sin direccion".
            var plan = new HeaderBatchPlan<int>.Prepared(
                new[] { 0, 2 },
                new[] { new HeaderOmission<int>(0, HeaderOmissionReason.IsSource) },
                new[] { new HeaderBatchWarning<int>(0, HeaderWarningSeverity.Informative, "aviso en 0") },
                Firma("t"));

            Assert.Equal(new[] { 0, 2 }, plan.Targets);
            Assert.Equal(0, Assert.Single(plan.Omitted).Address);
            Assert.Equal(0, Assert.Single(plan.Warnings).Address);
        }

        [Fact]
        public void C05_UN_PLAN_PREPARADO_RECHAZA_ENTRADAS_DEFECTUOSAS_EN_VEZ_DE_NACER_INCOHERENTE()
        {
            var omitted = new[] { Omision(D, HeaderOmissionReason.AbsentInScope) };
            var warnings = new HeaderBatchWarning<Direccion>[0];
            var signature = Firma("t");

            Assert.Throws<ArgumentNullException>(() => new HeaderBatchPlan<Direccion>.Prepared(null, omitted, warnings, signature));
            Assert.Throws<ArgumentNullException>(() => new HeaderBatchPlan<Direccion>.Prepared(new[] { A }, null, warnings, signature));
            Assert.Throws<ArgumentNullException>(() => new HeaderBatchPlan<Direccion>.Prepared(new[] { A }, omitted, null, signature));
            Assert.Throws<ArgumentNullException>(() => new HeaderBatchPlan<Direccion>.Prepared(new[] { A }, omitted, warnings, null));

            // Cero destinos aplicables es un rechazo (NoTargets o NoApplicableTargets), nunca un plan preparado.
            Assert.Throws<ArgumentException>(() => new HeaderBatchPlan<Direccion>.Prepared(new Direccion[0], omitted, warnings, signature));

            Assert.Throws<ArgumentException>(() => new HeaderBatchPlan<Direccion>.Prepared(new[] { A, null }, omitted, warnings, signature));
            Assert.Throws<ArgumentException>(() => new HeaderBatchPlan<Direccion>.Prepared(new[] { A }, new HeaderOmission<Direccion>[] { null }, warnings, signature));
            Assert.Throws<ArgumentException>(() => new HeaderBatchPlan<Direccion>.Prepared(new[] { A }, omitted, new HeaderBatchWarning<Direccion>[] { null }, signature));

            Assert.Throws<ArgumentNullException>(() => new HeaderOmission<Direccion>(null, HeaderOmissionReason.IsSource));
            Assert.Throws<ArgumentNullException>(() => new HeaderBatchWarning<Direccion>(null, HeaderWarningSeverity.Severe, "aviso"));
            Assert.Throws<ArgumentException>(() => new HeaderBatchWarning<Direccion>(A, HeaderWarningSeverity.Severe, " "));
            Assert.Throws<ArgumentException>(() => new HeaderBatchWarning<Direccion>(A, HeaderWarningSeverity.Severe, null));
        }

        [Fact]
        public void C05_LA_FIRMA_ES_UN_VALOR_OPACO_INMUTABLE_COMPARADO_POR_IGUALDAD_ORDINAL()
        {
            var components = new[] { "fondos=3", "generacion=7" };
            var first = Firma(components);
            var second = Firma("fondos=3", "generacion=7");

            Assert.True(first.Equals(second));
            Assert.True(first.Equals((object)second));
            Assert.True(first == second);
            Assert.False(first != second);
            Assert.Equal(first.GetHashCode(), second.GetHashCode());

            // El consumidor que reutiliza su arreglo no mueve una firma ya tomada.
            components[1] = "generacion=8";
            Assert.True(first == second);

            Assert.False(first == Firma("generacion=7", "fondos=3"));   // el orden cuenta
            Assert.False(first == Firma("fondos=3", "generacion=8"));   // un componente distinto
            Assert.False(first == Firma("fondos=3"));                   // uno de menos
            Assert.False(first == Firma("FONDOS=3", "generacion=7"));   // comparacion ordinal
            Assert.False(Firma("ab", "c") == Firma("a", "bc"));         // los componentes no se concatenan
            Assert.True(first != Firma("fondos=3", "generacion=8"));
            Assert.False(first.Equals(null));
            Assert.False(first == null);
            Assert.True((HeaderBatchSignature)null == null);

            // Opaca: no entrega sus componentes.
            Assert.Empty(SharedFoundationInspection.PublicInstanceProperties(typeof(HeaderBatchSignature)));
        }

        [Fact]
        public void C05_LA_FIRMA_RECHAZA_COMPONENTES_DEFECTUOSOS()
        {
            Assert.Throws<ArgumentNullException>(() => new HeaderBatchSignature(null));
            Assert.Throws<ArgumentException>(() => new HeaderBatchSignature(new string[0]));
            Assert.Throws<ArgumentException>(() => new HeaderBatchSignature(new[] { "t", null }));
        }

        [Fact]
        public void C05_GUARDA_EL_PLAN_TIENE_EXACTAMENTE_DOS_FORMAS_Y_NINGUNA_LLEVA_APPLIED()
        {
            SharedFoundationInspection.AssertClosedBase(typeof(HeaderBatchPlan<>));
            Assert.Equal(new[] { "Prepared", "Rejected" }, SharedFoundationInspection.VariantsOf(typeof(HeaderBatchPlan<>)));

            // Rejected no lleva destinos; Prepared lleva lo del contrato y nada mas: ningun Applied.
            Assert.Equal(new[] { "Code" }, SharedFoundationInspection.PublicInstanceProperties(typeof(HeaderBatchPlan<>.Rejected)));
            Assert.Equal(
                new[] { "Omitted", "RequiresConfirmation", "Signature", "Targets", "Warnings" },
                SharedFoundationInspection.PublicInstanceProperties(typeof(HeaderBatchPlan<>.Prepared)));

            foreach (var type in new[] { typeof(HeaderBatchPlan<>), typeof(HeaderBatchPlan<>.Rejected), typeof(HeaderBatchPlan<>.Prepared) })
            {
                Assert.DoesNotContain(
                    type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static),
                    member => member.Name.IndexOf("Applied", StringComparison.OrdinalIgnoreCase) >= 0);
            }

            // Inmutable por construccion: ninguna propiedad con setter en el plan, sus entradas ni la firma.
            foreach (var type in new[]
                     {
                         typeof(HeaderBatchPlan<>.Rejected), typeof(HeaderBatchPlan<>.Prepared), typeof(HeaderOmission<>),
                         typeof(HeaderBatchWarning<>), typeof(HeaderBatchSignature),
                     })
            {
                Assert.All(
                    type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance),
                    property => Assert.Null(property.GetSetMethod(nonPublic: true)));
            }
        }

        // ===== C-06 — Outcome: Rejected | Cancelled | Committed(Applied = Targets) ======================================

        [Fact]
        public void C06_COMMITTED_APLICA_EXACTAMENTE_LOS_TARGETS_PREPARADOS_EN_EL_MISMO_ORDEN_Y_CONSERVA_LOS_OMITIDOS()
        {
            var plan = Preparado(Aviso(B, HeaderWarningSeverity.Informative, "aviso"));

            HeaderBatchOutcome<Direccion> outcome = new HeaderBatchOutcome<Direccion>.Committed(plan);

            var committed = Assert.IsType<HeaderBatchOutcome<Direccion>.Committed>(outcome);
            AssertSameSequence(new[] { C, A, B }, committed.Applied);
            AssertSameSequence(plan.Targets, committed.Applied);
            AssertSameSequence(plan.Omitted, committed.Omitted);
        }

        [Theory]
        [MemberData(nameof(AllRejectionCodes))]
        public void C06_UN_OUTCOME_RECHAZADO_LLEVA_EXACTAMENTE_SU_CODIGO(HeaderRejectionCode code)
        {
            HeaderBatchOutcome<Direccion> outcome = new HeaderBatchOutcome<Direccion>.Rejected(code);

            var rejected = Assert.IsType<HeaderBatchOutcome<Direccion>.Rejected>(outcome);
            Assert.Equal(code, rejected.Code);
        }

        [Fact]
        public void C06_CANCELLED_NO_ES_REJECTED_Y_SOLO_EXISTE_TRAS_UN_PLAN_QUE_EXIGIA_CONFIRMACION()
        {
            var severe = Preparado(Aviso(A, HeaderWarningSeverity.Severe, "altura severa"));

            HeaderBatchOutcome<Direccion> outcome = new HeaderBatchOutcome<Direccion>.Cancelled(severe);
            Assert.IsType<HeaderBatchOutcome<Direccion>.Cancelled>(outcome);
            Assert.IsNotType<HeaderBatchOutcome<Direccion>.Rejected>(outcome);

            // CONFIRM se pide si y solo si hay un aviso severo: sin el, no hay a quien cancelar.
            Assert.Throws<InvalidOperationException>(() => new HeaderBatchOutcome<Direccion>.Cancelled(Preparado()));
            Assert.Throws<InvalidOperationException>(() =>
                new HeaderBatchOutcome<Direccion>.Cancelled(Preparado(Aviso(A, HeaderWarningSeverity.Informative, "solo informa"))));

            Assert.Throws<ArgumentNullException>(() => new HeaderBatchOutcome<Direccion>.Cancelled(null));
            Assert.Throws<ArgumentNullException>(() => new HeaderBatchOutcome<Direccion>.Committed(null));
        }

        [Fact]
        public void C06_GUARDA_EL_OUTCOME_TIENE_EXACTAMENTE_TRES_FORMAS_Y_SOLO_COMMITTED_LLEVA_APPLIED()
        {
            SharedFoundationInspection.AssertClosedBase(typeof(HeaderBatchOutcome<>));
            Assert.Equal(
                new[] { "Cancelled", "Committed", "Rejected" },
                SharedFoundationInspection.VariantsOf(typeof(HeaderBatchOutcome<>)));

            Assert.Equal(new[] { "Code" }, SharedFoundationInspection.PublicInstanceProperties(typeof(HeaderBatchOutcome<>.Rejected)));
            Assert.Empty(SharedFoundationInspection.PublicInstanceProperties(typeof(HeaderBatchOutcome<>.Cancelled)));
            Assert.Equal(new[] { "Applied", "Omitted" }, SharedFoundationInspection.PublicInstanceProperties(typeof(HeaderBatchOutcome<>.Committed)));

            // Cancelled y Rejected son formas distintas, no una la otra.
            Assert.False(typeof(HeaderBatchOutcome<>.Rejected).IsAssignableFrom(typeof(HeaderBatchOutcome<>.Cancelled)));
            Assert.False(typeof(HeaderBatchOutcome<>.Cancelled).IsAssignableFrom(typeof(HeaderBatchOutcome<>.Rejected)));

            // Committed solo nace de un plan preparado: no hay forma de construirlo con una lista Applied propia.
            var committedConstructor = Assert.Single(typeof(HeaderBatchOutcome<>.Committed).GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
            var parameter = Assert.Single(committedConstructor.GetParameters());
            Assert.Equal("Prepared", parameter.ParameterType.Name);
            Assert.Equal(typeof(HeaderBatchPlan<>), parameter.ParameterType.DeclaringType);

            Assert.All(
                typeof(HeaderBatchOutcome<>.Committed).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance),
                property => Assert.Null(property.GetSetMethod(nonPublic: true)));
        }

        // ===== C-07 — conjuntos cerrados ================================================================================

        [Fact]
        public void C07_GUARDA_LOS_MOTIVOS_OMITTED_Y_LOS_CODIGOS_REJECTED_SON_CONJUNTOS_CERRADOS()
        {
            AssertClosedEnum<HeaderOmissionReason>("AbsentInScope", "NotPhysicallyPresent", "IsSource");
            AssertClosedEnum<HeaderRejectionCode>(
                "StaleTargets", "SourceNotFound", "SourceUnusable", "NoTargets", "MalformedTarget", "NoApplicableTargets", "DestinationInvalid");
            AssertClosedEnum<HeaderWarningSeverity>("Informative", "Severe");
            AssertClosedEnum<HeaderCaptureFailure>("NullSource", "UnusableHeader");
        }

        private static void AssertClosedEnum<TEnum>(params string[] expected)
            where TEnum : struct, Enum
        {
            // Exactamente estos nombres: una adicion futura (StageInert, Clamped, Created, Unknown, Other...) rompe aqui.
            Assert.Equal(expected.OrderBy(name => name, StringComparer.Ordinal), Enum.GetNames<TEnum>().OrderBy(name => name, StringComparer.Ordinal));

            var values = Enum.GetValues<TEnum>().Select(value => Convert.ToInt64(value)).ToList();
            Assert.Equal(values.Count, values.Distinct().Count());
            Assert.DoesNotContain(0L, values); // default(TEnum) no es un miembro: un valor sin inicializar no pasa por valido

            foreach (var forbidden in new[] { "StageInert", "Clamped", "Created", "Unknown", "Other", "None", "Generic", "Error" })
            {
                Assert.DoesNotContain(forbidden, Enum.GetNames<TEnum>());
            }
        }

        [Fact]
        public void C07_UN_VALOR_NUMERICO_FUERA_DEL_CONJUNTO_NO_ENTRA_EN_UN_PLAN_UN_OUTCOME_NI_UNA_CAPTURA()
        {
            foreach (var code in new[] { (HeaderRejectionCode)0, (HeaderRejectionCode)8, (HeaderRejectionCode)(-1) })
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => new HeaderBatchPlan<Direccion>.Rejected(code));
                Assert.Throws<ArgumentOutOfRangeException>(() => new HeaderBatchOutcome<Direccion>.Rejected(code));
            }

            Assert.Throws<ArgumentOutOfRangeException>(() => new HeaderOmission<Direccion>(A, (HeaderOmissionReason)0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new HeaderOmission<Direccion>(A, (HeaderOmissionReason)4));
            Assert.Throws<ArgumentOutOfRangeException>(() => new HeaderBatchWarning<Direccion>(A, (HeaderWarningSeverity)0, "aviso"));
            Assert.Throws<ArgumentOutOfRangeException>(() => new HeaderBatchWarning<Direccion>(A, (HeaderWarningSeverity)3, "aviso"));
            Assert.Throws<ArgumentOutOfRangeException>(() => new HeaderConfigurationCapture.Failed((HeaderCaptureFailure)0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new HeaderConfigurationCapture.Failed((HeaderCaptureFailure)3));
        }

        // ===== C-08 — neutralidad =======================================================================================

        private static string ScannerProbe(string text) => string.Concat(text, text);

        [Fact]
        public void C08_GUARDA_LOS_TIPOS_NUEVOS_NO_REFERENCIAN_UI_AUTOCAD_BOM_DIBUJO_DTO_NI_TAXONOMIA_DE_SISTEMA()
        {
            // Premisa: el lector de IL ve de verdad las referencias de un cuerpo (sin esto, el resto seria un verde vacio).
            var probe = typeof(HeaderBatchContractTests).GetMethod(nameof(ScannerProbe), BindingFlags.NonPublic | BindingFlags.Static);
            Assert.Contains(SharedFoundationInspection.ReferencedMembers(probe), member => member.DeclaringType == typeof(string) && member.Name == nameof(string.Concat));

            var forbiddenNamespaces = new[]
            {
                "Autodesk", "System.Windows", "System.Xaml", "Microsoft.Win32", "RackCad.UI", "RackCad.Plugin",
                "RackCad.Application.Systems.Selective", "RackCad.Application.Systems.Dynamic", "RackCad.Application.Systems.PushBack",
                "RackCad.Application.Systems.FlowBed", "RackCad.Application.Systems.Larguero", "RackCad.Application.Systems.Cantilever",
                "RackCad.Domain.Systems", "RackCad.Application.Bom", "RackCad.Application.Drawing", "RackCad.Application.Layout",
                "System.Text.Json",
            };
            var forbiddenTypeNames = new[] { "ObjectId", "Database", "Transaction", "BlockReference", "Window", "Control" };
            var allowedPersistence = new[] { typeof(RackFrameProjectStore), typeof(RackDesignValidation) };

            var offenders = new List<string>();
            foreach (var type in SharedFoundationInspection.TypesWithNested())
            {
                foreach (var referenced in SharedFoundationInspection.ReferencedTypes(type))
                {
                    var ns = referenced.Namespace ?? string.Empty;
                    if (forbiddenNamespaces.Any(prefix => ns == prefix || ns.StartsWith(prefix + ".", StringComparison.Ordinal)))
                    {
                        offenders.Add(type.Name + " -> " + referenced.FullName);
                    }
                    else if (forbiddenTypeNames.Contains(referenced.Name))
                    {
                        offenders.Add(type.Name + " -> " + referenced.FullName);
                    }
                    else if (ns == typeof(RackFrameProjectStore).Namespace && !allowedPersistence.Contains(referenced))
                    {
                        offenders.Add(type.Name + " -> " + referenced.FullName + " (persistencia: solo DeepCopy y la validacion)");
                    }
                    else if (ns == typeof(RackFrameConfiguration).Namespace && referenced != typeof(RackFrameConfiguration))
                    {
                        offenders.Add(type.Name + " -> " + referenced.FullName + " (grafo authored: solo RackFrameConfiguration)");
                    }
                }

                // Ni nombres de la taxonomia de ningun sistema en lo que declaran los tipos nuevos.
                var names = type.GetMembers(SharedFoundationInspection.Declared)
                    .Where(member => !SharedFoundationInspection.IsCompilerGenerated(member))
                    .Select(member => member.Name)
                    .Concat(new[] { type.Name })
                    .Concat(type.GetConstructors(SharedFoundationInspection.Declared).SelectMany(constructor => constructor.GetParameters()).Select(parameter => parameter.Name))
                    .Concat(type.GetMethods(SharedFoundationInspection.Declared).SelectMany(method => method.GetParameters()).Select(parameter => parameter.Name));
                foreach (var name in names.Where(name => name != null))
                {
                    if (new[] { "Fondo", "Frente", "Poste", "PostIndex", "PostTarget", "Module", "PushBack", "Selectiv", "Dynamic", "Dinamic", "Peralte", "Bom", "Drawing", "Line" }
                        .Any(fragment => name.IndexOf(fragment, StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        offenders.Add(type.Name + " declara '" + name + "'");
                    }
                }
            }

            Assert.Empty(offenders);
        }

        [Fact]
        public void C08_GUARDA_EL_ENSAMBLADO_DEL_NUCLEO_NO_REFERENCIA_WPF_NI_AUTOCAD()
        {
            var references = typeof(HeaderConfigurationSnapshot).Assembly.GetReferencedAssemblies().Select(reference => reference.Name).ToList();
            Assert.NotEmpty(references);

            var forbidden = new[]
            {
                "Autodesk", "AcMgd", "AcDbMgd", "AcCoreMgd", "PresentationFramework", "PresentationCore", "WindowsBase", "System.Xaml",
                "System.Windows.Forms", "RackCad.UI", "RackCad.Plugin",
            };
            Assert.DoesNotContain(references, name => forbidden.Any(prefix => name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)));
        }

        [Fact]
        public void C08_GUARDA_TODO_TIPO_HEADER_DEL_NAMESPACE_SHARED_ESTA_BAJO_ESTAS_GUARDAS()
        {
            // Un tipo nuevo del nucleo que nadie agrego a las raices escaparia de C-08, C-09 y C-10 en silencio.
            var guarded = new HashSet<Type>(SharedFoundationInspection.Roots);
            var headerTypes = typeof(HeaderConfigurationSnapshot).Assembly.GetTypes()
                .Where(type => type.Namespace == SharedFoundationInspection.SharedNamespace && !type.IsNested)
                .Where(type => type.Name.StartsWith("Header", StringComparison.Ordinal))
                .ToList();

            Assert.NotEmpty(headerTypes);
            Assert.All(headerTypes, type => Assert.True(guarded.Contains(type), type.FullName + " no esta en SharedFoundationInspection.Roots."));
            Assert.All(SharedFoundationInspection.Roots, root => Assert.Equal(SharedFoundationInspection.SharedNamespace, root.Namespace));
        }

        // ===== C-10 — Shared no contiene politica de cobertura, StageInert ni ejecutor generico =========================

        [Fact]
        public void C10_GUARDA_SHARED_NO_CONTIENE_POLITICA_DE_COBERTURA_STAGEINERT_NI_EJECUTOR_GENERICO()
        {
            var shared = typeof(HeaderConfigurationSnapshot).Assembly.GetTypes()
                .Where(type => type.Namespace == SharedFoundationInspection.SharedNamespace)
                .ToList();
            Assert.Contains(shared, type => type == typeof(RackModuleEditSession)); // premisa: el alcance es el Shared real

            var offenders = new List<string>();

            // Todo Shared, no solo lo nuevo: ningun tipo ni miembro con esas responsabilidades.
            var forbiddenTypeFragments = new[] { "Coverage", "StageInert", "OmitAndReport", "Executor", "BatchService", "Engine", "Callback" };
            foreach (var type in shared)
            {
                if (forbiddenTypeFragments.Any(fragment => type.Name.IndexOf(fragment, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    offenders.Add("tipo " + type.FullName);
                }

                foreach (var member in type.GetMembers(SharedFoundationInspection.Declared))
                {
                    if (member.Name.IndexOf("StageInert", StringComparison.OrdinalIgnoreCase) >= 0
                        || member.Name.IndexOf("OmitAndReport", StringComparison.OrdinalIgnoreCase) >= 0
                        || member.Name.IndexOf("CoveragePolicy", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        offenders.Add("miembro " + type.Name + "." + member.Name);
                    }
                }
            }

            // Los tipos nuevos: resultados y valores, no un motor. Ni interfaces ni delegados en su API, ni verbos de
            // ejecucion (resolver, preparar, mutar, recomputar, aplicar, confirmar...).
            var engineVerbs = new[] { "Execute", "Run", "Resolve", "Prepare", "Mutate", "Recompute", "Apply", "Commit", "Cancel", "Confirm", "Reconcile", "Invoke", "Create", "Clamp" };
            foreach (var type in SharedFoundationInspection.TypesWithNested().Where(type => !SharedFoundationInspection.IsCompilerGenerated(type)))
            {
                if (type.IsInterface || typeof(Delegate).IsAssignableFrom(type))
                {
                    offenders.Add("abstraccion " + type.Name);
                }

                foreach (var method in type.GetMethods(SharedFoundationInspection.Declared).Where(method => !SharedFoundationInspection.IsCompilerGenerated(method)))
                {
                    if (engineVerbs.Any(verb => method.Name.StartsWith(verb, StringComparison.Ordinal)))
                    {
                        offenders.Add("verbo de motor " + type.Name + "." + method.Name);
                    }

                    if (method.GetParameters().Select(parameter => parameter.ParameterType).Append(method.ReturnType).Any(IsDelegate))
                    {
                        offenders.Add("callback " + type.Name + "." + method.Name);
                    }
                }

                foreach (var constructor in type.GetConstructors(SharedFoundationInspection.Declared))
                {
                    if (constructor.GetParameters().Any(parameter => IsDelegate(parameter.ParameterType)))
                    {
                        offenders.Add("callback " + type.Name + "..ctor");
                    }
                }

                foreach (var property in type.GetProperties(SharedFoundationInspection.Declared))
                {
                    if (IsDelegate(property.PropertyType))
                    {
                        offenders.Add("callback " + type.Name + "." + property.Name);
                    }
                }
            }

            Assert.Empty(offenders);
        }

        private static bool IsDelegate(Type type) => type != null && typeof(Delegate).IsAssignableFrom(type);
    }
}
