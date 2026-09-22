using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.CustomProperties;
using RackCad.Application.Persistence;
using RackCad.Application.Views.Insertion;
using RackCad.Application.Views.Redraw;
using RackCad.Plugin.Systems.Shared;

namespace RackCad.Plugin.Views
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
            var facts = new List<RackSiblingScanFact>();
            var definitions = new Dictionary<string, ObjectId>(StringComparer.Ordinal);
            var envelopes = new Dictionary<string, RackEmbedDocument>(StringComparer.Ordinal);
            var properties = new Dictionary<string, CustomPropertiesReadResult>(StringComparer.Ordinal);
            var layers = new Dictionary<string, IReadOnlyList<RackSiblingLayerRequirement>>(StringComparer.Ordinal);
            var embedStore = new RackEmbedStore();
            var propertyStore = new CustomPropertiesStore();

            using (document.LockDocument())
            using (var transaction = document.Database.TransactionManager.StartTransaction())
            {
                var table = (BlockTable)transaction.GetObject(document.Database.BlockTableId, OpenMode.ForRead);
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
                    layers.Add(key, layerFacts);
                    if (envelope != null)
                    {
                        envelopes.Add(key, envelope);
                        properties.Add(key, propertyStore.ReadElement(envelope.CustomProperties));
                    }
                }

                transaction.Commit();
            }

            var membership = RackSiblingMembership.Classify(
                facts, rackId, originalSourceId, originalSourceIdIsAttributable);
            return new RackSiblingScanSnapshot(membership, definitions, envelopes, properties, layers);
        }

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
