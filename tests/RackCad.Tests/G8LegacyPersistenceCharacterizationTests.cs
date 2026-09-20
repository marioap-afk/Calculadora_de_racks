using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Selective;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-49 G8 step 1: pins the I-47/I-48 persistence and effective-value contract before Expression enters either
    /// persisted union. Every test in this class is expected to stay green on the untouched G7 production tree.
    /// </summary>
    public sealed class G8LegacyPersistenceCharacterizationTests
    {
        private const string RackId = "3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f44";
        private const string VariableIdText = "8A1D4E77-2C93-4B60-8F15-6E0B93A7C221";
        private const string PropertyToken = ProjectPropertyIds.SelectiveVerticalClearanceToken;
        private const string PostId = "POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA";
        private const string BeamId = "LARGUERO_ESCALON_CAL14_3_REMACHES";

        [Fact]
        [Trait("Gate", "G8-Legacy")]
        public void LEGACY_VARIABLE_DEFINITION_LITERAL_ROUND_TRIPS_WITH_EXACT_ID_AND_VALUE()
        {
            var store = new ProjectVariablesStore();
            var json = store.Serialize(Registry(7.5));
            var loaded = store.Deserialize(json);

            Assert.Equal(
                "{\"SchemaVersion\":\"1.0\",\"Variables\":[{\"VariableId\":\"" + VariableIdText +
                "\",\"Name\":\"Holgura vertical\",\"Type\":\"Length\",\"Definition\":{\"Kind\":\"literal\",\"Value\":7.5}}]}",
                json);
            Assert.Equal(ProjectVariablesReadOutcome.Readable, loaded.Outcome);
            var variable = Assert.Single(loaded.Document.ToProjectVariables());
            Assert.Equal(VariableIdText, variable.Id.Value);
            Assert.Equal("Holgura vertical", variable.Name);
            Assert.Equal(VariableDefinitionKind.Literal, variable.Definition.Kind);
            Assert.Equal(7.5, variable.Definition.LiteralValue);
        }

        [Fact]
        [Trait("Gate", "G8-Legacy")]
        public void LEGACY_LITERAL_PROPERTY_IS_ABSENCE_OF_A_BINDING_AND_ROUND_TRIPS()
        {
            var store = new SelectivePalletDesignStore();
            var loaded = store.Deserialize(store.Serialize(Authored(6.0)));

            Assert.Equal(6.0, loaded.VerticalClearance);
            Assert.False(loaded.HasPropertyValues);
            Assert.Null(loaded.PropertyValues);
        }

        [Fact]
        [Trait("Gate", "G8-Legacy")]
        public void LEGACY_PROJECT_VARIABLE_REFERENCE_ROUND_TRIPS_WITH_EXACT_TEXT()
        {
            var store = new SelectivePalletDesignStore();
            var loaded = store.Deserialize(store.Serialize(Authored(6.0, Reference())));

            var binding = Assert.Single(loaded.PropertyValues);
            Assert.Equal(PropertyToken, binding.Key);
            Assert.Equal(SelectivePropertyValueDocument.ProjectVariableKind, binding.Value.Kind);
            Assert.Equal(VariableIdText, binding.Value.VariableId);
        }

        [Fact]
        [Trait("Gate", "G8-Legacy")]
        public void OLD_DOCUMENT_WITHOUT_EXPRESSION_DISCRIMINATORS_REMAINS_READABLE()
        {
            const string oldRegistry = "{\"SchemaVersion\":\"1.0\",\"Variables\":[]}";
            var registry = new ProjectVariablesStore().Deserialize(oldRegistry);
            var design = new SelectivePalletDesignStore().Deserialize(
                new SelectivePalletDesignStore().Serialize(Authored(6.0)));

            Assert.Equal(ProjectVariablesReadOutcome.Readable, registry.Outcome);
            Assert.True(registry.CanWrite);
            Assert.Equal(6.0, design.VerticalClearance);
        }

        [Fact]
        [Trait("Gate", "G8-Legacy")]
        public void LEGACY_LINKED_PROPERTY_EFFECTIVE_RESOLUTION_REMAINS_UNCHANGED()
        {
            var authored = Authored(6.0, Reference());
            var result = new SelectiveEffectiveDesignResolver().Resolve(authored, Registry(30.0));

            Assert.True(result.IsSuccess);
            Assert.Equal(30.0, result.Design.VerticalClearance);
            Assert.Equal(6.0, authored.VerticalClearance);
        }

        [Fact]
        [Trait("Gate", "G8-Legacy")]
        public void LEGACY_BROKEN_REFERENCE_FAILS_CLOSED_WITHOUT_FROZEN_LITERAL_FALLBACK()
        {
            var result = new SelectiveEffectiveDesignResolver().Resolve(Authored(6.0, Reference()), null);

            Assert.Equal(SelectiveEffectiveOutcome.BrokenProjectVariableReference, result.Outcome);
            Assert.False(result.IsSuccess);
            Assert.Null(result.Design);
            Assert.Equal(VariableIdText, result.VariableId);
        }

        [Fact]
        [Trait("Gate", "G8-Legacy")]
        public void LEGACY_BOM_CONSUMES_THE_SAME_EFFECTIVE_VALUE()
        {
            var linked = Bom(Authored(6.0, Reference()), Registry(30.0));
            var equivalentLiteral = Bom(Authored(30.0), null);
            var frozenLiteral = Bom(Authored(6.0), null);

            Assert.Equal(Signature(equivalentLiteral), Signature(linked));
            Assert.NotEqual(Signature(frozenLiteral), Signature(linked));
        }

        [Fact]
        [Trait("Gate", "G8-Legacy")]
        public void LEGACY_RESTAMP_PRESERVES_REFERENCE_AND_EXPORT_MATERIALIZES_EFFECTIVE()
        {
            var store = new SelectivePalletDesignStore();
            var restamped = SelectiveAuthoredRestamp.Restamp(
                store.Serialize(Authored(6.0, Reference())),
                "11111111-2222-3333-4444-555555555555",
                "Rack copia");

            Assert.True(restamped.IsSuccess);
            var copied = store.Deserialize(restamped.DesignJson);
            Assert.Equal(VariableIdText, copied.PropertyValues[PropertyToken].VariableId);

            var exported = SelectiveLibraryExport.Materialize(copied, Registry(30.0));
            Assert.True(exported.IsSuccess);
            Assert.Equal(30.0, exported.Document.VerticalClearance);
            Assert.Null(exported.Document.PropertyValues);
        }

        [Fact]
        [Trait("Gate", "G8-Legacy")]
        public void LEGACY_ONLY_CONTENT_PRESERVES_EXISTING_SCHEMA_AUTHORITIES()
        {
            var registryStore = new ProjectVariablesStore();
            var current = Registry(6.0);
            var compatibleMinor = Registry(6.0);
            compatibleMinor.SchemaVersion = "1.7";

            Assert.Contains("\"SchemaVersion\":\"1.0\"", registryStore.Serialize(current));
            Assert.Contains("\"SchemaVersion\":\"1.7\"", registryStore.Serialize(compatibleMinor));

            var selectiveStore = new SelectivePalletDesignStore();
            var literal = Authored(6.0);
            var linked = Authored(6.0, Reference());
            selectiveStore.Serialize(literal);
            selectiveStore.Serialize(linked);

            Assert.Equal(SelectivePalletDesignDocument.CurrentSchemaVersion, literal.SchemaVersion);
            Assert.Equal(SelectivePalletDesignDocument.PromotedSchemaVersion, linked.SchemaVersion);
        }

        private static ProjectVariablesDocument Registry(double value)
        {
            var document = ProjectVariablesDocument.CreateNew();
            document.Variables.Add(new ProjectVariableDocument
            {
                VariableId = VariableIdText,
                Name = "Holgura vertical",
                Type = "Length",
                Definition = new ProjectVariableDefinitionDocument { Kind = "literal", Value = value },
            });
            return document;
        }

        private static SelectivePropertyValueDocument Reference()
            => SelectivePropertyValueDocument.ToProjectVariable(VariableIdText);

        private static SelectivePalletDesignDocument Authored(
            double clearance,
            SelectivePropertyValueDocument binding = null)
        {
            var document = SelectivePalletDesignDocument.From(Design(clearance), RackId, "Rack A");
            if (binding != null)
            {
                document.PropertyValues = new Dictionary<string, SelectivePropertyValueDocument>
                {
                    [PropertyToken] = binding,
                };
            }

            return document;
        }

        private static SelectivePalletDesign Design(double clearance)
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId,
                PostPeralte = 3.0,
                PalletTolerance = 4.0,
                VerticalClearance = clearance,
                FloorBeamRise = 4.0,
                PalletDepth = 48.0,
                DepthCount = 1,
                DrawBasePlate = true,
            };
            var bay = new SelectiveBayDesign { FloorBeam = true };
            for (var index = 0; index < 2; index++)
            {
                bay.Levels.Add(new SelectiveCell
                {
                    Pallet = new Tarima { Frente = 42.0, Alto = 60.0 },
                    PalletCount = 2,
                    BeamId = BeamId,
                    BeamPeralte = 4.0,
                });
            }

            design.Bays.Add(bay);
            return design;
        }

        private static BillOfMaterials Bom(
            SelectivePalletDesignDocument authored,
            ProjectVariablesDocument registry)
        {
            var effective = new SelectiveEffectiveDesignResolver().Resolve(authored, registry);
            Assert.True(effective.IsSuccess);
            var catalog = JsonRackCatalogProvider.FromBaseDirectory().Load();
            return SelectiveBomBuilder.Build(new SelectiveGeometryResolver().Resolve(effective.Design, catalog), catalog);
        }

        private static string Signature(BillOfMaterials bom)
            => string.Join("|", bom.Lines.Select(line =>
                line.Category + ":" + line.ProfileId + ":" + line.Length.ToString("R") + "x" + line.Quantity));
    }
}
