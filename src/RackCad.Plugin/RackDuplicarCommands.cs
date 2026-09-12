using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using RackCad.Application.Persistence;
using RackCad.Plugin.KindHandlers;
using RackCad.Plugin.Systems.Shared;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin
{
    /// <summary>RACKDUPLICAR: COPY-style duplication of racks — base point + repeated destination points, each copy
    /// an INDEPENDENT rack (fresh GUID + numbered "- copia" name). Plus its short alias.</summary>
    public sealed class RackDuplicarCommands
    {
        [CommandMethod("RD")]  public void AliasRackDuplicar() => RackDuplicar();          // RACKDUPLICAR

        /// <summary>
        /// Duplicate racks like AutoCAD's COPY: select one or more racks — several racks, several views of one rack,
        /// linked references, anything else is ignored with a notice — pick a BASE point, then click destination points.
        /// Each click places an independent copy of every selected rack (its own GUID and name, so RACKEDITAR touches
        /// only that copy) keeping the selection's layout. Multiple mode by default (keep clicking; Enter/Esc ends); the
        /// [Unica] keyword switches to a single destination. The copy CLONES each selected view-block's drawn geometry
        /// (nested ARRAY defs shared, payload re-stamped), so it is exact and works the same for every rack type.
        ///
        /// <para>
        /// I-51 (ID15). Only what was selected is copied: a view nobody selected is never added (PD-1). The command runs
        /// in phases — ACQUIRE the selection, SNAPSHOT it in one read transaction, PREFLIGHT it with the pure
        /// <see cref="RackDuplicationPlan"/> and a rehearsal of every restamp, ask the BASE point, and then per
        /// destination: assign identities, PREPARE every restamp, MUTATE in one transaction. Everything that can fail
        /// for a semantic reason is decided before that transaction opens, and an exception inside it rolls the whole
        /// destination back: a destination is placed entirely or not at all, and the ones already placed remain.
        /// </para>
        /// </summary>
        [CommandMethod("RACKDUPLICAR")]
        public void RackDuplicar()
        {
            try
            {
                var document = AcApplication.DocumentManager.MdiActiveDocument;
                if (document == null)
                {
                    return;
                }

                var editor = document.Editor;

                // ACQUIRE: a plain multiple selection with no type filter; what is not a rack is reported by the plan.
                var selection = editor.GetSelection(new PromptSelectionOptions
                {
                    MessageForAdding = "\nSelecciona racks para duplicar: "
                });
                if (selection.Status != PromptStatus.OK)
                {
                    return;
                }

                var snapshot = TakeSnapshot(document, selection.Value.GetObjectIds());

                // PREFLIGHT: the plan is the only authority on which definitions and references form each copy.
                var plan = RackDuplicationPlan.Build(
                    snapshot.Selection,
                    snapshot.DefinitionSnapshots,
                    kind => KindHandlerRegistry.Default.TryGetIgnoreCase(kind, out _));

                foreach (var notice in plan.Notices)
                {
                    editor.WriteMessage("\nRackCad: " + notice);
                }

                if (!plan.IsSuccess)
                {
                    foreach (var error in plan.Errors)
                    {
                        editor.WriteMessage("\nRackCad: " + error);
                    }

                    return;
                }

                // INV-12: rehearse every restamp before asking for a point, so a rack this build cannot copy fails with
                // nothing clicked and nothing written. One throwaway id and name per group, like a real destination;
                // they never reach the drawing nor the destination assigner.
                Prepare(plan, snapshot, group => (Guid.NewGuid(), group.BaseName + " - copia"));

                var basePrompt = new PromptPointOptions("\nPunto base: ");
                var baseResult = editor.GetPoint(basePrompt);
                if (baseResult.Status != PromptStatus.OK)
                {
                    return;
                }

                var basePoint = baseResult.Value;
                var assigner = plan.CreateDestinationAssigner(Guid.NewGuid);
                var multiple = true; // like COPY: keep placing until Enter/Esc
                var placed = 0;

                while (true)
                {
                    var options = new PromptPointOptions("\nPunto de destino o")
                    {
                        UseBasePoint = true,
                        BasePoint = basePoint,
                        AllowNone = true, // Enter ends the command
                        AppendKeywordsToMessage = true
                    };
                    options.Keywords.Add("Unica");
                    options.Keywords.Add("Multiple");

                    var destination = editor.GetPoint(options);

                    if (destination.Status == PromptStatus.Keyword)
                    {
                        multiple = string.Equals(destination.StringResult, "Multiple", StringComparison.OrdinalIgnoreCase);
                        editor.WriteMessage(multiple
                            ? "\nRackCad: modo copia múltiple (Enter para terminar)."
                            : "\nRackCad: modo copia única (el siguiente punto coloca una copia y termina).");
                        continue;
                    }

                    if (destination.Status != PromptStatus.OK)
                    {
                        break; // Enter or Esc
                    }

                    var assignment = assigner.Next();
                    if (!assignment.IsSuccess)
                    {
                        throw new InvalidOperationException(assignment.Error);
                    }

                    // PREPARE (I-47 G14, I-51 G5): todas las definiciones de todos los grupos se re-estampan con la
                    // identidad y el nombre de SU grupo, y se comprueban, antes de abrir la transaccion de escritura. Si
                    // una falla, el comando termina aqui: este destino queda intacto y los ya colocados permanecen.
                    var identities = assignment.Groups.ToDictionary(group => group.Key);
                    var prepared = Prepare(plan, snapshot, group => (identities[group.Key].NewRackId, identities[group.Key].CopyName));

                    // GetPoint returns CURRENT-UCS coordinates but BlockReference.Position is WCS: transform the
                    // displacement (a vector — only the rotational part applies) or a rotated UCS lands copies wrong.
                    var displacement = (destination.Value - basePoint).TransformBy(editor.CurrentUserCoordinateSystem);

                    PlaceDestination(document, plan, snapshot, prepared, displacement);
                    placed = assignment.Ordinal;
                    editor.WriteMessage(DescribePlaced(assignment));

                    if (!multiple)
                    {
                        break;
                    }
                }

                if (placed > 0)
                {
                    editor.WriteMessage(DescribeSummary(plan, placed));
                }
            }
            catch (System.Exception ex)
            {
                RackCommandSupport.Report(ex);
            }
        }

        /// <summary>
        /// SNAPSHOT: everything the command needs from the selection, read in ONE transaction and kept in two worlds
        /// keyed by the same handles — the plain data the planner decides on, and the AutoCAD side only this command
        /// uses. A definition referenced several times is read once. No DBObject leaves the transaction.
        /// </summary>
        private static DuplicationSnapshot TakeSnapshot(Document document, ObjectId[] selectedIds)
        {
            return InDocumentTransaction.Run(document, transaction =>
            {
                var snapshot = new DuplicationSnapshot();
                var modelSpaceId = SymbolUtilityServices.GetBlockModelSpaceId(document.Database);

                foreach (var id in selectedIds)
                {
                    var key = id.Handle.ToString();

                    if (!(transaction.GetObject(id, OpenMode.ForRead) is BlockReference reference))
                    {
                        snapshot.Selection.Add(new RackDuplicationSelectedReference(key, false, false, null));
                        continue;
                    }

                    if (reference.OwnerId != modelSpaceId)
                    {
                        // PD-7: the plan filters it before looking at its definition, so the definition is not read.
                        snapshot.Selection.Add(new RackDuplicationSelectedReference(key, true, false, null));
                        continue;
                    }

                    var definitionId = reference.BlockTableRecord;
                    var definitionKey = definitionId.Handle.ToString();

                    snapshot.Selection.Add(new RackDuplicationSelectedReference(key, true, true, definitionKey));
                    snapshot.ReferencesByKey[key] = new SourceReference
                    {
                        Position = reference.Position,
                        Rotation = reference.Rotation,
                        Scale = reference.ScaleFactors,
                        LayerId = reference.LayerId // COPY preserves the entity's layer; so do we
                    };

                    if (snapshot.DefinitionsByKey.ContainsKey(definitionKey))
                    {
                        continue; // linked references share one definition
                    }

                    var definition = (BlockTableRecord)transaction.GetObject(definitionId, OpenMode.ForRead);
                    var payload = RackBlockData.Read(transaction, definitionId);

                    snapshot.DefinitionSnapshots.Add(new RackDuplicationDefinitionSnapshot(definitionKey, payload, definition.Name));
                    snapshot.DefinitionsByKey.Add(definitionKey, new SourceDefinition
                    {
                        DefinitionId = definitionId,
                        // The name THIS definition's envelope carries, as stored: RackCloner renames the drawn label
                        // equal to it, exactly as the historic single-view copy did with the clicked view's name.
                        LabelName = string.IsNullOrEmpty(payload) ? null : new RackEmbedStore().Deserialize(payload)?.Name
                    });
                }

                return snapshot;
            });
        }

        /// <summary>
        /// PREPARE: re-stamp EVERY definition of EVERY group with its group's identity and name and check each result,
        /// before anything is written (I-47 G14). One identity per group is what makes every view of one rack be born
        /// with the SAME id and name; each view still re-stamps its OWN payload, never another view's. Throws on the
        /// first failure, so a caller only ever holds a complete set.
        /// </summary>
        private static List<PreparedDefinition> Prepare(
            RackDuplicationPlan plan, DuplicationSnapshot snapshot, Func<RackDuplicationGroup, (Guid NewRackId, string CopyName)> identityOf)
        {
            var prepared = new List<PreparedDefinition>();
            var severalDefinitions = plan.Groups.Count > 1 || plan.Groups[0].Definitions.Count > 1;

            foreach (var group in plan.Groups)
            {
                var identity = identityOf(group);

                foreach (var definition in group.Definitions)
                {
                    var restamped = RackEnvelopeRestamp.RestampEnvelope(definition.RawPayload, identity.CopyName, identity.NewRackId);

                    if (!restamped.IsSuccess)
                    {
                        // One definition keeps the historic message; with several, say which one cannot be copied.
                        throw new InvalidOperationException(severalDefinitions
                            ? "La definicion de bloque '" + definition.DefinitionName + "' no se puede copiar: " + restamped.Error
                            : restamped.Error);
                    }

                    var source = snapshot.DefinitionsByKey[definition.DefinitionKey];

                    prepared.Add(new PreparedDefinition
                    {
                        DefinitionKey = definition.DefinitionKey,
                        SourceDefinitionId = source.DefinitionId,
                        SourceName = source.LabelName,
                        CopyName = identity.CopyName,
                        DesignJson = restamped.DesignJson
                    });
                }
            }

            return prepared;
        }

        /// <summary>
        /// MUTATE: one destination in ONE transaction. Each prepared definition is cloned exactly once, and every selected
        /// reference is re-created on the clone of ITS OWN definition — two references to one source definition become two
        /// references to one clone, so linked copies stay linked — at its own position plus the displacement, with its own
        /// rotation, scale and layer. Nothing here can fail for a semantic reason; if AutoCAD throws, nothing commits and
        /// the destination leaves no partial copy.
        /// </summary>
        private static void PlaceDestination(
            Document document, RackDuplicationPlan plan, DuplicationSnapshot snapshot, IReadOnlyList<PreparedDefinition> prepared, Vector3d displacement)
        {
            var database = document.Database;

            InDocumentTransaction.Run(document, transaction =>
            {
                var modelSpace = (BlockTableRecord)transaction.GetObject(
                    SymbolUtilityServices.GetBlockModelSpaceId(database), OpenMode.ForWrite);
                var clones = new Dictionary<string, ObjectId>(StringComparer.Ordinal); // source DefinitionKey -> clone

                foreach (var definition in prepared)
                {
                    clones.Add(definition.DefinitionKey, RackCloner.CloneDefinition(
                        database, transaction, definition.SourceDefinitionId, definition.CopyName, definition.DesignJson,
                        definition.SourceName, definition.CopyName));
                }

                foreach (var reference in plan.Groups.SelectMany(group => group.References))
                {
                    var source = snapshot.ReferencesByKey[reference.ReferenceKey];
                    var copy = new BlockReference(source.Position + displacement, clones[reference.DefinitionKey])
                    {
                        Rotation = source.Rotation,
                        ScaleFactors = source.Scale,
                        LayerId = source.LayerId
                    };
                    modelSpace.AppendEntity(copy);
                    transaction.AddNewlyCreatedDBObject(copy, true);
                }
            });
        }

        /// <summary>The line for one placed destination: the historic one for one rack, the list of copies for several.</summary>
        private static string DescribePlaced(RackDuplicationDestinationAssignment assignment)
            => assignment.Groups.Count == 1
                ? "\nRackCad: copia '" + assignment.Groups[0].CopyName + "' colocada."
                : "\nRackCad: copias " + string.Join(", ", assignment.Groups.Select(group => "'" + group.CopyName + "'")) + " colocadas.";

        /// <summary>The closing line: the historic one for one rack; for several, how many racks every destination copied.</summary>
        private static string DescribeSummary(RackDuplicationPlan plan, int placed)
            => plan.Groups.Count == 1
                ? string.Format(CultureInfo.InvariantCulture,
                    "\nRackCad: {0} copia(s) independiente(s) de '{1}'.", placed, plan.Groups[0].BaseName)
                : string.Format(CultureInfo.InvariantCulture,
                    "\nRackCad: {0} copia(s) independiente(s) de {1} racks.", placed, plan.Groups.Count);

        /// <summary>The SNAPSHOT of a selection, in two worlds keyed by the same handles.</summary>
        private sealed class DuplicationSnapshot
        {
            /// <summary>Planner input: every selected entity, in selection order.</summary>
            public readonly List<RackDuplicationSelectedReference> Selection = new List<RackDuplicationSelectedReference>();

            /// <summary>Planner input: one snapshot per distinct definition referenced from Model Space.</summary>
            public readonly List<RackDuplicationDefinitionSnapshot> DefinitionSnapshots = new List<RackDuplicationDefinitionSnapshot>();

            /// <summary>AutoCAD side: the placement of every Model Space block reference, by its handle.</summary>
            public readonly Dictionary<string, SourceReference> ReferencesByKey = new Dictionary<string, SourceReference>(StringComparer.Ordinal);

            /// <summary>AutoCAD side: every distinct definition behind them, by its handle.</summary>
            public readonly Dictionary<string, SourceDefinition> DefinitionsByKey = new Dictionary<string, SourceDefinition>(StringComparer.Ordinal);
        }

        /// <summary>What a copy of one selected reference keeps from it: COPY's own placement, nothing live.</summary>
        private struct SourceReference
        {
            public Point3d Position;
            public double Rotation;
            public Scale3d Scale;
            public ObjectId LayerId;
        }

        /// <summary>The AutoCAD side of one distinct source definition.</summary>
        private struct SourceDefinition
        {
            public ObjectId DefinitionId;
            public string LabelName;
        }

        /// <summary>PREPARE's result for one definition: everything MUTATE needs to clone it, already re-stamped and checked.</summary>
        private struct PreparedDefinition
        {
            public string DefinitionKey;
            public ObjectId SourceDefinitionId;
            public string SourceName;
            public string CopyName;
            public string DesignJson;
        }
    }
}
