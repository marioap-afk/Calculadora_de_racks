using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.CustomProperties;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Workspace;
using Xunit;
using static RackCad.Tests.G8ContractTestSupport;

namespace RackCad.Tests
{
    /// <summary>
    /// I-64 A-14, cuarta tanda (va), VA-02: the page «Variables» writes its whole draft as ONE plan (<see cref="ProjectVariablesBatch"/>),
    /// judged on the final state with the rules of each operation; a batch of ONE operation plans what the single preflight plans. Also the
    /// commit of a replaced register, the key form of a formula, the draft of custom properties and the rules of the door of the page.
    /// </summary>
    public class ProjectVariablesBatchTests
    {
        private const string RackA = G9ContractTestSupport.RackA;
        private const string RackB = G9ContractTestSupport.RackB;

        private static ProjectVariablesReadResult Read(ProjectVariablesDocument document) => ProjectVariablesReadResult.Readable(document);

        private static ProjectVariablesDocument Holguras(double a = 6.0, string bNode = null)
            => ReadRegistry(RegistryJson(
                VariableJson(IdA, "Holgura General", LiteralDefinition(a)),
                VariableJson(IdB, "Holgura Alta", ExpressionDefinition(bNode ?? Binary("add", Reference(IdA), Number(2))))));

        private static List<ProjectVariableScanEntry> RackUsing(string variable, string rack = RackA)
            => new() { G9ContractTestSupport.View(G9ContractTestSupport.Design(rack, directVariable: variable), "D-" + rack.Substring(0, 4), rack) };

        private static ProjectVariablesBatchResult Plan(ProjectVariablesDocument registry, IReadOnlyList<ProjectVariableScanEntry> entries,
            params ProjectVariableDraftRow[] rows)
            => ProjectVariablesBatch.Plan(Read(registry), entries, new ProjectVariablesBatchRequest(rows, Array.Empty<string>()));

        private static ProjectVariablesDocument Final(ProjectVariablesBatchResult result)
        {
            Assert.Equal(RegistryMutationKind.Replace, result.Plan.RegistryMutation.Kind);
            return result.Plan.RegistryMutation.Replacement;
        }

        private static string Json(SelectivePalletDesignDocument document) => new SelectivePalletDesignStore().Serialize(document);

        // ------------------------------------------------------------------ one operation = the single preflight

        [Fact]
        public void AnEmptyDraftPlansNothing()
        {
            var result = Plan(Holguras(), RackUsing(IdA));

            Assert.True(result.IsSuccess);
            Assert.True(result.Plan.IsEmpty);
            Assert.False(result.HasChanges);
            Assert.Equal(6.0, result.BaseValues[IdA]);
            Assert.Equal(8.0, result.FinalValues[IdB]);
        }

        [Fact]
        public void ARenameTouchesOnlyTheRegisterAndTheFormulaFollowsByIdentity()
        {
            var result = Plan(Holguras(), RackUsing(IdA), new ProjectVariableDraftRow(IdA, false, "Holgura Base", null));

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Plan.RackMutations);
            var final = Final(result);
            Assert.Equal("Holgura Base", final.Variables.Single(v => v.VariableId == IdA).Name);
            Assert.Equal(8.0, result.FinalValues[IdB]);

            var single = ProjectVariableMutationPreflight.Rename(Holguras(), VariableId.Parse(IdA), "Holgura Base");
            Assert.Equal(
                ProjectVariableCloning.Canonical(single.Plan.RegistryMutation.ApplyTo(Holguras())),
                ProjectVariableCloning.Canonical(final));
        }

        [Fact]
        public void ChangingAValueRedrawsTheRackThatUsesItAsTheSingleChangeOfDefinition()
        {
            var entries = RackUsing(IdA);
            var result = Plan(Holguras(), entries, new ProjectVariableDraftRow(IdA, false, "Holgura General", "7"));

            Assert.True(result.IsSuccess, string.Join(" | ", result.Errors.Select(e => e.Text)));
            Assert.Equal(new[] { RackA }, result.RedrawnRackIds);
            Assert.Equal(9.0, result.FinalValues[IdB]);

            var single = ProjectVariableMutationPreflight.ChangeDefinition(Holguras(), VariableId.Parse(IdA), VariableDefinition.Literal(7.0), entries);
            Assert.True(single.IsSuccess, single.Error);
            var mine = result.Plan.RackMutations.Single();
            var theirs = single.Plan.RackMutations.Single();
            Assert.Equal(Json(theirs.AuthoredOutput), Json(mine.AuthoredOutput));
            Assert.Equal(theirs.EffectiveOutput.VerticalClearance, mine.EffectiveOutput.VerticalClearance);
            Assert.Equal(7.0, mine.EffectiveOutput.VerticalClearance);
            Assert.Equal(
                ProjectVariableCloning.Canonical(single.Plan.RegistryMutation.ApplyTo(Holguras())),
                ProjectVariableCloning.Canonical(Final(result)));
        }

        [Fact]
        public void UnlinkAllAndDeleteKeepsTheValueOfNowInEveryRackAsTheSingleOne()
        {
            var registry = ReadRegistry(RegistryJson(VariableJson(IdA, "Holgura General", LiteralDefinition(6.5))));
            var entries = RackUsing(IdA);
            var result = Plan(registry, entries, new ProjectVariableDraftRow(IdA, false, "Holgura General", null, ProjectVariableDraftRemoval.UnlinkAllAndDelete));

            Assert.True(result.IsSuccess, string.Join(" | ", result.Errors.Select(e => e.Text)));
            Assert.Equal(6.5, result.KeptValues[IdA]);
            Assert.Empty(Final(result).Variables);

            var single = ProjectVariableMutationPreflight.UnlinkAllAndDelete(registry, VariableId.Parse(IdA), entries);
            Assert.True(single.IsSuccess, single.Error);
            var mine = result.Plan.RackMutations.Single();
            Assert.Equal(Json(single.Plan.RackMutations.Single().AuthoredOutput), Json(mine.AuthoredOutput));
            Assert.Equal(6.5, mine.EffectiveOutput.VerticalClearance);
            Assert.False(mine.AuthoredOutput.PropertyValues != null && mine.AuthoredOutput.PropertyValues.Count > 0);
        }

        [Fact]
        public void RepairingARackRemovesItsBrokenBindingsAsTheSingleRepair()
        {
            var registry = ReadRegistry(RegistryJson(VariableJson(IdA, "Holgura General", LiteralDefinition(6.0))));
            var entries = RackUsing(G9ContractTestSupport.MissingId);
            var result = ProjectVariablesBatch.Plan(Read(registry), entries,
                new ProjectVariablesBatchRequest(Array.Empty<ProjectVariableDraftRow>(), new[] { RackA }));

            Assert.True(result.IsSuccess, string.Join(" | ", result.Errors.Select(e => e.Text)));
            Assert.Equal(RegistryMutationKind.None, result.Plan.RegistryMutation.Kind);
            var single = ProjectVariableMutationPreflight.RepairBrokenRack(registry, RackA, entries, true);
            Assert.Equal(Json(single.Plan.RackMutations.Single().AuthoredOutput), Json(result.Plan.RackMutations.Single().AuthoredOutput));
        }

        // ------------------------------------------------------------------ the final state, not every step

        [Fact]
        public void ADraftValidAsAWholeIsAcceptedThoughOneOfItsStepsAloneIsNot()
        {
            // B = A - 5 (1 in). Lowering A to 3 alone would leave B at -2; the draft also changes B to A + 1.
            var registry = Holguras(6.0, Binary("sub", Reference(IdA), Number(5)));
            var alone = Plan(registry, Array.Empty<ProjectVariableScanEntry>(), new ProjectVariableDraftRow(IdA, false, "Holgura General", "3"));
            Assert.False(alone.IsSuccess);
            Assert.Contains(alone.Errors, e => e.Key == IdB && e.Text.StartsWith("Con este borrador deja de tener valor.", StringComparison.Ordinal));

            var result = Plan(registry, Array.Empty<ProjectVariableScanEntry>(),
                new ProjectVariableDraftRow(IdA, false, "Holgura General", "3"),
                new ProjectVariableDraftRow(IdB, false, "Holgura Alta", "={§" + IdA + "§} + 1"));

            Assert.True(result.IsSuccess, string.Join(" | ", result.Errors.Select(e => e.Text)));
            Assert.Equal(4.0, result.FinalValues[IdB]);
        }

        [Fact]
        public void ANewVariableCanUseAnotherNewOneByItsKeyAndTheirIdsAreMintedHere()
        {
            var result = Plan(Holguras(), Array.Empty<ProjectVariableScanEntry>(),
                new ProjectVariableDraftRow("nueva-1", true, "Base", "10"),
                new ProjectVariableDraftRow("nueva-2", true, "Doble", "={§nueva-1§} * 2"));

            Assert.True(result.IsSuccess, string.Join(" | ", result.Errors.Select(e => e.Text)));
            Assert.Equal(20.0, result.FinalValues["nueva-2"]);
            var final = Final(result);
            Assert.Equal(4, final.Variables.Count);
            Assert.All(final.Variables, v => Assert.True(VariableId.TryParse(v.VariableId, out _)));
            Assert.DoesNotContain(final.Variables, v => v.VariableId.StartsWith("nueva", StringComparison.Ordinal));
        }

        [Fact]
        public void AValueThatDoesNotChangeRedrawsNoRack()
        {
            var result = Plan(Holguras(), RackUsing(IdA), new ProjectVariableDraftRow(IdA, false, "Holgura General", "=3 * 2"));

            Assert.True(result.IsSuccess, string.Join(" | ", result.Errors.Select(e => e.Text)));
            Assert.Empty(result.RedrawnRackIds);
            Assert.Equal(RegistryMutationKind.Replace, result.Plan.RegistryMutation.Kind);
        }

        // ------------------------------------------------------------------ the errors, by field and in Spanish

        [Fact]
        public void RepeatedNamesAreNotAdmittedAndBothRowsSaySo()
        {
            // VA-05 of the Owner.
            var result = Plan(Holguras(), Array.Empty<ProjectVariableScanEntry>(), new ProjectVariableDraftRow(IdB, false, "holgura general", null));

            Assert.False(result.IsSuccess);
            Assert.True(result.Plan.IsEmpty);
            Assert.Contains(result.Errors, e => e.Key == IdA && e.Field == ProjectVariableDraftField.Name);
            Assert.Contains(result.Errors, e => e.Key == IdB && e.Field == ProjectVariableDraftField.Name && e.Text == "Ya hay otra variable con este nombre.");
        }

        [Theory]
        [InlineData("6,5", "Usa punto decimal: 6.5.")]
        [InlineData("0", "Tiene que ser mayor que cero.")]
        [InlineData("=", "Después de «=» va la fórmula.")]
        [InlineData("=Holgura Inexistente + 1", "No hay ninguna variable llamada «Holgura Inexistente».")]
        [InlineData("=FOO(2)", "Función desconocida «FOO»: solo hay MIN, MAX y ABS.")]
        [InlineData("=2 - 3", "El resultado tiene que ser mayor que cero (da -1).")]
        public void AWrongDefinitionSaysWhatIsWrongInItsField(string text, string expected)
        {
            var result = Plan(Holguras(), Array.Empty<ProjectVariableScanEntry>(), new ProjectVariableDraftRow(IdA, false, "Holgura General", text));

            Assert.False(result.IsSuccess);
            var error = Assert.Single(result.Errors, e => e.Key == IdA && e.Field == ProjectVariableDraftField.Definition);
            Assert.Equal(expected, error.Text);
            Assert.DoesNotContain(IdA, error.Text, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void ACycleNamesItsVariables()
        {
            var result = Plan(Holguras(), Array.Empty<ProjectVariableScanEntry>(),
                new ProjectVariableDraftRow(IdA, false, "Holgura General", "={§" + IdB + "§} - 1"));

            Assert.False(result.IsSuccess);
            var error = result.Errors.First(e => e.Key == IdA && e.Field == ProjectVariableDraftField.Definition);
            Assert.StartsWith("La fórmula forma un ciclo", error.Text);
        }

        [Fact]
        public void DeletingIsRefusedWhileAFormulaUsesItAndNamesTheFormula()
        {
            var result = Plan(Holguras(), Array.Empty<ProjectVariableScanEntry>(),
                new ProjectVariableDraftRow(IdA, false, "Holgura General", null, ProjectVariableDraftRemoval.Delete));

            Assert.False(result.IsSuccess);
            Assert.Contains(result.Errors, e => e.Key == IdA && e.Field == ProjectVariableDraftField.Removal
                                               && e.Text == "La usa la fórmula de «Holgura Alta»: cámbiala antes de eliminar esta variable.");
            Assert.Equal(new[] { IdB }, result.Dependents[IdA]);
        }

        [Fact]
        public void AFormulaThatUsesARemovedVariableSaysSo()
        {
            var result = Plan(Holguras(), Array.Empty<ProjectVariableScanEntry>(),
                new ProjectVariableDraftRow(IdA, false, "Holgura General", null, ProjectVariableDraftRemoval.Delete),
                new ProjectVariableDraftRow("nueva-1", true, "Otra", "={§" + IdA + "§}"));

            Assert.Contains(result.Errors, e => e.Key == "nueva-1" && e.Text == "Usa «Holgura General», que se eliminará.");
        }

        [Fact]
        public void TheFormulaOfTheDependentCanBeChangedInTheSameDraftAsTheDelete()
        {
            var result = Plan(Holguras(), Array.Empty<ProjectVariableScanEntry>(),
                new ProjectVariableDraftRow(IdA, false, "Holgura General", null, ProjectVariableDraftRemoval.Delete),
                new ProjectVariableDraftRow(IdB, false, "Holgura Alta", "8"));

            Assert.True(result.IsSuccess, string.Join(" | ", result.Errors.Select(e => e.Text)));
            Assert.Single(Final(result).Variables);
        }

        // ------------------------------------------------------------------ the commit of a replaced register

        [Fact]
        public void TheCommitWritesTheReplacementOnlyOverTheRegisterTheBatchWasPlannedAgainst()
        {
            var result = Plan(Holguras(), Array.Empty<ProjectVariableScanEntry>(), new ProjectVariableDraftRow(IdA, false, "Holgura General", "7"));

            var same = RegistryCommit.Prepare(result.Plan, Read(Holguras()), Array.Empty<ProjectVariableScanEntry>());
            Assert.True(same.IsReady, same.Error);
            Assert.Equal(7.0, same.Changed.Variables.Single(v => v.VariableId == IdA).Definition.Value);

            var moved = RegistryCommit.Prepare(result.Plan, Read(Holguras(5.0)), Array.Empty<ProjectVariableScanEntry>());
            Assert.True(moved.IsBlocked);
            Assert.Null(moved.Changed);
        }

        // ------------------------------------------------------------------ the key form

        [Fact]
        public void TheKeyFormFollowsTheVariablesByIdentityAndOnlyTouchesNames()
        {
            var keys = new List<(string, string)> { ("k1", "Holgura General"), ("k2", "Alta") };
            var encoded = ProjectVariableFormulaText.Encode("=MAX(Holgura General, {Alta}) + 100[mm]", keys);

            Assert.Equal("=MAX({§k1§}, {§k2§}) + 100[mm]", encoded);
            Assert.Equal(new[] { "k1", "k2" }, ProjectVariableFormulaText.Keys(encoded));
            Assert.Equal("=MAX(Base, {Holgura 2.0}) + 100[mm]",
                ProjectVariableFormulaText.Decode(encoded, key => key == "k1" ? "Base" : "Holgura 2.0"));
            Assert.Equal("6.5", ProjectVariableFormulaText.Encode(" 6.5 ", keys));
            Assert.Equal("2.3", ProjectVariableFormulaText.Literal(2.3));
        }

        // ------------------------------------------------------------------ custom properties as one draft

        private static CustomPropertiesReadResult Collection(params (string Id, string Name, string Value)[] entries)
        {
            var document = CustomPropertiesDocument.CreateNew();
            foreach (var (id, name, value) in entries) document.Entries.Add(new CustomPropertyEntryDocument { Id = id, Name = name, Value = value });
            return CustomPropertiesReadResult.Readable(document, Array.Empty<CustomPropertyId>());
        }

        private const string P1 = "6f1c2a3b-0000-4000-8000-000000000001";
        private const string P2 = "6f1c2a3b-0000-4000-8000-000000000002";

        [Fact]
        public void TwoPropertiesCanSwapTheirNamesAndANewOneIsAddedAtTheEnd()
        {
            var current = Collection((P1, "Zona", "A"), (P2, "Nota", "x"));
            var result = CustomPropertiesBatch.Apply(current, new[]
            {
                new CustomPropertyDraftEntry(P1, "Nota", "A"),
                new CustomPropertyDraftEntry(P2, "Zona", "y"),
                new CustomPropertyDraftEntry(null, "Revisión", "B"),
            });

            Assert.True(result.Succeeded, result.Error);
            Assert.Equal(new[] { "Nota", "Zona", "Revisión" }, result.Document.Entries.Select(e => e.Name));
            Assert.Equal(new[] { P1, P2 }, result.Document.Entries.Take(2).Select(e => e.Id));
            Assert.Equal("y", result.Document.Entries[1].Value);
            Assert.True(CustomPropertiesBatch.IsUnchanged(current.Document, CustomPropertiesBatch.DraftOf(current.Document)));
        }

        [Fact]
        public void TheProblemsOfAPropertyDraftAreSaidByRowAndField()
        {
            var problems = CustomPropertiesBatch.Check(Collection((P1, "Zona", "A")), new[]
            {
                new CustomPropertyDraftEntry(P1, "Zona", "A"),
                new CustomPropertyDraftEntry(null, " zona ", ""),
                new CustomPropertyDraftEntry(null, "", new string('x', 1001)),
            });

            Assert.Contains(problems, p => p.Index == 1 && p.Field == CustomPropertyDraftField.Name && p.Text == "Ya existe otra propiedad llamada «zona».");
            Assert.Contains(problems, p => p.Index == 2 && p.Field == CustomPropertyDraftField.Name);
            Assert.Contains(problems, p => p.Index == 2 && p.Field == CustomPropertyDraftField.Value);
            Assert.DoesNotContain(problems, p => p.Index == 0);
        }

        [Fact]
        public void APropertyThatIsDeletedFromTheDraftLeavesTheCollection()
        {
            var result = CustomPropertiesBatch.Apply(Collection((P1, "Zona", "A"), (P2, "Nota", "x")), new[] { new CustomPropertyDraftEntry(P2, "Nota", "x") });

            Assert.True(result.Succeeded, result.Error);
            Assert.Equal(new[] { P2 }, result.Document.Entries.Select(e => e.Id));
        }

        // ------------------------------------------------------------------ the rules of the door of «Variables»

        private static VariablesDoorWrite Write(ProjectVariablesDocument registry, CustomPropertiesReadResult properties,
            IReadOnlyList<CustomPropertyDraftEntry> draft, params ProjectVariableDraftRow[] rows)
            => new(1, new SessionId(7), VariablesDoorRules.RegistryFingerprint(Read(registry)),
                new ProjectVariablesBatchRequest(rows, Array.Empty<string>()), VariablesDoorRules.PropertiesFingerprint(properties), draft);

        [Fact]
        public void TheDoorWritesOnlyOverWhatThePageRead()
        {
            var properties = Collection((P1, "Zona", "A"));
            var write = Write(Holguras(), properties, null, new ProjectVariableDraftRow(IdA, false, "Holgura General", "7"));

            var plan = VariablesDoorRules.PlanWrite(Read(Holguras()), RackUsing(IdA), properties, write);
            Assert.True(plan.CanWrite, plan.Reason);
            Assert.Equal(new[] { RackA }, plan.RedrawnRackIds);
            Assert.Null(plan.ProjectProperties);

            var moved = VariablesDoorRules.PlanWrite(Read(Holguras(5.0)), RackUsing(IdA), properties, write);
            Assert.False(moved.CanWrite);
            Assert.StartsWith("El registro de variables cambió en el dibujo desde que se leyó la página", moved.Reason);
            Assert.True(moved.Variables.IsEmpty);
        }

        [Fact]
        public void TheDoorWritesTheProjectPropertiesWithTheVariablesAndRefusesAnythingThatChangedOrFails()
        {
            var properties = Collection((P1, "Zona", "A"));
            var draft = new[] { new CustomPropertyDraftEntry(P1, "Zona", "B") };

            var only = VariablesDoorRules.PlanWrite(Read(Holguras()), RackUsing(IdA), properties, Write(Holguras(), properties, draft));
            Assert.True(only.CanWrite, only.Reason);
            Assert.True(only.Variables.IsEmpty);
            Assert.Equal("B", only.ProjectProperties.Entries.Single().Value);

            var changed = VariablesDoorRules.PlanWrite(Read(Holguras()), RackUsing(IdA), Collection((P1, "Zona", "Z")), Write(Holguras(), properties, draft));
            Assert.False(changed.CanWrite);

            var bad = VariablesDoorRules.PlanWrite(Read(Holguras()), RackUsing(IdA), properties,
                Write(Holguras(), properties, new[] { new CustomPropertyDraftEntry(P1, "Zona", "A"), new CustomPropertyDraftEntry(null, "zona", "") }));
            Assert.False(bad.CanWrite);
            Assert.StartsWith("Propiedades del proyecto: ", bad.Reason);

            var nothing = VariablesDoorRules.PlanWrite(Read(Holguras()), RackUsing(IdA), properties, Write(Holguras(), properties, null));
            Assert.False(nothing.CanWrite);
            Assert.Equal("El borrador no cambia nada del dibujo.", nothing.Reason);
        }

        [Fact]
        public void TheAnswerSaysHowManyRacksWereRedrawnAndThatOneUUndoesItAll()
        {
            var answer = VariablesDoorRules.AnswerForWrite(3, true, new[] { RackA, RackB }, 5, true, null);
            Assert.True(answer.Written);
            Assert.Equal("AutoCAD escribió el borrador de «Variables» y redibujó 2 racks (5 vistas). Un «U» en AutoCAD lo deshace todo.", answer.Message);

            var partial = VariablesDoorRules.AnswerForWrite(3, true, new[] { RackA }, 2, false, "las propiedades del proyecto cambiaron en el dibujo desde que se leyeron.");
            Assert.True(partial.Written);
            Assert.StartsWith("AutoCAD escribió las variables, pero no las propiedades del proyecto", partial.Message);
        }
    }
}
