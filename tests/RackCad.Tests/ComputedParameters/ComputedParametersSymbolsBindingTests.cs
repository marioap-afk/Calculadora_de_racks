using System.Collections.Generic;
using System.Linq;
using RackCad.Application.ComputedParameters;
using RackCad.Application.Expressions;
using Xunit;
using static RackCad.Tests.ComputedParametersSymbolsKit;
using static RackCad.Tests.ExpressionSemanticTestSupport;

namespace RackCad.Tests
{
    /// <summary>
    /// I-63 G3-T1 (RED) - el binder con entradas rack (D-04, D-14 y D-16.4-5): INV-17 (disponibilidad), INV-19 (homonimos
    /// entre namespaces), INV-20 con A-1.2 (<c>OperatorInName</c> por namespace), INV-25 (regla de ambito) e INV-26 (el
    /// nombre visible no es identidad). Los contextos usan entradas rack SINTETICAS: el nucleo no valida nombres de
    /// miembro, eso lo exige el catalogo (D-03).
    /// </summary>
    public class ComputedParametersSymbolsBindingTests
    {
        private static SymbolId RefId(BoundExpression expression) => Assert.IsType<BoundReference>(expression).Symbol;

        // ================================================================ INV-17: disponibilidad D-14 en el contexto de rack

        [Theory]
        [InlineData("Rack.Frentes", "frentes")]
        [InlineData("rack.frentes", "frentes")]
        [InlineData("RACK.FRENTES", "frentes")]
        [InlineData("Rack.FrentesVacios", "frentesVacios")]
        [InlineData("Rack.frentesvacios", "frentesVacios")]
        public void INV17_RackMember_BindsInRackScope_IgnoringCase(string text, string token)
        {
            Assert.Equal(RackId(token), RefId(BindOk(text, RackContext(), SymbolScope.Rack)));
        }

        [Fact]
        public void INV17_InTheRackContext_RackFrentesBinds_AndProjectTotalRacksIsAnUnknownNamespace()
        {
            var context = RackContext(Variable(1, "Holgura", 6));

            // Control en el MISMO contexto: Rack.Frentes enlaza.
            Assert.Equal(FrentesId, RefId(BindOk("Rack.Frentes", context, SymbolScope.Rack)));

            foreach (var text in new[] { "Project.TotalRacks", "Project.Frentes", "project.totalracks", "Holgura.Frentes", "Total.Racks" })
            {
                var diagnostic = Assert.Single(BindFails(text, context, SymbolScope.Rack));

                Assert.Equal(ExpressionDiagnosticCode.UnknownNamespace, diagnostic.Code);
                Assert.Equal(ExpressionDiagnosticClass.Binding, diagnostic.Class);
                Assert.Equal(new SourceSpan(0, text.Length), diagnostic.Span);
            }
        }

        [Fact]
        public void INV17_ARackMemberIsLookedUpOnlyAmongRackEntries()
        {
            var context = RackContext(Variable(1, "Holgura", 6));

            // Control: Holgura SI es un simbolo del contexto, enlazado sin namespace.
            Assert.Equal(Id(1), RefId(BindOk("Holgura", context, SymbolScope.Rack)));

            foreach (var text in new[] { "Rack.Holgura", "Rack.Nope", "Rack.Frente" })
            {
                var diagnostic = Assert.Single(BindFails(text, context, SymbolScope.Rack));

                Assert.Equal(ExpressionDiagnosticCode.UnknownSymbol, diagnostic.Code);
                Assert.Equal(ExpressionDiagnosticClass.Binding, diagnostic.Class);
            }
        }

        [Fact]
        public void INV17_RackWithoutMember_IsNameRequired_OnlyWhenTheTableHasRackEntries()
        {
            var withRack = Assert.Single(BindFails("Rack.", RackContext(), SymbolScope.Rack));

            Assert.Equal(ExpressionDiagnosticCode.NameRequired, withRack.Code);
            Assert.Equal(ExpressionDiagnosticClass.Binding, withRack.Class);
            Assert.Equal(new SourceSpan(0, 5), withRack.Span);

            // Sin entradas rack en la tabla todo sigue como hoy: UnknownNamespace.
            var withoutRack = Assert.Single(BindFails("Rack.", Context(Variable(1, "Holgura", 6)), SymbolScope.Rack));

            Assert.Equal(ExpressionDiagnosticCode.UnknownNamespace, withoutRack.Code);
            Assert.Equal(new SourceSpan(0, 5), withoutRack.Span);
        }

        // ================================================================ INV-25: regla de ambito para rack

        [Fact]
        public void INV25_ARackScopedEntryUsedByAProjectConsumer_IsAScopeViolation()
        {
            var context = RackContext();

            // Control: el mismo texto y el mismo contexto enlazan para un consumidor de ambito Rack.
            Assert.Equal(FrentesId, RefId(BindOk("Rack.Frentes", context, SymbolScope.Rack)));

            var diagnostic = Assert.Single(BindFails("Rack.Frentes", context, SymbolScope.Project));

            Assert.Equal(ExpressionDiagnosticCode.ScopeViolation, diagnostic.Code);
            Assert.Equal(ExpressionDiagnosticClass.Binding, diagnostic.Class);
            Assert.Equal(new SourceSpan(0, 12), diagnostic.Span);
            Assert.Equal(new[] { FrentesId }, diagnostic.RelatedSymbols);
        }

        [Fact]
        public void INV25_AProjectConsumerStillSeesItsProjectVariables_BesideRackEntries()
        {
            var context = RackContext(Variable(1, "Holgura", 6));

            Assert.Equal(Id(1), RefId(BindOk("Holgura", context, SymbolScope.Project)));
            Assert.Equal(ExpressionDiagnosticCode.ScopeViolation, Assert.Single(BindFails("Rack.FrentesVacios", context, SymbolScope.Project)).Code);
        }

        // ================================================================ INV-19: homonimos entre namespaces

        [Theory]
        [InlineData("{Frentes}")]
        [InlineData("Frentes")]
        [InlineData("frentes")]
        [InlineData("{frentes}")]
        public void INV19_TheHomonymousVariable_WinsForEveryFormWithoutNamespace(string text)
        {
            var context = RackContext(Variable(1, "Frentes", 3));

            // BindOk afirma ademas cero diagnosticos: ni AmbiguousName ni OperatorInName nuevos.
            Assert.Equal(Id(1), RefId(BindOk(text, context, SymbolScope.Rack)));
        }

        [Fact]
        public void INV19_TheQualifiedForm_ResolvesTheVariable_NotTheComputed()
        {
            var context = RackContext(Variable(1, "Frentes", 3));

            Assert.Equal(Id(1), RefId(BindOk("Frentes#" + Key(1), context, SymbolScope.Rack)));
            Assert.Equal(Id(1), RefId(BindOk("{Frentes}#" + Key(1), context, SymbolScope.Rack)));
        }

        [Fact]
        public void INV19_RackFrentes_ResolvesTheComputed_EvenWithTheHomonymousVariable()
        {
            var context = RackContext(Variable(1, "Frentes", 3));

            Assert.Equal(FrentesId, RefId(BindOk("Rack.Frentes", context, SymbolScope.Rack)));
            Assert.Equal(FrentesId, RefId(BindOk("rack.FRENTES", context, SymbolScope.Rack)));

            var tree = Assert.IsType<BoundBinary>(BindOk("Rack.Frentes * {Frentes}", context, SymbolScope.Rack));
            Assert.Equal(FrentesId, RefId(tree.Left));
            Assert.Equal(Id(1), RefId(tree.Right));
        }

        [Fact]
        public void INV19_WithoutTheHomonymousVariable_ANameWithoutNamespaceIsUnknown_NeverTheComputed()
        {
            var context = RackContext(Variable(2, "Otra", 1));

            // Control: con la variable homonima presente el mismo texto enlaza (a la variable).
            Assert.Equal(Id(1), RefId(BindOk("{Frentes}", RackContext(Variable(1, "Frentes", 3)), SymbolScope.Rack)));

            foreach (var text in new[] { "{Frentes}", "Frentes", "frentes", "{FrentesVacios}", "FrentesVacios" })
            {
                var diagnostic = Assert.Single(BindFails(text, context, SymbolScope.Rack));

                Assert.Equal(ExpressionDiagnosticCode.UnknownSymbol, diagnostic.Code);
                Assert.Equal(ExpressionDiagnosticClass.Binding, diagnostic.Class);
                Assert.DoesNotContain(FrentesId, diagnostic.RelatedSymbols ?? new List<SymbolId>());
                Assert.DoesNotContain(FrentesVaciosId, diagnostic.RelatedSymbols ?? new List<SymbolId>());
            }
        }

        [Fact]
        public void INV19_AmbiguousName_ListsOnlyTheProjectVariableCandidates()
        {
            // Dos variables homonimas y un miembro rack con el mismo nombre: el indice es por namespace.
            var context = RackContext(Variable(1, "Frentes", 3), Variable(2, "Frentes", 4));

            var diagnostic = Assert.Single(BindFails("Frentes", context, SymbolScope.Rack));

            Assert.Equal(ExpressionDiagnosticCode.AmbiguousName, diagnostic.Code);
            Assert.Equal(new[] { Id(1), Id(2) }, diagnostic.RelatedSymbols);

            // El miembro rack sigue sin ambiguedad.
            Assert.Equal(FrentesId, RefId(BindOk("Rack.Frentes", context, SymbolScope.Rack)));
        }

        // ================================================================ INV-20 (con A-1.2): OperatorInName por namespace

        /// <summary>
        /// UN solo helper para el objetivo y para el control de A-1.2: enlaza el texto, afirma que falla y devuelve los
        /// diagnosticos.
        /// </summary>
        private static IReadOnlyList<ExpressionDiagnostic> Diagnose(string text, ExpressionContext context)
            => BindFails(text, context, SymbolScope.Rack);

        [Fact]
        public void INV20_ARackMemberNameWithAnOperator_IsNotAnOperatorInNameRun()
        {
            // Objetivo: entrada rack SINTETICA con nombre "Frentes-Vacios" y ninguna variable con operador.
            var target = Context(RackEntry("frentesVacios", "Frentes-Vacios"));

            var diagnostics = Diagnose("Frentes-Vacios", target);

            Assert.DoesNotContain(diagnostics, diagnostic => diagnostic.Code == ExpressionDiagnosticCode.OperatorInName);
            Assert.All(diagnostics, diagnostic => Assert.Equal(ExpressionDiagnosticCode.UnknownSymbol, diagnostic.Code));
            Assert.Equal(new SourceSpan(0, 7), diagnostics[0].Span);

            // Control explicito (A-1.2), con el MISMO helper y la misma entrada rack: la variable A-B sigue dando OperatorInName.
            var control = Context(Variable(1, "A-B", 1), RackEntry("frentesVacios", "Frentes-Vacios"));

            var found = Assert.Single(Diagnose("A-B", control));

            Assert.Equal(ExpressionDiagnosticCode.OperatorInName, found.Code);
            Assert.Equal(ExpressionDiagnosticClass.Binding, found.Class);
            Assert.Equal(new SourceSpan(0, 3), found.Span);
            Assert.Equal(new[] { Id(1) }, found.RelatedSymbols);
        }

        [Fact]
        public void INV20_TheRackEntryDoesNotDisableTheDetectorForTheVariablesOfTheSameTable()
        {
            var context = Context(Variable(1, "Holgura-Base", 1), RackEntry("frentesVacios", "Frentes-Vacios"));

            var found = Assert.Single(Diagnose("Holgura-Base + 2", context));

            Assert.Equal(ExpressionDiagnosticCode.OperatorInName, found.Code);
            Assert.Equal(new SourceSpan(0, 12), found.Span);
            Assert.Equal(new[] { Id(1) }, found.RelatedSymbols);
        }

        // ================================================================ INV-26: el nombre visible no es identidad

        [Fact]
        public void INV26_RenamingTheMember_DoesNotChangeTheSymbolIdNorTheTree()
        {
            var original = Context(Frentes());
            var renamed = Context(RackEntry(RackMetricIds.FrentesToken, "Anchos"));

            var tree = BindOk("Rack.Frentes", original, SymbolScope.Rack);
            var renamedTree = BindOk("Rack.Anchos", renamed, SymbolScope.Rack);

            Assert.Equal(tree, renamedTree);
            Assert.Equal(FrentesId, RefId(renamedTree));
            Assert.Equal(RackMetricIds.FrentesToken, RefId(renamedTree).Key);

            // El arbol se muestra con el nombre ACTUAL de cada tabla.
            Assert.Equal("Rack.Frentes", ExpressionFormatter.Format(tree, original.Symbols));
            Assert.Equal("Rack.Anchos", ExpressionFormatter.Format(tree, renamed.Symbols));

            // El nombre viejo ya no resuelve en la tabla renombrada: el nombre no es identidad.
            var stale = Assert.Single(BindFails("Rack.Frentes", renamed, SymbolScope.Rack));
            Assert.Equal(ExpressionDiagnosticCode.UnknownSymbol, stale.Code);
        }

        [Fact]
        public void INV26_TheTreeDoesNotCarryTheName_OnlyTheIdentity()
        {
            var tree = BindOk("Rack.Frentes + Rack.FrentesVacios", RackContext(), SymbolScope.Rack);

            var dependencies = BoundExpressionDependencies.DirectDependencies(tree);

            // Las dependencias son identidades rack (token), no los nombres de miembro con los que se escribio el texto.
            Assert.Equal(new[] { FrentesId, FrentesVaciosId }, dependencies);
            Assert.All(dependencies, id => Assert.Equal(RackNamespace, id.Namespace));
            Assert.Equal(new[] { "frentes", "frentesVacios" }, dependencies.Select(id => id.Key));
        }
    }
}
