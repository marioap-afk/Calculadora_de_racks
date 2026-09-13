using System;
using System.Linq;
using RackCad.Application.CustomProperties;
using RackCad.Application.Persistence;
using Xunit;
using static RackCad.Tests.CustomPropertiesAuthorityTestKit;
using static RackCad.Tests.CustomPropertiesTestKit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-54 G5 — workspace puro y agnostico al alcance (Proposal V5 D-18.3 y D-10.3; §12.3 T-AUT-16). Filas
    /// <c>{ Id, Name, Value, NombreRepetido }</c> solo cuando la autoridad es editable; en cualquier otro caso, diagnostico
    /// y resumenes por vista, nunca los valores de una hermana como si fueran los del rack.
    /// </summary>
    public class CustomPropertiesWorkspaceTests
    {
        private static readonly string Base = Props(Entry(IdA, "Cliente", "ACME"), Entry(IdB, "Area", "Norte"));

        // ================================================================ Rack

        [Fact]
        public void TAut16_RackSingle_EsEditable_ConFilasPorId()
        {
            var workspace = CustomPropertiesWorkspace.ForRack(
                Authority("1", View("1", properties: Base), View("2", properties: Base, view: "lateral")));

            Assert.Equal(CustomPropertiesWorkspaceState.Editable, workspace.State);
            Assert.Equal(CustomPropertiesScope.Rack, workspace.Scope);
            Assert.Null(workspace.ReadOnlyReason);
            Assert.Equal(new[] { Id(IdA), Id(IdB) }, workspace.Rows.Select(row => row.Id));
            Assert.Equal(new[] { "Cliente", "Area" }, workspace.Rows.Select(row => row.Name));
            Assert.Equal(new[] { "ACME", "Norte" }, workspace.Rows.Select(row => row.Value));
            Assert.All(workspace.Rows, row => Assert.False(row.NombreRepetido));
            Assert.Empty(workspace.ViewSummaries);
            Assert.False(workspace.UnifyAvailable);
            Assert.Contains("Rack A", workspace.ScopeLabel);
        }

        [Fact]
        public void TAut16_RackSingle_MarcaLosNombresRepetidosSinDejarDeSerEditable()
        {
            var repeated = Props(Entry(IdA, AreaNfc, "1"), Entry(IdB, AreaNfd, "2"), Entry(IdC, "Otro", "3"));

            var workspace = CustomPropertiesWorkspace.ForRack(Authority("1", View("1", properties: repeated)));

            Assert.Equal(CustomPropertiesWorkspaceState.Editable, workspace.State);
            Assert.Equal(new[] { true, true, false }, workspace.Rows.Select(row => row.NombreRepetido));
        }

        [Fact]
        public void TAut16_RackDivergent_SinFilas_ConResumenPorVista_YUnificacionDisponible()
        {
            var other = Props(Entry(IdA, "Cliente", "OTRO"));
            var workspace = CustomPropertiesWorkspace.ForRack(
                Authority("1", View("1", properties: Base, section: -1), View("2", properties: other, view: "lateral", section: 4), View("3", view: "planta")));

            Assert.Equal(CustomPropertiesWorkspaceState.Divergent, workspace.State);
            Assert.Empty(workspace.Rows);
            Assert.True(workspace.UnifyAvailable);
            Assert.Equal(new[] { "1", "2", "3" }, workspace.ViewSummaries.Select(summary => summary.Handle));
            Assert.False(string.IsNullOrWhiteSpace(workspace.Diagnostic));

            var lateral = Assert.Single(workspace.ViewSummaries, summary => summary.Handle == "2");
            Assert.Equal("RACK_2", lateral.BlockName);
            Assert.Equal("lateral", lateral.View);
            Assert.Equal(4, lateral.Section);
            Assert.Equal(CustomPropertiesReadOutcome.Readable, lateral.State);
            Assert.Equal(new[] { "Cliente — OTRO" }, lateral.Entries.Select(entry => entry.Name + " — " + entry.Value));
            Assert.True(lateral.IsUnifySourceAvailable);

            var planta = Assert.Single(workspace.ViewSummaries, summary => summary.Handle == "3");
            Assert.Equal(CustomPropertiesReadOutcome.Absent, planta.State);
            Assert.Empty(planta.Entries);
        }

        [Fact]
        public void TAut16_RackDivergent_ConUnificacionNoDisponible()
        {
            var one = Doc("1.0", Entries(Entry(IdA, "Cliente", "ACME")), ",\"Raiz\":1");
            var two = Doc("1.0", Entries(Entry(IdA, "Cliente", "OTRO")), ",\"Raiz\":2");

            var workspace = CustomPropertiesWorkspace.ForRack(Authority("1", View("1", properties: one), View("2", properties: two)));

            Assert.Equal(CustomPropertiesWorkspaceState.Divergent, workspace.State);
            Assert.False(workspace.UnifyAvailable);
            Assert.All(workspace.ViewSummaries, summary => Assert.False(summary.IsUnifySourceAvailable));
        }

        [Fact]
        public void TAut16_RackCustomPropertiesReadOnly_MuestraElEstadoDeCadaVista_SinFilas()
        {
            var workspace = CustomPropertiesWorkspace.ForRack(
                Authority("1", View("1", properties: Base), View("2", properties: EmptyAt("2.0"), view: "lateral")));

            Assert.Equal(CustomPropertiesWorkspaceState.ReadOnly, workspace.State);
            Assert.Equal(CustomPropertiesReadOnlyReason.CustomPropertiesReadOnly, workspace.ReadOnlyReason);
            Assert.Empty(workspace.Rows);
            Assert.False(workspace.UnifyAvailable);

            var incompatible = Assert.Single(workspace.ViewSummaries, summary => summary.Handle == "2");
            Assert.Equal(CustomPropertiesReadOutcome.IncompatibleMajor, incompatible.State);
            Assert.Empty(incompatible.Entries);
            Assert.False(string.IsNullOrWhiteSpace(incompatible.Error));
        }

        public static TheoryData<string, CustomPropertiesReadOnlyReason> ReadOnlyRacks()
            => new TheoryData<string, CustomPropertiesReadOnlyReason>
            {
                { "xref", CustomPropertiesReadOnlyReason.XrefRejected },
                { "noidentity", CustomPropertiesReadOnlyReason.NoIdentity },
                { "indeterminate", CustomPropertiesReadOnlyReason.IndeterminateMembership },
                { "mixed", CustomPropertiesReadOnlyReason.MixedKind },
                { "unknown", CustomPropertiesReadOnlyReason.UnknownKind },
                { "profundidad", CustomPropertiesReadOnlyReason.CustomPropertiesReadOnly },
            };

        private static RackCustomPropertiesAuthorityResult ReadOnlyRack(string caso)
        {
            switch (caso)
            {
                case "xref":
                    return RackCustomPropertiesAuthority.Evaluate(new[] { View("1", properties: Base) }, Pick("X", fromExternalReference: true), KnownKinds);
                case "noidentity":
                    return Authority("1", View("1", rackId: "", properties: Base));
                case "indeterminate":
                    return Authority("1", View("1", properties: Base), Uninterpretable("9", placed: false));
                case "mixed":
                    return Authority("1", View("1", properties: Base), View("2", properties: Base, kind: null));
                case "unknown":
                    return Authority("1", View("1", properties: Base, kind: "futuro"));
                default:
                    return Authority("1", View("1", properties: Doc("1.0", "[]", ",\"P\":" + Nested(16))));
            }
        }

        /// <summary>
        /// Todo estado distinto de <c>Single</c> y de <c>Divergent</c> es solo lectura DESDE LA LECTURA, con su motivo: la
        /// ventana no ofrece ninguna operacion que despues falle al confirmar (D-18.3, C-1, C-2).
        /// </summary>
        [Theory]
        [MemberData(nameof(ReadOnlyRacks))]
        public void TAut16_RackDeSoloLectura_TieneMotivoYDiagnostico_YNingunaFila(string caso, CustomPropertiesReadOnlyReason reason)
        {
            var workspace = CustomPropertiesWorkspace.ForRack(ReadOnlyRack(caso));

            Assert.Equal(CustomPropertiesWorkspaceState.ReadOnly, workspace.State);
            Assert.Equal(reason, workspace.ReadOnlyReason);
            Assert.False(string.IsNullOrWhiteSpace(workspace.Diagnostic));
            Assert.Empty(workspace.Rows);
            Assert.False(workspace.UnifyAvailable);
        }

        [Fact]
        public void TAut16_RackIndeterminate_ListaLasDefinicionesIlegibles()
        {
            var workspace = CustomPropertiesWorkspace.ForRack(ReadOnlyRack("indeterminate"));

            var definition = Assert.Single(workspace.UninterpretableDefinitions);
            Assert.Equal("9", definition.Handle);
            Assert.Equal("RACK_9", definition.BlockName);
            Assert.False(definition.IsPlaced);
        }

        [Fact]
        public void TAut16_RackMixedYUnknown_TienenResumenPorVista_PeroNuncaFilas()
        {
            foreach (var caso in new[] { "mixed", "unknown" })
            {
                var workspace = CustomPropertiesWorkspace.ForRack(ReadOnlyRack(caso));

                Assert.NotEmpty(workspace.ViewSummaries);
                Assert.Empty(workspace.Rows);
            }
        }

        [Fact]
        public void TAut16_RackXrefYNoIdentity_NoTienenResumenes()
        {
            Assert.Empty(CustomPropertiesWorkspace.ForRack(ReadOnlyRack("xref")).ViewSummaries);
            Assert.Empty(CustomPropertiesWorkspace.ForRack(ReadOnlyRack("noidentity")).ViewSummaries);
        }

        // ================================================================ Proyecto

        [Fact]
        public void TAut16_ProyectoAbsent_EsEditableYVacio()
        {
            var workspace = CustomPropertiesWorkspace.ForProject(AbsentResult());

            Assert.Equal(CustomPropertiesWorkspaceState.Editable, workspace.State);
            Assert.Equal(CustomPropertiesScope.Project, workspace.Scope);
            Assert.Empty(workspace.Rows);
            Assert.Empty(workspace.ViewSummaries);
            Assert.False(string.IsNullOrWhiteSpace(workspace.ScopeLabel));
        }

        [Fact]
        public void TAut16_ProyectoReadable_EsEditable_ConFilas()
        {
            var workspace = CustomPropertiesWorkspace.ForProject(ReadText(Base));

            Assert.Equal(CustomPropertiesWorkspaceState.Editable, workspace.State);
            Assert.Equal(new[] { "Cliente", "Area" }, workspace.Rows.Select(row => row.Name));
        }

        public static TheoryData<string, CustomPropertiesReadOnlyReason> ReadOnlyProjects()
            => new TheoryData<string, CustomPropertiesReadOnlyReason>
            {
                { "[1]", CustomPropertiesReadOnlyReason.PresentButUnreadable },
                { Props(Entry(IdA, "Uno", "1"), Entry(IdA, "Dos", "2")), CustomPropertiesReadOnlyReason.AmbiguousIdentity },
                { EmptyAt("3.1"), CustomPropertiesReadOnlyReason.IncompatibleMajor },
                { Doc("1.0", "[]", ",\"P\":" + Nested(16)), CustomPropertiesReadOnlyReason.DepthLimitExceeded },
            };

        [Theory]
        [MemberData(nameof(ReadOnlyProjects))]
        public void TAut16_ProyectoNoEscribible_EsSoloLecturaConSuMotivo(string collection, CustomPropertiesReadOnlyReason reason)
        {
            var read = ReadText(collection);
            var workspace = CustomPropertiesWorkspace.ForProject(read);

            Assert.Equal(CustomPropertiesWorkspaceState.ReadOnly, workspace.State);
            Assert.Equal(reason, workspace.ReadOnlyReason);
            Assert.Equal(read.Error, workspace.Diagnostic);
            Assert.Empty(workspace.Rows);
        }

        /// <summary>El workspace es un modelo puro: ninguna propiedad publica expone tipos de un framework de UI.</summary>
        [Fact]
        public void TAut16_ElWorkspaceNoExponeTiposDeUi()
        {
            var types = new[]
            {
                typeof(CustomPropertiesWorkspace), typeof(CustomPropertiesRow), typeof(CustomPropertiesViewSummary),
                typeof(CustomPropertiesSummaryEntry),
            };

            foreach (var property in types.SelectMany(type => type.GetProperties()))
            {
                var ns = property.PropertyType.Namespace ?? string.Empty;
                Assert.False(ns.StartsWith("System.Windows", StringComparison.Ordinal), property.Name);
                Assert.False(ns.StartsWith("System.Xaml", StringComparison.Ordinal), property.Name);
            }
        }
    }
}
