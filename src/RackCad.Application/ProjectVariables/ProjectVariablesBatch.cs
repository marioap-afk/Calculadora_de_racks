using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using RackCad.Application.Expressions;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Selective;
using RackCad.Application.Units;

namespace RackCad.Application.ProjectVariables
{
    /// <summary>What the draft of the page «Variables» does with a variable that it removes (I-64 A-14, cuarta tanda, va).</summary>
    public enum ProjectVariableDraftRemoval
    {
        None = 0,

        /// <summary>«Eliminar»: only when no rack and no formula uses it, as the single delete.</summary>
        Delete = 1,

        /// <summary>«Desvincular todos y eliminar…»: every rack that uses it keeps the value it has NOW, then it is removed.</summary>
        UnlinkAllAndDelete = 2,
    }

    /// <summary>
    /// One variable of the draft of the page «Variables». <see cref="Key"/> is the <see cref="VariableId"/> of a variable of the drawing,
    /// or a key of the draft for a new one (the id of a new variable is minted by <see cref="ProjectVariablesBatch"/>, never by the page).
    /// <see cref="DefinitionText"/> is in KEY FORM (<see cref="ProjectVariableFormulaText"/>): a number in inches, or «=» and a formula whose
    /// references are «{§key§}», so a rename never breaks it; null on a variable of the drawing means «unchanged».
    /// </summary>
    public sealed record ProjectVariableDraftRow(
        string Key,
        bool IsNew,
        string Name,
        string DefinitionText,
        ProjectVariableDraftRemoval Removal = ProjectVariableDraftRemoval.None);

    /// <summary>The whole draft of the page: its variables and the racks whose broken references it repairs.</summary>
    public sealed record ProjectVariablesBatchRequest(IReadOnlyList<ProjectVariableDraftRow> Rows, IReadOnlyList<string> RepairRacks);

    /// <summary>The field of a row an error belongs to.</summary>
    public enum ProjectVariableDraftField
    {
        None = 0,
        Name = 1,
        Definition = 2,
        Removal = 3,
        Repair = 4,
        Rack = 5,
    }

    /// <summary>
    /// One reason the draft cannot be written, in Spanish and naming variables and racks by their name, never by an id (VA-09).
    /// <see cref="Key"/> is the key of the row, or the id of the rack for <see cref="ProjectVariableDraftField.Repair"/> and
    /// <see cref="ProjectVariableDraftField.Rack"/>; null for the whole register.
    /// </summary>
    public sealed record ProjectVariableDraftError(string Key, ProjectVariableDraftField Field, string Text);

    /// <summary>What a batch would write, or why it would write nothing.</summary>
    public sealed class ProjectVariablesBatchResult
    {
        internal ProjectVariablesBatchResult(
            IReadOnlyList<ProjectVariableDraftError> errors,
            MutationPlan plan,
            IReadOnlyList<string> redrawnRackIds,
            IReadOnlyDictionary<string, double?> finalValues,
            IReadOnlyDictionary<string, double?> baseValues,
            IReadOnlyDictionary<string, IReadOnlyList<string>> dependents,
            IReadOnlyDictionary<string, double?> keptValues)
        {
            Errors = errors;
            Plan = errors.Count == 0 ? plan ?? MutationPlan.Empty : MutationPlan.Empty;
            RedrawnRackIds = redrawnRackIds;
            FinalValues = finalValues;
            BaseValues = baseValues;
            Dependents = dependents;
            KeptValues = keptValues;
        }

        public bool IsSuccess => Errors.Count == 0;

        public IReadOnlyList<ProjectVariableDraftError> Errors { get; }

        /// <summary>The ONE plan of the whole draft: the register replaced and every rack that changes. Empty on any error.</summary>
        public MutationPlan Plan { get; }

        public bool HasChanges => IsSuccess && !Plan.IsEmpty;

        /// <summary>The racks the plan redraws, in order of their id. Empty on any error.</summary>
        public IReadOnlyList<string> RedrawnRackIds { get; }

        /// <summary>The value of each live variable after the draft (by key); null when it has none.</summary>
        public IReadOnlyDictionary<string, double?> FinalValues { get; }

        /// <summary>The value of each variable of the drawing now (by key); null when it has none.</summary>
        public IReadOnlyDictionary<string, double?> BaseValues { get; }

        /// <summary>The live variables whose formula uses each variable (by key).</summary>
        public IReadOnlyDictionary<string, IReadOnlyList<string>> Dependents { get; }

        /// <summary>For each «Desvincular todos y eliminar», the value its racks keep (by key).</summary>
        public IReadOnlyDictionary<string, double?> KeptValues { get; }
    }

    /// <summary>
    /// The batch of the page «Variables» (I-64 A-14, cuarta tanda, va; VA-02 of the Owner): the whole draft is ONE plan — one order of the
    /// door, one «U» — or nothing. PURE.
    ///
    /// <para>
    /// <b>Declared change.</b> Until now Application planned one operation at a time (<see cref="ProjectVariableMutationPreflight"/>), each
    /// over the register as it was. Chaining those single plans would judge every intermediate state, and a draft that is valid as a whole
    /// can pass through invalid ones (lower a variable that a formula subtracts from while changing that formula). So the batch judges the
    /// FINAL state with the rules of each operation, and reuses their building blocks: the repair of a rack is
    /// <see cref="ProjectVariableMutationPreflight.RepairBrokenRack"/>; «Desvincular todos y eliminar» materialises the CURRENT effective
    /// value of each bound property, as the single one does; consumers are found by <see cref="ProjectVariableConsumerDiscovery"/>, and
    /// each rack is resolved against the final register by the same resolver.
    /// </para>
    /// <para>The rules, on the final state:</para>
    /// <list type="bullet">
    /// <item>every live variable has a name, and no two share one, compared without case (VA-05: the page does not admit them);</item>
    /// <item>a new or changed definition evaluates, to a value greater than zero;</item>
    /// <item>a variable that had a value still has one;</item>
    /// <item>«Eliminar» only with no rack and no formula using it; «Desvincular todos y eliminar» only with no formula using it and no
    /// property expression depending on it;</item>
    /// <item>every rack that changes resolves against the final register.</item>
    /// </list>
    /// <para>
    /// A rack is redrawn when its authored design changes (a repair, an unlink) or when a variable it reads changes its RESULT; a rack whose
    /// variable keeps its value is not redrawn (the single change of definition redraws every consumer; declared).
    /// </para>
    /// </summary>
    public static class ProjectVariablesBatch
    {
        private static readonly SelectiveEffectiveDesignResolver Resolver = new SelectiveEffectiveDesignResolver();

        public static ProjectVariablesBatchResult Plan(
            ProjectVariablesReadResult registry,
            IReadOnlyList<ProjectVariableScanEntry> entries,
            ProjectVariablesBatchRequest request)
        {
            var errors = new List<ProjectVariableDraftError>();
            var finalValues = new Dictionary<string, double?>(StringComparer.OrdinalIgnoreCase);
            var baseValues = new Dictionary<string, double?>(StringComparer.OrdinalIgnoreCase);
            var dependents = new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase);
            var kept = new Dictionary<string, double?>(StringComparer.OrdinalIgnoreCase);
            var noRacks = Array.Empty<string>();
            entries ??= Array.Empty<ProjectVariableScanEntry>();
            var rows = request?.Rows ?? Array.Empty<ProjectVariableDraftRow>();
            var repairs = (request?.RepairRacks ?? Array.Empty<string>())
                .Where(rack => !string.IsNullOrWhiteSpace(rack)).Select(rack => rack.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            ProjectVariablesBatchResult Done(MutationPlan plan = null, IReadOnlyList<string> redrawn = null)
                => new ProjectVariablesBatchResult(errors, plan, errors.Count == 0 ? redrawn ?? noRacks : noRacks, finalValues, baseValues, dependents, kept);

            var baseAccreditation = UsableProjectVariablesRegistry.Accredit(registry);
            if (!baseAccreditation.IsUsable)
            {
                // A register this build cannot administer (PresentButUnreadable, IncompatibleMajor, an ambiguous identity): nothing is
                // planned, repairs included. A repair resolves the rack against the register, and an unreadable one is not an empty one
                // (I-47, ADR-0034 §8; correction 7 of the review of the cuarta tanda).
                if (rows.Any(row => row.IsNew || row.Removal != ProjectVariableDraftRemoval.None || row.DefinitionText != null) || repairs.Count > 0)
                {
                    errors.Add(new ProjectVariableDraftError(null, ProjectVariableDraftField.None,
                        "El registro de variables de este dibujo no se puede administrar: " + (baseAccreditation.Error ?? "no se pudo leer.")));
                }

                return Done();
            }

            var baseDocument = registry?.Document ?? ProjectVariablesDocument.CreateNew();
            var baseUsable = baseAccreditation.IsUsable ? baseAccreditation.Registry : null;
            var baseEvaluation = baseUsable == null ? null : RegistryEvaluation.Evaluate(ProjectVariablesExpressionAdapter.From(baseUsable));
            var baseTargets = baseUsable == null ? new List<VariableTargetSnapshot>() : baseUsable.Targets().ToList();
            foreach (var target in baseTargets)
                baseValues[target.VariableId.Value] = ValueOf(baseEvaluation.Result(SymbolId.ProjectVariable(target.VariableId.Value)));

            // ---------------------------------------------------------------- the rows, keyed
            var byKey = new Dictionary<string, ProjectVariableDraftRow>(StringComparer.OrdinalIgnoreCase);
            foreach (var row in rows)
            {
                if (row == null || string.IsNullOrWhiteSpace(row.Key)) continue;
                if (byKey.ContainsKey(row.Key.Trim()))
                {
                    errors.Add(new ProjectVariableDraftError(row.Key, ProjectVariableDraftField.None, "El borrador repite una variable."));
                    continue;
                }

                if (!row.IsNew && (!VariableId.TryParse(row.Key.Trim(), out var existing) || baseUsable == null || !baseUsable.TryGetTarget(existing, out _)))
                {
                    errors.Add(new ProjectVariableDraftError(row.Key, ProjectVariableDraftField.None,
                        "Esta variable ya no está en el registro que se leyó del dibujo: vuelve a leer la página."));
                    continue;
                }

                byKey[row.Key.Trim()] = row;
            }

            if (errors.Count > 0) return Done();

            // Every variable of the drawing is in the draft, as it is when the page did not send its row.
            var all = new List<ProjectVariableDraftRow>();
            foreach (var target in baseTargets)
            {
                all.Add(byKey.TryGetValue(target.VariableId.Value, out var row)
                    ? row with { Key = target.VariableId.Value }
                    : new ProjectVariableDraftRow(target.VariableId.Value, false, target.Name, null));
            }

            all.AddRange(byKey.Values.Where(row => row.IsNew).Select(row => row with { Key = row.Key.Trim() }));
            var live = all.Where(row => row.Removal == ProjectVariableDraftRemoval.None).ToList();
            var removed = new HashSet<string>(all.Where(row => !row.IsNew && row.Removal != ProjectVariableDraftRemoval.None).Select(row => row.Key),
                StringComparer.OrdinalIgnoreCase);

            // ---------------------------------------------------------------- names (VA-05)
            var finalName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var seen = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
            foreach (var row in all)
            {
                var name = (row.Name ?? string.Empty).Trim();
                finalName[row.Key] = name;
                if (row.Removal != ProjectVariableDraftRemoval.None) continue;
                if (name.Length == 0)
                {
                    errors.Add(new ProjectVariableDraftError(row.Key, ProjectVariableDraftField.Name, "Una variable necesita un nombre."));
                    continue;
                }

                if (name.IndexOf('{') >= 0 || name.IndexOf('}') >= 0)
                {
                    errors.Add(new ProjectVariableDraftError(row.Key, ProjectVariableDraftField.Name, "El nombre no puede llevar llaves «{ }»."));
                    continue;
                }

                if (seen.TryGetValue(name, out var other))
                {
                    errors.Add(new ProjectVariableDraftError(row.Key, ProjectVariableDraftField.Name, "Ya hay otra variable con este nombre."));
                    if (!errors.Any(e => e.Key == other && e.Field == ProjectVariableDraftField.Name))
                        errors.Add(new ProjectVariableDraftError(other, ProjectVariableDraftField.Name, "Ya hay otra variable con este nombre."));
                    continue;
                }

                seen[name] = row.Key;
            }

            // ---------------------------------------------------------------- the register with every row (names, then definitions)
            var idOf = new Dictionary<string, VariableId>(StringComparer.OrdinalIgnoreCase);
            var document = ProjectVariableCloning.Clone(baseDocument) ?? ProjectVariablesDocument.CreateNew();
            foreach (var row in all)
            {
                if (!row.IsNew)
                {
                    var id = VariableId.Parse(row.Key);
                    idOf[row.Key] = id;
                    var target = baseTargets.First(t => t.VariableId.Equals(id));
                    if (finalName[row.Key].Length > 0 && !string.Equals(finalName[row.Key], target.Name, StringComparison.Ordinal))
                        document = RegistryMutation.Rename(id, finalName[row.Key]).ApplyTo(document);
                    continue;
                }

                if (row.Removal != ProjectVariableDraftRemoval.None) continue;
                var minted = VariableId.New();
                while (baseUsable != null && baseUsable.TryGetTarget(minted, out _)) minted = VariableId.New();
                idOf[row.Key] = minted;

                // A provisional literal until its definition is bound below, so the names are all there to bind against.
                document = RegistryMutation.Add(ProjectVariable.Create(
                    minted, finalName[row.Key].Length > 0 ? finalName[row.Key] : "(sin nombre)", VariableType.Length, VariableDefinition.Literal(1.0)))
                    .ApplyTo(document);
            }

            var redefined = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // A removed variable gives up its name: another one may take it, or a new one, in the same draft (correction 4 of the
            // review). A formula that uses a removed variable is refused below by its key, before it is bound.
            var bindingDocument = document;
            foreach (var row in all.Where(r => !r.IsNew && r.Removal != ProjectVariableDraftRemoval.None))
                bindingDocument = RegistryMutation.Remove(idOf[row.Key]).ApplyTo(bindingDocument);
            var namesForBinding = Accredited(bindingDocument);
            foreach (var row in live)
            {
                if (!row.IsNew && row.DefinitionText == null) continue;
                var text = ProjectVariableFormulaText.Substitute(row.DefinitionText, key =>
                    !idOf.ContainsKey(key) && !byKey.ContainsKey(key) ? null : finalName.TryGetValue(key, out var n) ? n : null, out var missing);
                if (missing != null)
                {
                    errors.Add(new ProjectVariableDraftError(row.Key, ProjectVariableDraftField.Definition, missing));
                    continue;
                }

                var removedUse = ProjectVariableFormulaText.Keys(row.DefinitionText).FirstOrDefault(key => removed.Contains(key)
                    || (byKey.TryGetValue(key, out var r) && r.Removal != ProjectVariableDraftRemoval.None));
                if (removedUse != null)
                {
                    errors.Add(new ProjectVariableDraftError(row.Key, ProjectVariableDraftField.Definition,
                        "Usa «" + Display(finalName, removedUse) + "», que se eliminará."));
                    continue;
                }

                if (!TryBind(text, namesForBinding, out var definition, out var error))
                {
                    errors.Add(new ProjectVariableDraftError(row.Key, ProjectVariableDraftField.Definition, error));
                    continue;
                }

                document = RegistryMutation.ChangeValue(idOf[row.Key], definition).ApplyTo(document);
                redefined.Add(row.Key);
            }

            // Every row, removed ones included: who uses whom (the formulas of the draft).
            var withAll = Accredited(document);
            var allEvaluation = withAll == null ? null : RegistryEvaluation.Evaluate(ProjectVariablesExpressionAdapter.From(withAll));
            var keyOfId = idOf.ToDictionary(pair => pair.Value.Value, pair => pair.Key, StringComparer.OrdinalIgnoreCase);
            foreach (var row in all.Where(r => idOf.ContainsKey(r.Key)))
            {
                dependents[row.Key] = allEvaluation == null
                    ? Array.Empty<string>()
                    : allEvaluation.DependencyGraph.DirectDependents(SymbolId.ProjectVariable(idOf[row.Key].Value))
                        .Select(symbol => keyOfId.TryGetValue(symbol.Key, out var k) ? k : null)
                        .Where(k => k != null && live.Any(l => string.Equals(l.Key, k, StringComparison.OrdinalIgnoreCase))).ToList();
            }

            // ---------------------------------------------------------------- removals
            foreach (var row in all.Where(r => !r.IsNew && r.Removal != ProjectVariableDraftRemoval.None))
            {
                var users = dependents.TryGetValue(row.Key, out var d) ? d : Array.Empty<string>();
                if (users.Count > 0)
                {
                    errors.Add(new ProjectVariableDraftError(row.Key, ProjectVariableDraftField.Removal,
                        "La usa la fórmula de " + List(users.Select(k => "«" + Display(finalName, k) + "»")) + ": cámbiala antes de eliminar esta variable."));
                }
                else if (row.Removal == ProjectVariableDraftRemoval.Delete)
                {
                    // On the sweep of NOW (the command plans again on a fresh read): a rack linked after the page read it, or a «U» that
                    // undid an unlink, would be left with a broken reference (correction 1 of the review).
                    var discovery = ProjectVariableConsumerDiscovery.DiscoverConsumers(entries, idOf[row.Key]);
                    if (!discovery.IsSuccess)
                    {
                        errors.Add(new ProjectVariableDraftError(row.Key, ProjectVariableDraftField.Removal, discovery.Error));
                    }
                    else if (discovery.Consumers.Count > 0)
                    {
                        var names = discovery.Consumers.Select(c => RackLabel(c.Authored?.Name)).Distinct().ToList();
                        errors.Add(new ProjectVariableDraftError(row.Key, ProjectVariableDraftField.Removal,
                            (names.Count == 1 ? "La usa " : "La usan ") + List(names) + ", así que no se puede eliminar sin más: usa «Desvincular todos y eliminar…», que conserva su valor."));
                    }
                }

                document = RegistryMutation.Remove(idOf[row.Key]).ApplyTo(document);
            }

            // ---------------------------------------------------------------- the final register
            var finalUsable = Accredited(document);
            if (finalUsable == null)
            {
                errors.Add(new ProjectVariableDraftError(null, ProjectVariableDraftField.None, "El registro que quedaría no es válido."));
                return Done();
            }

            var finalEvaluation = RegistryEvaluation.Evaluate(ProjectVariablesExpressionAdapter.From(finalUsable));
            foreach (var row in live)
            {
                if (!idOf.TryGetValue(row.Key, out var id)) continue;
                var result = finalEvaluation.Result(SymbolId.ProjectVariable(id.Value));
                finalValues[row.Key] = ValueOf(result);
                if (errors.Any(e => e.Key == row.Key && e.Field == ProjectVariableDraftField.Definition)) continue;

                var hadValue = !row.IsNew && baseValues.TryGetValue(row.Key, out var before) && before.HasValue;
                if (!row.IsNew && !redefined.Contains(row.Key) && !hadValue) continue; // it had no value and the draft does not touch it

                // VA-S5: not redefined and with the same result as now (also a value that is not greater than zero): it is how the drawing
                // is, not something the draft did (correction 6 of the review).
                if (!row.IsNew && !redefined.Contains(row.Key) && baseEvaluation != null
                    && RegistryResultComparer.Equals(baseEvaluation.Result(SymbolId.ProjectVariable(id.Value)), result)) continue;
                if (result.Succeeded && result.Value > 0.0 && !double.IsInfinity(result.Value)) continue;

                var why = result.Succeeded
                    ? "El resultado tiene que ser mayor que cero (da " + Format(result.Value) + ")."
                    : Explain(result, finalUsable, keyOfId, finalName);
                errors.Add(new ProjectVariableDraftError(row.Key, ProjectVariableDraftField.Definition,
                    redefined.Contains(row.Key) || row.IsNew ? why : "Con este borrador deja de tener valor. " + why));
            }

            // ---------------------------------------------------------------- racks
            var authored = new Dictionary<string, SelectivePalletDesignDocument>(StringComparer.OrdinalIgnoreCase);
            var siblings = new Dictionary<string, IReadOnlyList<ProjectVariableScanEntry>>(StringComparer.OrdinalIgnoreCase);
            var rackNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var rack in repairs)
            {
                var repair = ProjectVariableMutationPreflight.RepairBrokenRack(baseDocument, rack, entries, true);
                if (!repair.IsSuccess || repair.Plan.RackMutations.Count != 1)
                {
                    errors.Add(new ProjectVariableDraftError(rack, ProjectVariableDraftField.Repair,
                        "No se puede reparar este rack: vuelve a leer la página; si sigue igual, revísalo con RACKVARIABLES."));
                    continue;
                }

                var mutation = repair.Plan.RackMutations[0];
                authored[rack] = mutation.AuthoredOutput;
                siblings[rack] = mutation.Destinations;
                rackNames[rack] = mutation.AuthoredOutput?.Name;
            }

            foreach (var row in all.Where(r => !r.IsNew && r.Removal == ProjectVariableDraftRemoval.UnlinkAllAndDelete))
            {
                var id = idOf[row.Key];
                kept[row.Key] = baseValues.TryGetValue(row.Key, out var value) ? value : null;
                var discovery = ProjectVariableConsumerDiscovery.DiscoverConsumers(entries, id);
                if (!discovery.IsSuccess)
                {
                    errors.Add(new ProjectVariableDraftError(row.Key, ProjectVariableDraftField.Removal, discovery.Error));
                    continue;
                }

                var symbol = SymbolId.ProjectVariable(id.Value);
                foreach (var consumer in discovery.Consumers)
                {
                    rackNames[consumer.RackId] = consumer.Authored?.Name;
                    if (consumer.Authored.PropertyValues != null && consumer.Authored.PropertyValues.Values.Any(source =>
                            source != null && string.Equals(source.Kind, SelectivePropertyValueDocument.ExpressionKind, StringComparison.Ordinal)
                            && source.Expression != null && BoundExpressionDependencies.DirectDependencies(source.Expression).Contains(symbol)))
                    {
                        errors.Add(new ProjectVariableDraftError(row.Key, ProjectVariableDraftField.Removal,
                            RackLabel(consumer.Authored?.Name) + " la usa dentro de una fórmula de propiedad: no se puede desvincular desde aquí."));
                        continue;
                    }

                    // The value of NOW, against the register as it is, as the single unlink-all-and-delete. A rack this draft also repairs is
                    // read as repaired: its broken references would leave it without a value to keep (correction 5 of the review).
                    var source = authored.TryGetValue(consumer.RackId, out var repaired) ? repaired : consumer.Authored;
                    var current = Resolver.ResolveAgainst(source, baseUsable);
                    if (!current.IsSuccess)
                    {
                        errors.Add(new ProjectVariableDraftError(consumer.RackId, ProjectVariableDraftField.Rack,
                            RackLabel(consumer.Authored?.Name) + " no tiene un valor que conservar: " + current.Error));
                        continue;
                    }

                    if (!authored.TryGetValue(consumer.RackId, out var design))
                    {
                        design = ProjectVariableCloning.Clone(consumer.Authored);
                        authored[consumer.RackId] = design;
                        siblings[consumer.RackId] = consumer.Siblings;
                    }

                    foreach (var property in SelectiveLinkedPropertyKernel.PropertiesBoundTo(consumer.Authored, id))
                    {
                        if (!SelectiveLinkedProperties.All.TryGetDescriptor(property, out var descriptor)) continue;
                        descriptor.WriteAuthored(design, descriptor.ReadEffective(current.Design));
                        design.PropertyValues?.Remove(property.Value);
                    }
                }
            }

            // The racks that read a variable whose RESULT changes (or that keeps no result).
            if (baseEvaluation != null)
            {
                var changed = new List<VariableId>();
                foreach (var target in baseTargets)
                {
                    if (removed.Contains(target.VariableId.Value)) continue;
                    var symbol = SymbolId.ProjectVariable(target.VariableId.Value);
                    if (!RegistryResultComparer.Equals(baseEvaluation.Result(symbol), finalEvaluation.Result(symbol))) changed.Add(target.VariableId);
                }

                if (changed.Count > 0)
                {
                    var discovery = ProjectVariableConsumerDiscovery.DiscoverConsumers(entries, changed);
                    if (!discovery.IsSuccess)
                    {
                        errors.Add(new ProjectVariableDraftError(null, ProjectVariableDraftField.None, discovery.Error));
                    }
                    else
                    {
                        foreach (var consumer in discovery.Consumers)
                        {
                            rackNames[consumer.RackId] = consumer.Authored?.Name;
                            if (authored.ContainsKey(consumer.RackId)) continue;
                            authored[consumer.RackId] = ProjectVariableCloning.Clone(consumer.Authored);
                            siblings[consumer.RackId] = consumer.Siblings;
                        }
                    }
                }
            }

            var racks = new List<RackMutation>();
            foreach (var rack in authored.Keys.OrderBy(k => k, StringComparer.OrdinalIgnoreCase))
            {
                var effective = Resolver.ResolveAgainst(authored[rack], finalUsable);
                if (!effective.IsSuccess)
                {
                    errors.Add(new ProjectVariableDraftError(rack, ProjectVariableDraftField.Rack,
                        RackLabel(rackNames.TryGetValue(rack, out var n) ? n : null) + " no se podría redibujar con este borrador: " + effective.Error));
                    continue;
                }

                racks.Add(new RackMutation(rack, authored[rack], effective.Design, siblings[rack]));
            }

            if (errors.Count > 0) return Done();

            var registryChanges = !string.Equals(
                ProjectVariableCloning.Canonical(document), ProjectVariableCloning.Canonical(baseDocument), StringComparison.Ordinal);
            var plan = MutationPlan.Of(registryChanges ? RegistryMutation.Replace(document, baseDocument) : RegistryMutation.None, racks);
            return Done(plan, racks.Select(r => r.RackId).ToList());
        }

        // ------------------------------------------------------------------ helpers

        private static UsableProjectVariablesRegistry Accredited(ProjectVariablesDocument document)
        {
            var accreditation = UsableProjectVariablesRegistry.Accredit(ProjectVariablesReadResult.Readable(document));
            return accreditation.IsUsable ? accreditation.Registry : null;
        }

        private static bool TryBind(string text, UsableProjectVariablesRegistry names, out VariableDefinition definition, out string error)
        {
            definition = null;
            error = null;
            var source = (text ?? string.Empty).Trim();
            if (source.Length == 0)
            {
                error = "Escribe un número mayor que cero, o «=» y una fórmula.";
                return false;
            }

            if (!source.StartsWith("=", StringComparison.Ordinal))
            {
                if (double.TryParse(source, NumberStyles.Float, CultureInfo.InvariantCulture, out var literal)
                    && !double.IsNaN(literal) && !double.IsInfinity(literal))
                {
                    if (literal <= 0.0)
                    {
                        error = "Tiene que ser mayor que cero.";
                        return false;
                    }

                    definition = VariableDefinition.Literal(literal);
                    return true;
                }

                error = source.IndexOf(',') >= 0 && double.TryParse(source.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out _)
                    ? "Usa punto decimal: " + source.Replace(',', '.') + "."
                    : "Escribe un número mayor que cero (en pulgadas), o «=» y una fórmula.";
                return false;
            }

            var body = source.Substring(1);
            if (body.Trim().Length == 0)
            {
                error = "Después de «=» va la fórmula.";
                return false;
            }

            var parsed = ExpressionParser.Parse(body);
            if (!parsed.Succeeded)
            {
                error = ProjectVariableDiagnosticText.Describe(parsed.Diagnostics.Select(d => d.Code), body, parsed.Diagnostics.Select(d => d.Span));
                return false;
            }

            var context = names == null
                ? ProjectVariablesExpressionAdapter.From(Array.Empty<ProjectVariable>())
                : ProjectVariablesExpressionAdapter.From(names);
            var bound = ExpressionBinder.Bind(parsed.Syntax, context, SymbolScope.Project);
            if (!bound.Succeeded)
            {
                error = ProjectVariableDiagnosticText.Describe(bound.Diagnostics.Select(d => d.Code), body, bound.Diagnostics.Select(d => d.Span));
                return false;
            }

            definition = VariableDefinition.Expression(bound.Expression);
            return true;
        }

        private static string Explain(
            RegistrySymbolResult result, UsableProjectVariablesRegistry registry, IReadOnlyDictionary<string, string> keyOfId,
            IReadOnlyDictionary<string, string> names)
        {
            string NameOf(SymbolId symbol) => symbol != null && keyOfId.TryGetValue(symbol.Key, out var key) ? Display(names, key) : "?";

            var codes = result.Diagnostics.Select(d => d.Code).Concat(result.RootCauses.Select(r => r.Code)).Distinct().ToList();
            if (codes.Contains(ExpressionDiagnosticCode.Cycle))
            {
                var members = result.RootCauses.Where(r => r.Code == ExpressionDiagnosticCode.Cycle).Select(r => r.Owner)
                    .Concat(result.Diagnostics.Where(d => d.Code == ExpressionDiagnosticCode.Cycle && d.Cause != null).Select(d => d.Cause))
                    .Distinct().Select(NameOf).Where(n => n != "?").Distinct().ToList();
                return members.Count > 1
                    ? "La fórmula forma un ciclo entre " + List(members.Select(m => "«" + m + "»")) + "."
                    : "La fórmula forma un ciclo.";
            }

            var failed = result.RootCauses.Select(r => r.Owner).Concat(result.Diagnostics.Where(d => d.Cause != null).Select(d => d.Cause))
                .Distinct().Select(NameOf).Where(n => n != "?").Distinct().ToList();
            if (codes.Contains(ExpressionDiagnosticCode.DependencyFailed) && failed.Count > 0)
                return "Depende de " + List(failed.Select(f => "«" + f + "»")) + ", que no tiene valor.";
            return ProjectVariableDiagnosticText.Describe(codes, null, null);
        }

        internal static double? ValueOf(RegistrySymbolResult result) => result != null && result.Succeeded ? result.Value : (double?)null;

        private static string Display(IReadOnlyDictionary<string, string> names, string key)
            => names.TryGetValue(key, out var name) && !string.IsNullOrWhiteSpace(name) ? name : "(sin nombre)";

        private static string RackLabel(string name) => string.IsNullOrWhiteSpace(name) ? "Un rack sin nombre" : name.Trim();

        internal static string Format(double value) => Math.Round(value, 3).ToString("0.###", CultureInfo.InvariantCulture);

        /// <summary>«A», «A y B», «A, B y C»: how the page and the batch name several things.</summary>
        public static string List(IEnumerable<string> items)
        {
            var list = items.ToList();
            return list.Count <= 1 ? string.Concat(list) : string.Join(", ", list.Take(list.Count - 1)) + " y " + list[list.Count - 1];
        }
    }

    /// <summary>
    /// The KEY FORM of a definition of the page «Variables» (cuarta tanda, va): the references of a formula are «{§key§}», so a formula
    /// follows its variables by identity while the user renames them (as the register does by <see cref="VariableId"/>). The page writes
    /// and shows names; this file turns names into keys (<see cref="Encode"/>) and keys into names (<see cref="Decode"/>), with the lexer of
    /// the formulas, so only names are touched — never a function, a number or a unit.
    /// </summary>
    public static class ProjectVariableFormulaText
    {
        private const char Mark = '§';

        /// <summary>
        /// Turns the names of <paramref name="text"/> that are names of the draft into keys. Names that are no variable stay as written,
        /// and the batch says so when it binds. A literal is returned trimmed and unchanged.
        /// </summary>
        public static string Encode(string text, IReadOnlyList<(string Key, string Name)> variables)
        {
            var source = (text ?? string.Empty).Trim();
            if (!source.StartsWith("=", StringComparison.Ordinal)) return source;
            var body = source.Substring(1);
            var lexed = ExpressionLexer.Tokenize(body);
            var result = new StringBuilder("=");
            var at = 0;
            foreach (var token in lexed.Tokens)
            {
                if (token.Kind != ExpressionTokenKind.Name && token.Kind != ExpressionTokenKind.BracedName) continue;
                var name = (token.Text ?? string.Empty).Trim();
                var match = (variables ?? Array.Empty<(string, string)>())
                    .Where(v => !string.IsNullOrWhiteSpace(v.Name) && string.Equals(v.Name.Trim(), name, StringComparison.CurrentCultureIgnoreCase))
                    .Select(v => v.Key).ToList();
                if (match.Count != 1) continue;
                result.Append(body, at, token.Span.Start - at);
                result.Append('{').Append(Mark).Append(match[0]).Append(Mark).Append('}');
                at = token.Span.End;
            }

            result.Append(body, at, body.Length - at);
            return result.ToString();
        }

        /// <summary>Shows a key-form definition with the names of now: bare when the name reads as one, between braces otherwise.</summary>
        public static string Decode(string text, Func<string, string> nameOf)
        {
            if (string.IsNullOrEmpty(text)) return text ?? string.Empty;
            return Replace(text, key =>
            {
                var name = nameOf?.Invoke(key);
                if (string.IsNullOrWhiteSpace(name)) return "{?}";
                name = name.Trim();
                return ReadsAsBareName(name) ? name : "{" + name + "}";
            });
        }

        /// <summary>The keys a key-form definition uses, in order and without repeating.</summary>
        public static IReadOnlyList<string> Keys(string text)
        {
            var keys = new List<string>();
            Replace(text ?? string.Empty, key =>
            {
                if (!keys.Contains(key, StringComparer.OrdinalIgnoreCase)) keys.Add(key);
                return string.Empty;
            });
            return keys;
        }

        /// <summary>The key form of a definition of the drawing: its formula with each reference as the key of its variable.</summary>
        public static string FromDefinition(VariableDefinition definition, IReadOnlyList<(string Key, string Name)> variables)
        {
            if (definition == null) return string.Empty;
            if (definition.Kind == VariableDefinitionKind.Literal) return Literal(definition.LiteralValue);
            var context = ProjectVariablesExpressionAdapter.From((variables ?? Array.Empty<(string, string)>())
                .Where(v => VariableId.TryParse(v.Key, out _))
                .Select(v => ProjectVariable.Create(VariableId.Parse(v.Key), string.IsNullOrWhiteSpace(v.Name) ? "?" : v.Name, VariableType.Length,
                    VariableDefinition.Literal(1.0))));
            return Encode("=" + ExpressionFormatter.Format(definition.ExpressionValue, context.Symbols), variables);
        }

        /// <summary>A literal as the page shows it: up to three decimals, more only when the value needs them to read the same.</summary>
        public static string Literal(double value)
        {
            var rounded = Math.Round(value, 3);
            return Math.Abs(rounded - value) < 1e-9
                ? rounded.ToString("0.###", CultureInfo.InvariantCulture)
                : value.ToString("R", CultureInfo.InvariantCulture);
        }

        // The key form with every key replaced by the name of now, between braces, so the binder reads it as one name.
        internal static string Substitute(string text, Func<string, string> nameOf, out string missing)
        {
            string failure = null;
            var result = Replace(text ?? string.Empty, key =>
            {
                var name = nameOf(key);
                if (string.IsNullOrWhiteSpace(name))
                {
                    failure ??= "Usa una variable que ya no está en el borrador.";
                    return "{?}";
                }

                return "{" + name.Trim() + "}";
            });
            missing = failure;
            return result;
        }

        private static string Replace(string text, Func<string, string> replace)
        {
            var result = new StringBuilder();
            var at = 0;
            while (at < text.Length)
            {
                var open = text.IndexOf("{" + Mark, at, StringComparison.Ordinal);
                var close = open < 0 ? -1 : text.IndexOf(Mark + "}", open + 2, StringComparison.Ordinal);
                if (open < 0 || close < 0)
                {
                    result.Append(text, at, text.Length - at);
                    break;
                }

                result.Append(text, at, open - at);
                result.Append(replace(text.Substring(open + 2, close - open - 2)));
                at = close + 2;
            }

            return result.ToString();
        }

        private static bool ReadsAsBareName(string name)
        {
            var lexed = ExpressionLexer.Tokenize(name);
            var tokens = lexed.Tokens.Where(t => t.Kind != ExpressionTokenKind.EndOfText).ToList();
            return lexed.Diagnostics.Count == 0 && tokens.Count == 1 && tokens[0].Kind == ExpressionTokenKind.Name
                   && string.Equals(tokens[0].Text, name, StringComparison.Ordinal);
        }
    }

    /// <summary>The diagnostics of a formula in Spanish, for the page «Variables» (dolor 6 of the inventory: no codes in English).</summary>
    public static class ProjectVariableDiagnosticText
    {
        /// <summary>A unit as a formula writes it, between brackets, from the one authority of the unit tokens.</summary>
        public static string Unit(LengthUnit unit) => "[" + LengthUnits.Authority.Token(unit) + "]";

        public static string Describe(IEnumerable<ExpressionDiagnosticCode> codes, string source, IEnumerable<SourceSpan?> spans)
        {
            var list = (codes ?? Enumerable.Empty<ExpressionDiagnosticCode>()).ToList();
            var first = list.Count == 0 ? (ExpressionDiagnosticCode?)null : list[0];
            var at = spans?.FirstOrDefault(s => s.HasValue);
            var piece = source != null && at.HasValue && at.Value.Start >= 0 && at.Value.End <= source.Length
                ? source.Substring(at.Value.Start, at.Value.Length).Trim()
                : null;
            switch (first)
            {
                case ExpressionDiagnosticCode.EmptyExpression: return "Después de «=» va la fórmula.";
                case ExpressionDiagnosticCode.UnexpectedCharacter:
                    return string.IsNullOrEmpty(piece) ? "La fórmula lleva un carácter que no admite." : "Carácter no admitido en una fórmula: «" + piece + "».";
                case ExpressionDiagnosticCode.UnexpectedToken: return "Sobra algo, o falta un operador entre dos términos.";
                case ExpressionDiagnosticCode.UnbalancedParenthesis: return "Los paréntesis no cierran: sobra o falta uno.";
                case ExpressionDiagnosticCode.UnterminatedName: return "Falta cerrar una llave «}».";
                case ExpressionDiagnosticCode.InvalidNumber: return "Hay un número mal escrito.";
                case ExpressionDiagnosticCode.AmbiguousDecimalComma:
                    return "Usa punto decimal (6.5). La coma solo separa argumentos, con un espacio detrás: MAX(A, 5).";
                case ExpressionDiagnosticCode.UnknownUnit: return "Unidad no admitida: usa " + Unit(LengthUnit.Millimeter) + ", " + Unit(LengthUnit.Inch) + " o " + Unit(LengthUnit.Foot) + ".";
                case ExpressionDiagnosticCode.UnitNotAllowedHere:
                case ExpressionDiagnosticCode.UnitSyntaxNotSupported: return "Escribe la unidad entre corchetes, pegada al número: 100" + Unit(LengthUnit.Millimeter) + ".";
                case ExpressionDiagnosticCode.InvalidQualifier:
                case ExpressionDiagnosticCode.QualifiedNameMismatch: return "La referencia a una variable no es válida.";
                case ExpressionDiagnosticCode.LimitExceeded: return "La fórmula es demasiado larga o tiene demasiados niveles.";
                case ExpressionDiagnosticCode.UnknownSymbol:
                    return string.IsNullOrEmpty(piece) ? "Usa un nombre que no es de ninguna variable." : "No hay ninguna variable llamada «" + piece.Trim('{', '}', ' ') + "».";
                case ExpressionDiagnosticCode.AmbiguousName: return "Hay más de una variable con ese nombre.";
                case ExpressionDiagnosticCode.UnknownNamespace:
                case ExpressionDiagnosticCode.ScopeViolation: return "Una variable de proyecto solo puede usar otras variables de proyecto.";
                case ExpressionDiagnosticCode.UnknownFunction:
                    return string.IsNullOrEmpty(piece) ? "Función desconocida: solo hay MIN, MAX y ABS." : "Función desconocida «" + piece + "»: solo hay MIN, MAX y ABS.";
                case ExpressionDiagnosticCode.ReservedName: return "Ese nombre está reservado.";
                case ExpressionDiagnosticCode.NameRequired: return "Falta un nombre.";
                case ExpressionDiagnosticCode.OperatorInName: return "Un nombre no puede llevar operadores; escríbelo entre llaves.";
                case ExpressionDiagnosticCode.BrokenReference: return "Usa una variable que ya no está en el dibujo.";
                case ExpressionDiagnosticCode.Cycle: return "La fórmula forma un ciclo.";
                case ExpressionDiagnosticCode.DependencyFailed: return "Depende de una variable que no tiene valor.";
                case ExpressionDiagnosticCode.InvalidArguments: return "MIN y MAX llevan dos argumentos o más, y ABS uno, separados por «, ».";
                case ExpressionDiagnosticCode.DivisionByZero: return "División entre cero.";
                case ExpressionDiagnosticCode.NonFiniteResult: return "El resultado no es un número finito.";
                case ExpressionDiagnosticCode.OutOfRange: return "El resultado queda fuera del rango admitido.";
                default: return "La fórmula no es válida.";
            }
        }
    }
}
