#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RackCad.Application.Systems.PushBack.Pilot;
using Xunit;
using Src = RackCad.Tests.WorkspaceHostSource;

namespace RackCad.Tests;

// I-64 F4-T4B-MUTATE-POST (D-T4B-04 and D-T4B-09; INV-F4T4B-04, 07, 12, 14, 15, 16 and 17): the source guards of the update of the pilot. The Core
// test project cannot reference the Plugin (it needs AutoCAD), so the writer and the traversal are read as text. Every guard is followed by the
// proof that it trips on the real text with ONE injected change: a guard that never fails watches nothing. They read the code without its
// comments unless a test says that it reads the raw text.
public class FullPanelCopyWorkspacePilotUpdateGuardRedTests
{
    private const string CorePath = "src/RackCad.Application/Workspace/WorkspaceHostCore.cs";

    private static void AssertInOrder(string text, params string[] needles)
    {
        var previous = -1;
        foreach (var needle in needles)
        {
            var at = text.IndexOf(needle, StringComparison.Ordinal);
            Assert.True(at >= 0, "missing: " + needle);
            Assert.True(at > previous, "out of order: " + needle);
            previous = at;
        }
    }

    [Fact]
    public void TheBodyOfTheUpdateFollowsTheOrderOfThePackageAndEndsTheMarkOfKnownCleanAfterTheClose()
    {
        var body = Src.BodyOf(Src.Code(CorePath), @"public\s+EditUpdateOutcome\s+TryUpdatePilot\s*\(");

        AssertInOrder(
            body,
            "UpdateGate(",
            "string.IsNullOrWhiteSpace(request.RackId)",
            "_drafts.Find<PushBackFullDraftState>(",
            "PushBackPilotUpdateRun.Precheck(",
            ".TryBeginApply(",
            "PushBackPilotUpdateRun.ExecuteFull(",
            ".CompleteApply(",
            "_knownClean.Remove(request.Session);");
        Assert.Equal(1, Src.Count(body, @"PushBackPilotUpdateRun\.ExecuteFull\s*\("));
        Assert.Equal(1, Src.Count(body, @"PushBackPilotUpdateRun\.Precheck\s*\("));
        Assert.DoesNotMatch(new Regex(@"\.SetCurrentDraftState\s*\(|\.DiscardAndReload\s*\(|\.KeepMineOnCurrentVersion\s*\(|\.ConfirmBase\s*\(|\.CheckNow\s*\(|\.Begin\s*\("), body);
    }
}

/// <summary>
/// The text of the traversal of the base (c3ae1c97, RackSiblingScan.cs) that D-T4B-04 freezes, raw: the nucleus (lines 103 to 110, 115 to 161 and 166 to
/// 169, with the ONE substitution of document.Database.BlockTableId by database.BlockTableId) and the rest of the file (every line but 98 to 170).
/// The guards compare it normalized, so layout and comments do not count.
/// </summary>
internal static class TraverseBaselineFull
{
    public const string CoreLiteral = """
            var facts = new List<RackSiblingScanFact>();
            var definitions = new Dictionary<string, ObjectId>(StringComparer.Ordinal);
            var envelopes = new Dictionary<string, RackEmbedDocument>(StringComparer.Ordinal);
            var properties = new Dictionary<string, CustomPropertiesReadResult>(StringComparer.Ordinal);
            var layers = new Dictionary<string, IReadOnlyList<RackSiblingLayerRequirement>>(StringComparer.Ordinal);
            var rawEnvelopes = new Dictionary<string, string>(StringComparer.Ordinal);
            var embedStore = new RackEmbedStore();
            var propertyStore = new CustomPropertiesStore();

                var table = (BlockTable)transaction.GetObject(database.BlockTableId, OpenMode.ForRead);
                foreach (ObjectId definitionId in table)
                {
                    var definition = (BlockTableRecord)transaction.GetObject(definitionId, OpenMode.ForRead);
                    if (definition.IsLayout || definition.IsAnonymous) continue;

                    var json = RackBlockData.Read(transaction, definitionId);
                    if (string.IsNullOrEmpty(json)) continue;

                    var envelope = embedStore.Deserialize(json);
                    var key = definitionId.Handle.ToString();
                    var layoutReferences = 0;
                    var nestedReferences = 0;
                    var layerFacts = new List<RackSiblingLayerRequirement>();
                    foreach (ObjectId referenceId in definition.GetBlockReferenceIds(directOnly: true, forceValidity: true))
                    {
                        var reference = transaction.GetObject(referenceId, OpenMode.ForRead) as BlockReference;
                        var owner = reference == null ? null : transaction.GetObject(reference.OwnerId, OpenMode.ForRead) as BlockTableRecord;
                        if (owner?.IsLayout == true) layoutReferences++; else nestedReferences++;
                        AddLayer(transaction, reference?.LayerId ?? ObjectId.Null, RackSiblingLayerUse.DirectReference, layerFacts);
                    }

                    foreach (ObjectId entityId in definition)
                        AddLayer(transaction, (transaction.GetObject(entityId, OpenMode.ForRead) as Entity)?.LayerId ?? ObjectId.Null,
                            RackSiblingLayerUse.ExistingDefinitionEntity, layerFacts);

                    var probe = envelope?.Id ?? RackEnvelopeIdProbe.Probe(json);
                    facts.Add(new RackSiblingScanFact(
                        key,
                        envelope?.View,
                        definitionId == selectedDefinition,
                        definition.IsFromExternalReference,
                        envelope != null,
                        envelope?.Id,
                        probe,
                        layoutReferences,
                        nestedReferences,
                        envelope != null && eraseByKind != null && eraseByKind(envelope)));
                    definitions.Add(key, definitionId);
                    rawEnvelopes.Add(key, json);
                    layers.Add(key, layerFacts);
                    if (envelope != null)
                    {
                        envelopes.Add(key, envelope);
                        properties.Add(key, propertyStore.ReadElement(envelope.CustomProperties));
                    }
                }

            return new RackSiblingDrawingScan(facts, definitions, envelopes, properties, layers)
            {
                RawEnvelopes = new ReadOnlyDictionary<string, string>(rawEnvelopes)
            };
""";

    // The raw text has the keyword that opens a namespace written as a placeholder, so no line of this file starts with it (a guard of the
    // namespaces of the test projects counts those lines).
    private const string RestRaw = """
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.CustomProperties;
using RackCad.Application.Persistence;
using RackCad.Application.Views.Insertion;
using RackCad.Application.Views.Redraw;
using RackCad.Plugin.Systems.Shared;

@NS@ RackCad.Plugin.Views
{
    internal sealed class RackSiblingScanSnapshot
    {
        internal RackSiblingScanSnapshot(
            RackSiblingMembershipSnapshot membership,
            IReadOnlyDictionary<string, ObjectId> definitions,
            IReadOnlyDictionary<string, RackEmbedDocument> envelopes,
            IReadOnlyDictionary<string, CustomPropertiesReadResult> properties,
            IReadOnlyDictionary<string, IReadOnlyList<RackSiblingLayerRequirement>> layers)
        {
            Membership = membership;
            Definitions = definitions;
            Envelopes = envelopes;
            Properties = properties;
            Layers = layers;
        }

        internal RackSiblingMembershipSnapshot Membership { get; }
        internal IReadOnlyDictionary<string, ObjectId> Definitions { get; }
        internal IReadOnlyDictionary<string, RackEmbedDocument> Envelopes { get; }
        internal IReadOnlyDictionary<string, CustomPropertiesReadResult> Properties { get; }
        internal IReadOnlyDictionary<string, IReadOnlyList<RackSiblingLayerRequirement>> Layers { get; }
    }

    /// <summary>
    /// One whole-drawing scan, unclassified: every definition of the BlockTable read once. ID19 (RACKPROYECTAR) classifies it
    /// per RackId without touching the drawing again.
    /// </summary>
    internal sealed class RackSiblingDrawingScan
    {
        internal RackSiblingDrawingScan(
            IReadOnlyList<RackSiblingScanFact> facts,
            IReadOnlyDictionary<string, ObjectId> definitions,
            IReadOnlyDictionary<string, RackEmbedDocument> envelopes,
            IReadOnlyDictionary<string, CustomPropertiesReadResult> properties,
            IReadOnlyDictionary<string, IReadOnlyList<RackSiblingLayerRequirement>> layers)
        {
            Facts = facts;
            Definitions = definitions;
            Envelopes = envelopes;
            Properties = properties;
            Layers = layers;
        }

        internal IReadOnlyList<RackSiblingScanFact> Facts { get; }
        internal IReadOnlyDictionary<string, ObjectId> Definitions { get; }
        internal IReadOnlyDictionary<string, RackEmbedDocument> Envelopes { get; }
        internal IReadOnlyDictionary<string, CustomPropertiesReadResult> Properties { get; }
        internal IReadOnlyDictionary<string, IReadOnlyList<RackSiblingLayerRequirement>> Layers { get; }

        /// <summary>
        /// The raw text of the envelope of each scanned definition that has text, by key (ordinal): the same text that
        /// <c>Traverse</c> deserializes. It is a read-only view; the scan still writes nothing. It is assigned at the one point
        /// where a scan is built (<c>Traverse</c>); a scan without it carries null, and the read of the intention then fails
        /// with a typed failure instead of receiving an empty dictionary in silence.
        /// </summary>
        internal IReadOnlyDictionary<string, string> RawEnvelopes { get; init; }

        /// <summary>The members of one rack among the scanned definitions; no source is selected, so none is erased by kind.</summary>
        internal RackSiblingMembershipSnapshot MembershipFor(string rackId)
            => RackSiblingMembership.Classify(Facts, rackId, null, false);
    }

    /// <summary>The single physical BlockTable traversal used by one Insert gesture.</summary>
    internal static class RackSiblingScan
    {
        internal static RackSiblingScanSnapshot Capture(
            Document document,
            ObjectId selectedDefinition,
            string rackId,
            string originalSourceId,
            bool originalSourceIdIsAttributable,
            Func<RackEmbedDocument, bool> eraseByKind)
        {
            var scan = Traverse(document, selectedDefinition, eraseByKind);
            var membership = RackSiblingMembership.Classify(
                scan.Facts, rackId, originalSourceId, originalSourceIdIsAttributable);
            return new RackSiblingScanSnapshot(
                membership, scan.Definitions, scan.Envelopes, scan.Properties, scan.Layers);
        }

        /// <summary>The same traversal with nothing selected, for a caller that classifies several racks from ONE pass.</summary>
        internal static RackSiblingDrawingScan CaptureDrawing(Document document)
            => Traverse(document, ObjectId.Null, null);


        private static void AddLayer(Transaction transaction, ObjectId layerId, RackSiblingLayerUse use,
            List<RackSiblingLayerRequirement> facts)
        {
            if (layerId.IsNull) return;
            var layer = transaction.GetObject(layerId, OpenMode.ForRead) as LayerTableRecord;
            if (layer == null || facts.Exists(item => item.Use == use && string.Equals(item.Layer, layer.Name, StringComparison.OrdinalIgnoreCase))) return;
            facts.Add(new RackSiblingLayerRequirement(layer.Name, layer.IsLocked, use));
        }
    }
}

""";

    public static readonly string RestLiteral = RestRaw.Replace("@NS@ ", "namespace ", StringComparison.Ordinal);
}
