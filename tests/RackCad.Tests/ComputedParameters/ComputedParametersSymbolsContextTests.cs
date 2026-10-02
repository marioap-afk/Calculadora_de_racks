using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.ComputedParameters;
using RackCad.Application.Expressions;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using Xunit;
using static RackCad.Tests.ComputedParametersSymbolsKit;
using static RackCad.Tests.ExpressionSemanticTestSupport;

namespace RackCad.Tests
{
    /// <summary>
    /// I-63 G3-T1 (RED) - <c>RackComputedExpressionContext</c> (D-14, D-15 y D-17 puntos 4 a 6) e INV-15: la propagacion
    /// por estado. El contexto se construye SOLO con los resultados terminados de <see cref="RackMetricRequest"/>
    /// (<see cref="RackMetricResults"/>); evaluar nunca resuelve. Su tabla une las entradas projectVariable del registro y
    /// las entradas rack <c>Computed</c>; se enlaza con ambito Rack. Si todas las referencias rack estan
    /// <c>Available</c> evalua con el evaluador del nucleo; si alguna no lo esta, devuelve sin evaluar
    /// <c>ComputedReferencesNotAvailable</c> con la lista ordenada por SymbolId de TODAS las que no estan Available,
    /// sin valor parcial, sin colapsar estados y sin <c>BrokenReference</c>.
    /// </summary>
    public class ComputedParametersSymbolsContextTests
    {
        private const string RackGuid = "3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f44";
        private const string BeamId = "BEAM_A";
        private const string Evaluated = "Evaluated";
        private const string NotAvailable = "ComputedReferencesNotAvailable";

        // ---------------------------------------------------------------- fixtures reales de G1 (peticion por rack)

        private static SelectiveCell Cell()
            => new SelectiveCell
            {
                Pallet = new Tarima { Frente = 48, Alto = 50 },
                PalletCount = 1,
                BeamId = BeamId,
                BeamPeralte = 4.5,
            };

        private static SelectiveBayDesign Bay(int levels)
        {
            var bay = new SelectiveBayDesign();
            for (var i = 0; i < levels; i++)
            {
                bay.Levels.Add(Cell());
            }

            return bay;
        }

        private static SelectivePalletDesign Design(params SelectiveBayDesign[] fondo0)
        {
            var design = new SelectivePalletDesign { PostId = "POST_A", PostPeralte = 3.0 };
            foreach (var bay in fondo0)
            {
                design.Bays.Add(bay);
            }

            return design;
        }

        private static string DesignJson(SelectivePalletDesign design)
            => new SelectivePalletDesignStore().Serialize(SelectivePalletDesignDocument.From(design, RackGuid, "Rack 1"));

        private static RackDefinitionCapture Capture(string key, string kind, string designJson, int references = 1)
            => new RackDefinitionCapture(
                key,
                new RackEmbedStore().Serialize(new RackEmbedDocument
                {
                    Id = RackGuid,
                    Kind = kind,
                    View = RackEmbedDocument.ViewFrontal,
                    Name = "Rack 1",
                    Design = designJson,
                }),
                references);

        private static string PushBackJson()
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
                },
            };
            design.Structure.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 2, PalletsDeep = 6, DepthStartPosition = 1 });
            design.Fronts.Add(new PushBackFrontConfig());
            return new RackProjectStore().Serialize(RackProject.ForPushBack(design));
        }

        private static string HeaderJson()
            => new RackProjectStore().Serialize(RackProject.ForSelective(new HardcodedStandardRackFrameService().CreateDefault()));

        private static RackMetricResults Request(IReadOnlyList<RackDefinitionCapture> siblings)
            => new RackMetricRequest(
                siblings,
                ProjectVariablesReadResult.Absent(),
                RackCatalogInput.Loaded(new RackCatalog())).Execute();

        private static RackMetricResults Direct(MetricValue frentes, MetricValue frentesVacios)
            => RackMetricResults.Create(
                RackGuid,
                new Dictionary<MetricId, MetricValue>
                {
                    [RackMetricIds.Frentes] = frentes,
                    [RackMetricIds.FrentesVacios] = frentesVacios,
                });

        private static BoundExpression Bound(string text, ComputedParametersSymbolsKit.RackContextView context)
            => BindOk(text, context.Expressions, SymbolScope.Rack);

        private static void AssertNotAvailable(
            ComputedParametersSymbolsKit.EvaluationView result,
            params (SymbolId Symbol, MetricStatus Status, UnavailableReason Reason)[] expected)
        {
            Assert.Equal(NotAvailable, result.Outcome);

            // Nunca hay valor parcial, ni un fallo de evaluacion, ni BrokenReference.
            Assert.Throws<InvalidOperationException>(() => result.Value);
            Assert.Empty(result.Diagnostics);

            var actual = result.NotAvailable;
            Assert.Equal(expected.Length, actual.Count);
            for (var index = 0; index < expected.Length; index++)
            {
                Assert.Equal(expected[index].Symbol, actual[index].Symbol);
                Assert.Equal(expected[index].Status, actual[index].Status);
                Assert.Equal(expected[index].Reason, actual[index].Reason);
            }
        }

        // ================================================================ INV-15: propagacion por estado

        [Fact]
        public void INV15_PushBack_EvenWithAnUnreadableDesign_IsNotSupported_WithoutReadingOrResolving()
        {
            // Control: ese texto SI es ilegible para el lector D-26; si el contexto lo leyera, no daria NotSupported.
            Assert.False(new RackMetricDesignReader().IsReadable(RackEmbedDocument.KindPushBack, "{ esto no es un diseno"));

            var results = Request(new List<RackDefinitionCapture>
            {
                Capture("DEF-A", RackEmbedDocument.KindPushBack, "{ esto no es un diseno"),
            });
            var context = CreateRackContext(results);

            // El enlace NO depende del kind (D-17.5): Rack.Frentes enlaza en cualquier rack.
            var result = context.Evaluate(Bound("Rack.Frentes * 2", context));

            AssertNotAvailable(result, (FrentesId, MetricStatus.NotSupported, null));
        }

        [Fact]
        public void INV15_PushBackWithAReadableDesign_IsNotSupported()
        {
            var results = Request(new List<RackDefinitionCapture> { Capture("DEF-A", RackEmbedDocument.KindPushBack, PushBackJson()) });
            var context = CreateRackContext(results);

            AssertNotAvailable(
                context.Evaluate(Bound("Rack.Frentes * 2", context)),
                (FrentesId, MetricStatus.NotSupported, null));
        }

        [Fact]
        public void INV15_Cabecera_IsNotApplicable()
        {
            var results = Request(new List<RackDefinitionCapture> { Capture("DEF-A", RackEmbedDocument.KindCabecera, HeaderJson()) });
            var context = CreateRackContext(results);

            AssertNotAvailable(
                context.Evaluate(Bound("Rack.Frentes * 2", context)),
                (FrentesId, MetricStatus.NotApplicable, null));
        }

        [Fact]
        public void INV15_SelectiveWithDivergentSiblings_IsUnavailableSiblingsDivergent()
        {
            var one = Design(Bay(2), Bay(2));
            var other = Design(Bay(2), Bay(2));
            other.VerticalClearance = one.VerticalClearance + 2.0;

            var results = Request(new List<RackDefinitionCapture>
            {
                Capture("DEF-A", RackEmbedDocument.KindSelective, DesignJson(one)),
                Capture("DEF-B", RackEmbedDocument.KindSelective, DesignJson(other)),
            });
            var context = CreateRackContext(results);

            AssertNotAvailable(
                context.Evaluate(Bound("Rack.Frentes * 2", context)),
                (FrentesId, MetricStatus.Unavailable, UnavailableReason.Of(UnavailableReasonKind.SiblingsDivergent)));
        }

        [Fact]
        public void INV15_ABrokenProjectVariable_IsUnavailableEffectiveFailed_NotABrokenReference()
        {
            var broken = MetricValue.Unavailable(
                UnavailableReason.EffectiveFailed(SelectiveEffectiveOutcome.BrokenProjectVariableReference));
            var context = CreateRackContext(Direct(broken, broken));

            var result = context.Evaluate(Bound("Rack.Frentes * 2", context));

            AssertNotAvailable(
                result,
                (FrentesId, MetricStatus.Unavailable, UnavailableReason.EffectiveFailed(SelectiveEffectiveOutcome.BrokenProjectVariableReference)));
            Assert.DoesNotContain(result.Diagnostics, diagnostic => diagnostic.Code == ExpressionDiagnosticCode.BrokenReference);
        }

        [Fact]
        public void INV15_AReadableSelectiveWithFourFrontsAtDepthZero_EvaluatesRackFrentesTimesTwoAsEight()
        {
            var results = Request(new List<RackDefinitionCapture>
            {
                Capture("DEF-A", RackEmbedDocument.KindSelective, DesignJson(Design(Bay(2), Bay(2), Bay(2), Bay(2)))),
                Capture("DEF-B", RackEmbedDocument.KindSelective, DesignJson(Design(Bay(2), Bay(2), Bay(2), Bay(2))), references: 0),
            });

            // Control sobre el resultado de G1: el rack es legible y Frentes esta Available con valor 4.
            Assert.Equal(MetricStatus.Available, results[RackMetricIds.Frentes].Status);
            Assert.Equal(4.0, results[RackMetricIds.Frentes].Value);

            var context = CreateRackContext(results);

            var result = context.Evaluate(Bound("Rack.Frentes * 2", context));

            Assert.Equal(Evaluated, result.Outcome);
            AssertBits(8.0, result.Value);
            Assert.Empty(result.NotAvailable);
            Assert.Empty(result.Diagnostics);
        }

        [Fact]
        public void INV15_AnAvailableRackReference_EvaluatesWithItsMetricValues_ThroughTheCoreEvaluator()
        {
            var context = CreateRackContext(Direct(MetricValue.Available(4), MetricValue.Available(1)));

            AssertBits(3.0, context.Evaluate(Bound("Rack.Frentes - Rack.FrentesVacios", context)).Value);
            AssertBits(10.0, context.Evaluate(Bound("MAX(Rack.Frentes, 2) + Rack.FrentesVacios * 6", context)).Value);

            // Un resultado del nucleo que falla NO es una referencia no disponible: es un fallo de evaluacion.
            var divided = context.Evaluate(Bound("Rack.Frentes / (Rack.FrentesVacios - 1)", context));
            Assert.Equal("EvaluationFailed", divided.Outcome);
            Assert.Contains(divided.Diagnostics, diagnostic => diagnostic.Code == ExpressionDiagnosticCode.DivisionByZero);
            Assert.Throws<InvalidOperationException>(() => divided.Value);
            Assert.Empty(divided.NotAvailable);
        }

        [Fact]
        public void INV15_TwoReferencesWithDifferentStates_ListBoth_InSymbolIdOrder_WithoutCollapsingThem()
        {
            var context = CreateRackContext(Direct(
                MetricValue.Unavailable(UnavailableReason.Of(UnavailableReasonKind.SiblingsDivergent)),
                MetricValue.NotSupported()));

            // El texto cita FrentesVacios antes que Frentes: la lista va en orden de SymbolId, no de aparicion.
            var result = context.Evaluate(Bound("Rack.FrentesVacios + Rack.Frentes", context));

            AssertNotAvailable(
                result,
                (FrentesId, MetricStatus.Unavailable, UnavailableReason.Of(UnavailableReasonKind.SiblingsDivergent)),
                (FrentesVaciosId, MetricStatus.NotSupported, null));
        }

        [Fact]
        public void INV15_TheListHoldsAllTheReferencesThatAreNotAvailable_AndOnlyThose()
        {
            var context = CreateRackContext(Direct(MetricValue.Available(4), MetricValue.NotApplicable()));

            // Frentes esta Available y no aparece; FrentesVacios no lo esta y aparece con su propio estado.
            AssertNotAvailable(
                context.Evaluate(Bound("Rack.Frentes + Rack.FrentesVacios", context)),
                (FrentesVaciosId, MetricStatus.NotApplicable, null));

            // Una expresion que solo lee referencias Available no ve la que no lo esta.
            AssertBits(8.0, context.Evaluate(Bound("Rack.Frentes * 2", context)).Value);
        }

        [Fact]
        public void INV15_ARepeatedReference_IsListedOnce()
        {
            var context = CreateRackContext(Direct(MetricValue.NotSupported(), MetricValue.NotSupported()));

            AssertNotAvailable(
                context.Evaluate(Bound("Rack.Frentes + Rack.Frentes * Rack.Frentes", context)),
                (FrentesId, MetricStatus.NotSupported, null));
        }

        [Fact]
        public void INV15_AnExpressionWithoutRackReferences_IsEvaluatedWhateverTheStates()
        {
            var context = CreateRackContext(Direct(MetricValue.NotSupported(), MetricValue.NotApplicable()));

            var result = context.Evaluate(Bound("2 * 3", context));

            Assert.Equal(Evaluated, result.Outcome);
            AssertBits(6.0, result.Value);
        }

        // ================================================================ D-14, D-15 y D-17: forma del contexto

        [Fact]
        public void D17_TheTable_JoinsTheProjectVariableEntriesAndTheRackComputedEntries()
        {
            var context = CreateRackContext(
                Direct(MetricValue.Available(4), MetricValue.Available(0)),
                Variable(1, "Holgura", 6),
                Variable(2, "Base", 10));

            var entries = context.Symbols.Entries;

            Assert.Equal(new[] { Id(1), Id(2), FrentesId, FrentesVaciosId }, entries.Select(entry => entry.Id));

            var rack = entries.Where(entry => entry.Id.Namespace == RackNamespace).ToList();
            Assert.Equal(new[] { "Frentes", "FrentesVacios" }, rack.Select(entry => entry.DisplayName));
            Assert.All(rack, entry => Assert.Equal(SymbolScope.Rack, entry.Scope));
            Assert.All(rack, entry => Assert.Equal(ComputedKind, entry.Definition.Kind));

            var variables = entries.Where(entry => entry.Id.Namespace == SymbolNamespace.ProjectVariable).ToList();
            Assert.All(variables, entry => Assert.Equal(SymbolScope.Project, entry.Scope));
        }

        [Fact]
        public void D14_InTheRackContext_RackFrentesBinds_AndProjectTotalRacksIsAnUnknownNamespace()
        {
            var context = CreateRackContext(
                Direct(MetricValue.Available(4), MetricValue.Available(0)),
                Variable(1, "Frentes", 3));

            Assert.Equal(FrentesId, Assert.IsType<BoundReference>(Bound("Rack.Frentes", context)).Symbol);
            Assert.Equal(Id(1), Assert.IsType<BoundReference>(Bound("{Frentes}", context)).Symbol);

            foreach (var text in new[] { "Project.TotalRacks", "Project.RackCount" })
            {
                var diagnostic = Assert.Single(BindFails(text, context.Expressions, SymbolScope.Rack));
                Assert.Equal(ExpressionDiagnosticCode.UnknownNamespace, diagnostic.Code);
            }
        }

        [Fact]
        public void D15_TheContextIsBuiltOnlyFromFinishedResults_NeverFromARequestOrAResolutionSide()
        {
            var type = RackContextType();
            var create = CreateMethod();

            // R2: el contexto vive en Application, fuera de RackCad.Application.Expressions (guarda de P26.1).
            Assert.Equal("RackCad.Application.ComputedParameters", type.Namespace);

            var parameterTypes = create.GetParameters().Select(parameter => parameter.ParameterType).ToList();
            Assert.Contains(typeof(RackMetricResults), parameterTypes);
            Assert.DoesNotContain(typeof(RackMetricRequest), parameterTypes);
            Assert.DoesNotContain(typeof(IRackMetricResolutionSide), parameterTypes);
            Assert.DoesNotContain(typeof(IRackMetricDesignReader), parameterTypes);

            Assert.Empty(type.GetConstructors(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance));
        }
    }
}
