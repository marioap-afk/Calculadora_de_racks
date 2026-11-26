using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Application.VentanaRackCad;
using Xunit;
using static RackCad.Tests.G8ContractTestSupport;

namespace RackCad.Tests
{
    /// <summary>
    /// I-64 A-14, cuarta tanda (va), correcciones de la revisión independiente: cada prueba reproduce un hallazgo del lote de la página
    /// «Variables» (<see cref="ProjectVariablesBatch"/>) o de su puerta, y fija la corrección.
    /// </summary>
    public class ProjectVariablesBatchReviewTests
    {
        private const string RackA = G9ContractTestSupport.RackA;

        private static ProjectVariablesReadResult Read(ProjectVariablesDocument document) => ProjectVariablesReadResult.Readable(document);

        private static ProjectVariablesDocument Holguras(string bNode = null)
            => ReadRegistry(RegistryJson(
                VariableJson(IdA, "Holgura General", LiteralDefinition(6.0)),
                VariableJson(IdB, "Holgura Alta", ExpressionDefinition(bNode ?? Binary("add", Reference(IdA), Number(2))))));

        private static List<ProjectVariableScanEntry> RackUsing(string variable)
            => new() { G9ContractTestSupport.View(G9ContractTestSupport.Design(RackA, directVariable: variable), "D-A", RackA) };

        private static ProjectVariablesBatchResult Plan(ProjectVariablesReadResult registry, IReadOnlyList<ProjectVariableScanEntry> entries,
            IReadOnlyList<string> repairs, params ProjectVariableDraftRow[] rows)
            => ProjectVariablesBatch.Plan(registry, entries, new ProjectVariablesBatchRequest(rows, repairs ?? Array.Empty<string>()));

        private static string Errors(ProjectVariablesBatchResult result) => string.Join(" | ", result.Errors.Select(e => e.Text));

        // ------------------------------------------------------------------ 1 [ALTO] «Eliminar» and the racks that use the variable

        [Fact]
        public void DeletingAVariableThatARackUsesIsRefusedAsTheSingleDelete()
        {
            // The page read no rack using «Holgura Alta»; RACKEDITAR linked one since. The command plans again on the sweep of now.
            var entries = RackUsing(IdB);
            var single = ProjectVariableMutationPreflight.Delete(Holguras(), VariableId.Parse(IdB), entries);
            Assert.False(single.IsSuccess);
            Assert.Equal(RackA, Assert.Single(single.BlockingConsumers).RackId);

            var batch = Plan(Read(Holguras()), entries, null, new ProjectVariableDraftRow(IdB, false, "Holgura Alta", null, ProjectVariableDraftRemoval.Delete));
            Assert.False(batch.IsSuccess);
            Assert.True(batch.Plan.IsEmpty);
            var error = Assert.Single(batch.Errors);
            Assert.Equal((IdB, ProjectVariableDraftField.Removal), (error.Key, error.Field));
            Assert.Equal("La usa Rack 3f2b, así que no se puede eliminar sin más: usa «Desvincular todos y eliminar…», que conserva su valor.", error.Text);
        }

        [Fact]
        public void DeletingAVariableNobodyUsesPlansTheRegisterOfTheSingleDelete()
        {
            var single = ProjectVariableMutationPreflight.Delete(Holguras(), VariableId.Parse(IdB), Array.Empty<ProjectVariableScanEntry>());
            var batch = Plan(Read(Holguras()), Array.Empty<ProjectVariableScanEntry>(), null,
                new ProjectVariableDraftRow(IdB, false, "Holgura Alta", null, ProjectVariableDraftRemoval.Delete));

            Assert.True(batch.IsSuccess, Errors(batch));
            Assert.Equal(
                ProjectVariableCloning.Canonical(single.Plan.RegistryMutation.ApplyTo(Holguras())),
                ProjectVariableCloning.Canonical(batch.Plan.RegistryMutation.Replacement));
        }

        // ------------------------------------------------------------------ 4 a removed variable gives up its name

        [Fact]
        public void ANewVariableCanTakeTheNameOfARemovedOneAndAFormulaCanUseIt()
        {
            var created = Plan(Read(Holguras()), Array.Empty<ProjectVariableScanEntry>(), null,
                new ProjectVariableDraftRow(IdB, false, "Holgura Alta", null, ProjectVariableDraftRemoval.Delete),
                new ProjectVariableDraftRow("nueva-1", true, "Holgura Alta", "3"),
                new ProjectVariableDraftRow("nueva-2", true, "Otra", "={§nueva-1§} + 1"));
            Assert.True(created.IsSuccess, Errors(created));
            Assert.Equal(4.0, created.FinalValues["nueva-2"]);

            var renamed = Plan(Read(Holguras()), Array.Empty<ProjectVariableScanEntry>(), null,
                new ProjectVariableDraftRow(IdB, false, "Holgura Alta", null, ProjectVariableDraftRemoval.Delete),
                new ProjectVariableDraftRow(IdA, false, "Holgura Alta", null),
                new ProjectVariableDraftRow("nueva-1", true, "Doble", "={§" + IdA + "§} * 2"));
            Assert.True(renamed.IsSuccess, Errors(renamed));
            Assert.Equal(12.0, renamed.FinalValues["nueva-1"]);
        }

        // ------------------------------------------------------------------ 5 repair and unlink on the same rack

        [Fact]
        public void ARackCanBeRepairedAndUnlinkedInTheSameDraft()
        {
            var registry = ReadRegistry(RegistryJson(VariableJson(IdA, "Tolerancia General", LiteralDefinition(4.0))));
            var design = G9ContractTestSupport.Design(RackA, directVariable: G9ContractTestSupport.MissingId);
            design.PropertyValues[ProjectPropertyIds.SelectivePalletToleranceToken] = SelectivePropertyValueDocument.ToProjectVariable(IdA);
            var entries = new List<ProjectVariableScanEntry> { G9ContractTestSupport.View(design, "D-A", RackA) };

            var batch = Plan(Read(registry), entries, new[] { RackA },
                new ProjectVariableDraftRow(IdA, false, "Tolerancia General", null, ProjectVariableDraftRemoval.UnlinkAllAndDelete));

            Assert.True(batch.IsSuccess, Errors(batch));
            var rack = Assert.Single(batch.Plan.RackMutations);
            Assert.True(rack.AuthoredOutput.PropertyValues == null || rack.AuthoredOutput.PropertyValues.Count == 0);
            Assert.Equal(4.0, rack.EffectiveOutput.PalletTolerance);
        }

        // ------------------------------------------------------------------ 6 VA-S5: what the draft does not touch

        [Fact]
        public void AnUntouchedVariableWhoseValueIsNotGreaterThanZeroDoesNotStopTheDraft()
        {
            // «Holgura Alta» = «Holgura General» - 10 is -4 in the drawing already; the draft only renames the other one.
            var registry = Holguras(Binary("sub", Reference(IdA), Number(10)));
            var batch = Plan(Read(registry), Array.Empty<ProjectVariableScanEntry>(), null, new ProjectVariableDraftRow(IdA, false, "Holgura Base", null));

            Assert.True(batch.IsSuccess, Errors(batch));
            Assert.Equal(RegistryMutationKind.Replace, batch.Plan.RegistryMutation.Kind);
        }

        // ------------------------------------------------------------------ 7 an unusable register denies the repairs too

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void AnUnusableRegisterPlansNothingNotEvenARepair(bool incompatible)
        {
            var registry = incompatible ? ProjectVariablesReadResult.IncompatibleMajor("versión 9") : ProjectVariablesReadResult.Unreadable("ilegible");
            var batch = Plan(registry, RackUsing(G9ContractTestSupport.MissingId), new[] { RackA });

            Assert.False(batch.IsSuccess);
            Assert.True(batch.Plan.IsEmpty);
            Assert.StartsWith("El registro de variables de este dibujo no se puede administrar: ", Assert.Single(batch.Errors).Text);
        }

        // ------------------------------------------------------------------ 2 VA-S7: the second transaction of the write

        [Fact]
        public void AWriteWhosePropertiesFailedSaysSoApartFromTheVariables()
        {
            var answer = VariablesDoorRules.AnswerForWrite(3, true, new[] { RackA }, 2, false, "las propiedades del proyecto cambiaron en el dibujo desde que se leyeron.");

            Assert.True(answer.Written);
            Assert.Equal("las propiedades del proyecto cambiaron en el dibujo desde que se leyeron.", answer.PropertiesError);
            Assert.Null(VariablesDoorRules.AnswerForWrite(3, true, new[] { RackA }, 2, true, null).PropertiesError);
        }

        [Fact]
        public void TheWriteOfTheProjectPropertiesHasItsOwnTryAfterTheVariablesAreConfirmed()
        {
            // A failure of the second transaction must not answer «No se pudo escribir» when the register and the racks are written.
            var code = WorkspaceHostSource.Code("src/RackCad.Plugin/VentanaRackCad/RackCadVariablesDoorCommands.cs");
            var execute = code.IndexOf("ProjectVariableMutationExecutor.Execute(", StringComparison.Ordinal);
            var write = code.IndexOf("CustomPropertiesExecutor.ExecuteProjectDraft(", StringComparison.Ordinal);
            Assert.True(execute > 0 && write > execute, "the properties are written after the variables, through the edge");
            var ownTry = code.LastIndexOf("try", write, StringComparison.Ordinal);
            Assert.True(ownTry > execute, "the write of the properties is not inside its own try");
        }
    }
}
