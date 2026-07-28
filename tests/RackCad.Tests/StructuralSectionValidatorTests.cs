using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs.Validation;
using RackCad.Application.StructuralSections;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// The neutral catalog's own validator, driven with synthetic sections.
    ///
    /// It reuses I-19's severity vocabulary and NOTHING else: <c>CatalogValidator</c> is untouched, which
    /// <see cref="CatalogValidatorTests"/> keeps proving on its own data.
    /// </summary>
    public class StructuralSectionValidatorTests
    {
        private static readonly StructuralSectionCatalogValidator Validator = new StructuralSectionCatalogValidator();

        [Fact]
        public void AConsistentCatalogHasNoIssues()
        {
            var report = Validator.Validate(Catalog(Valid()));

            Assert.True(report.IsValid(strict: true), report.Format());
            Assert.Empty(report.Issues);
        }

        [Fact]
        public void AnUnknownSourceIsAnError()
        {
            var section = With(Valid(), identity: Identity("W12X26", sourceId: "FUENTE-QUE-NO-EXISTE"));
            var report = Validator.Validate(Catalog(section));

            Assert.Single(report.WithCode(StructuralSectionCatalogValidator.CodeUnknownSource));
        }

        [Fact]
        public void AMissingRevisionIsAnError()
        {
            var section = With(Valid(), identity: Identity("W12X26", revision: string.Empty));
            var report = Validator.Validate(Catalog(section));

            Assert.NotEmpty(report.WithCode(StructuralSectionCatalogValidator.CodeMissingRevision));
        }

        [Fact]
        public void ARevisionThatContradictsItsSourceIsAnError()
        {
            var section = With(Valid(), identity: Identity("W12X26", revision: "15.0"));
            var report = Validator.Validate(Catalog(section));

            Assert.NotEmpty(report.WithCode(StructuralSectionCatalogValidator.CodeMissingRevision));
        }

        [Fact]
        public void AnIdThatDoesNotMatchItsDesignationIsAnError()
        {
            var identity = new StructuralSectionIdentity
            {
                SectionId = StructuralSectionId.Parse("AISC-W-INVENTADO"),
                Family = StructuralSectionFamily.W,
                EdiDesignation = "W12X26",
                ManualLabel = "W12X26",
                SourceId = StructuralSectionSource.AiscShapesId,
                SourceRevision = "16.0"
            };

            var report = Validator.Validate(Catalog(With(Valid(), identity: identity)));

            Assert.Single(report.WithCode(StructuralSectionCatalogValidator.CodeIdMismatch));
        }

        [Fact]
        public void ZeroWhereAMagnitudeIsExpectedIsAnError()
        {
            var section = With(Valid(), properties: new StructuralSectionProperties { Area = 7.65, Ix = 0 });
            var report = Validator.Validate(Catalog(section));

            Assert.Single(report.WithCode(StructuralSectionCatalogValidator.CodeZeroInsteadOfNull));
        }

        [Fact]
        public void ZeroWhereAPositionIsExpectedIsAccepted()
        {
            // zB is genuinely 0 for equal-leg angles. This is the regression that keeps 61 real rows out of
            // the error list.
            var angle = new StructuralSectionDefinition
            {
                Identity = Identity("L4X4X1/4", family: StructuralSectionFamily.Angle),
                WeightPerLength = 6.6,
                NativeUnitSystem = StructuralSectionUnitSystem.UsCustomary,
                Dimensions = new AngleSectionDimensions { ShortLeg = 4, LongLeg = 4, Thickness = 0.25 },
                Properties = new StructuralSectionProperties { Area = 1.93, ZB = 0 }
            };

            var report = Validator.Validate(Catalog(angle));

            Assert.Empty(report.WithCode(StructuralSectionCatalogValidator.CodeZeroInsteadOfNull));
            Assert.True(report.IsValid(strict: true), report.Format());
        }

        [Fact]
        public void ANonPositiveWeightIsAnError()
        {
            var report = Validator.Validate(Catalog(With(Valid(), weight: -1)));

            Assert.Single(report.WithCode(StructuralSectionCatalogValidator.CodeNonPositiveWeight));
        }

        [Fact]
        public void AMissingAreaIsAnError()
        {
            var report = Validator.Validate(Catalog(With(Valid(), properties: new StructuralSectionProperties())));

            Assert.NotEmpty(report.WithCode(StructuralSectionCatalogValidator.CodeMissingRequiredField));
        }

        [Fact]
        public void ANonPositiveAreaIsAnError()
        {
            var report = Validator.Validate(
                Catalog(With(Valid(), properties: new StructuralSectionProperties { Area = -3 })));

            Assert.Single(report.WithCode(StructuralSectionCatalogValidator.CodeNonPositiveArea));
        }

        [Fact]
        public void AFamilyRequiredDimensionThatIsMissingIsAnError()
        {
            var section = With(Valid(), dimensions: new WSectionDimensions { Depth = 12.2, FlangeWidth = 6.49 });
            var report = Validator.Validate(Catalog(section));

            Assert.Equal(2, report.WithCode(StructuralSectionCatalogValidator.CodeFamilyInvariant).Count());
        }

        [Fact]
        public void DimensionsThatDoNotMatchTheDeclaredFamilyAreAnError()
        {
            var section = With(Valid(), dimensions: new AngleSectionDimensions
            {
                ShortLeg = 4, LongLeg = 4, Thickness = 0.25
            });

            var report = Validator.Validate(Catalog(section));

            Assert.Single(report.WithCode(StructuralSectionCatalogValidator.CodeFamilyMismatch));
        }

        [Fact]
        public void AnHssWhoseDesignThicknessExceedsTheNominalIsAnError()
        {
            var hss = new StructuralSectionDefinition
            {
                Identity = Identity("HSS4X4X.250", family: StructuralSectionFamily.HssRectangular),
                WeightPerLength = 12.21,
                NativeUnitSystem = StructuralSectionUnitSystem.UsCustomary,
                Dimensions = new HssRectangularSectionDimensions
                {
                    OverallDepth = 4, OverallWidth = 4, FlatDepth = 3.3, FlatWidth = 3.3,
                    NominalThickness = 0.25, DesignThickness = 0.30
                },
                Properties = new StructuralSectionProperties { Area = 3.37 }
            };

            var report = Validator.Validate(Catalog(hss));

            Assert.Single(report.WithCode(StructuralSectionCatalogValidator.CodeThicknessNotDistinguished));
        }

        [Fact]
        public void AnHssWhoseFlatWallReachesTheOverallWallIsAnError()
        {
            var hss = new StructuralSectionDefinition
            {
                Identity = Identity("HSS4X4X.250", family: StructuralSectionFamily.HssRectangular),
                WeightPerLength = 12.21,
                NativeUnitSystem = StructuralSectionUnitSystem.UsCustomary,
                Dimensions = new HssRectangularSectionDimensions
                {
                    OverallDepth = 4, OverallWidth = 4, FlatDepth = 4, FlatWidth = 3.3,
                    NominalThickness = 0.25, DesignThickness = 0.233
                },
                Properties = new StructuralSectionProperties { Area = 3.37 }
            };

            var report = Validator.Validate(Catalog(hss));

            Assert.Single(report.WithCode(StructuralSectionCatalogValidator.CodeFamilyInvariant));
        }

        [Fact]
        public void AnAngleWithItsLegsSwappedIsAnError()
        {
            var angle = new StructuralSectionDefinition
            {
                Identity = Identity("L6X4X1/2", family: StructuralSectionFamily.Angle),
                WeightPerLength = 16,
                NativeUnitSystem = StructuralSectionUnitSystem.UsCustomary,
                Dimensions = new AngleSectionDimensions { ShortLeg = 6, LongLeg = 4, Thickness = 0.5 },
                Properties = new StructuralSectionProperties { Area = 4.79 }
            };

            var report = Validator.Validate(Catalog(angle));

            Assert.Single(report.WithCode(StructuralSectionCatalogValidator.CodeFamilyInvariant));
        }

        [Fact]
        public void AMaterialGradeOnAnAiscSectionIsAWarning_BecauseTheSourcePublishesNone()
        {
            var report = Validator.Validate(Catalog(With(Valid(), grade: "A992")));

            Assert.Single(report.WithCode(StructuralSectionCatalogValidator.CodeMaterialGradeInferred));
            Assert.True(report.IsValid());               // a warning, not an error
            Assert.False(report.IsValid(strict: true));  // fatal for a deployment
        }

        [Fact]
        public void AnAmbiguousDesignationIsAnError()
        {
            var first = With(Valid(), identity: Identity("W12X26", label: "AMBIGUA"));
            var second = With(Valid(), identity: Identity("W12X30", label: "AMBIGUA"));
            var report = Validator.Validate(Catalog(first, second));

            Assert.Single(report.WithCode(StructuralSectionCatalogValidator.CodeAmbiguousDesignation));
        }

        // ---- Manifest ---------------------------------------------------------------------------------------

        [Fact]
        public void AManifestThatAgreesWithTheCatalogProducesNoIssue()
        {
            var catalog = Catalog(Valid());
            var report = Validator.Validate(catalog, Manifest(catalog), HashOfEverything);

            Assert.True(report.IsValid(strict: true), report.Format());
        }

        [Fact]
        public void AWrongCountInTheManifestIsAnError()
        {
            var catalog = Catalog(Valid());
            var manifest = Manifest(catalog, totalOverride: 99);
            var report = Validator.Validate(catalog, manifest, HashOfEverything);

            Assert.NotEmpty(report.WithCode(StructuralSectionCatalogValidator.CodeManifestCount));
        }

        [Fact]
        public void AWrongHashInTheManifestIsAnError()
        {
            var catalog = Catalog(Valid());
            var report = Validator.Validate(catalog, Manifest(catalog), (_ => "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF"));

            Assert.Equal(
                StructuralSectionCsvSchema.ImmutableFiles().Length,
                report.WithCode(StructuralSectionCatalogValidator.CodeManifestHash).Count());
        }

        // ---- F5: cada metadata del manifiesto, por separado --------------------------------------------------

        [Fact]
        public void AManifestThatHashesTheMutableOverlayIsAnError()
        {
            var catalog = Catalog(Valid());
            var report = Validator.Validate(catalog, Manifest(catalog, includeOverlay: true), HashOfEverything);

            Assert.Single(report.WithCode(StructuralSectionCatalogValidator.CodeManifestFileSet));
        }

        [Fact]
        public void AManifestThatDeclaresTheSameFileTwiceIsAnError()
        {
            var catalog = Catalog(Valid());
            var report = Validator.Validate(catalog, Manifest(catalog, duplicateFile: true), HashOfEverything);

            Assert.Single(report.WithCode(StructuralSectionCatalogValidator.CodeManifestDuplicateFile));
        }

        [Fact]
        public void AManifestMissingOneOfTheImmutableFilesIsAnError()
        {
            var catalog = Catalog(Valid());
            var manifest = Manifest(catalog);
            var trimmed = new StructuralSectionsManifest
            {
                SchemaVersion = manifest.SchemaVersion,
                CatalogId = manifest.CatalogId,
                SourceId = manifest.SourceId,
                SourceRevision = manifest.SourceRevision,
                IdNamespace = manifest.IdNamespace,
                SourceFileName = manifest.SourceFileName,
                SourceSha256 = manifest.SourceSha256,
                SourceWorksheet = manifest.SourceWorksheet,
                MapperVersion = manifest.MapperVersion,
                CountsByFamily = manifest.CountsByFamily,
                TotalCount = manifest.TotalCount,
                RejectedSelectedRows = manifest.RejectedSelectedRows,
                ExcludedTypeCounts = manifest.ExcludedTypeCounts,
                Files = manifest.Files.Skip(1).ToArray()
            };

            var report = Validator.Validate(catalog, trimmed, HashOfEverything);

            Assert.Single(report.WithCode(StructuralSectionCatalogValidator.CodeManifestFileSet));
        }

        [Theory]
        [InlineData("")]
        [InlineData("no-son-64-hex")]
        [InlineData("ZZZ0CEB96A0D938AE1A6BD9637CB10A1E269225B5D668DCE5B0BDC8D86013496")]
        [InlineData("82D0CEB96A0D938AE1A6BD9637CB10A1E269225B5D668DCE5B0BDC8D8601349")]
        public void AWorkbookHashThatIsNotSixtyFourHexIsAnError(string sourceSha)
        {
            var catalog = Catalog(Valid());
            var report = Validator.Validate(catalog, Manifest(catalog, sourceSha: sourceSha), HashOfEverything);

            Assert.NotEmpty(report.WithCode(StructuralSectionCatalogValidator.CodeManifestMalformedHash));
        }

        [Fact]
        public void AManifestOfAnotherCatalogIsAnError()
        {
            var catalog = Catalog(Valid());
            var report = Validator.Validate(catalog, Manifest(catalog, catalogId: "otro-catalogo"), HashOfEverything);

            Assert.NotEmpty(report.WithCode(StructuralSectionCatalogValidator.CodeManifestMetadata));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void AManifestWithoutItsWorksheetIsAnError(string worksheet)
        {
            var catalog = Catalog(Valid());
            var report = Validator.Validate(catalog, Manifest(catalog, worksheet: worksheet), HashOfEverything);

            Assert.NotEmpty(report.WithCode(StructuralSectionCatalogValidator.CodeManifestMetadata));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void AManifestWithoutItsMapperVersionIsAnError(string mapperVersion)
        {
            var catalog = Catalog(Valid());
            var report = Validator.Validate(
                catalog, Manifest(catalog, mapperVersion: mapperVersion), HashOfEverything);

            Assert.NotEmpty(report.WithCode(StructuralSectionCatalogValidator.CodeManifestMetadata));
        }

        [Fact]
        public void AManifestWithoutItsIdNamespaceIsAnError()
        {
            var catalog = Catalog(Valid());
            var report = Validator.Validate(catalog, Manifest(catalog, idNamespace: null), HashOfEverything);

            Assert.NotEmpty(report.WithCode(StructuralSectionCatalogValidator.CodeManifestMetadata));
        }

        [Fact]
        public void AManifestWhoseIdNamespaceContradictsTheSourceIsAnError()
        {
            var catalog = Catalog(Valid());
            var report = Validator.Validate(catalog, Manifest(catalog, idNamespace: "OTRO"), HashOfEverything);

            Assert.NotEmpty(report.WithCode(StructuralSectionCatalogValidator.CodeManifestSourceMismatch));
        }

        [Fact]
        public void AManifestWithAnInvalidIdNamespaceIsAnError()
        {
            var catalog = Catalog(Valid());
            var report = Validator.Validate(catalog, Manifest(catalog, idNamespace: "no-vale"), HashOfEverything);

            Assert.NotEmpty(report.WithCode(StructuralSectionCatalogValidator.CodeInvalidIdNamespace));
        }

        // ---- F4: el overlay se valida aparte -----------------------------------------------------------------

        [Fact]
        public void TheOverlayIsValidatedSeparately_AndAnUnknownIdIsAnError()
        {
            var catalog = Catalog(Valid());
            var report = Validator.ValidateOverlay(catalog, new[]
            {
                new StructuralSectionStatusOverride
                {
                    SectionId = StructuralSectionId.Parse("AISC-W-W99X999"),
                    IsEnabled = false
                }
            });

            Assert.False(report.IsValid());
        }

        [Fact]
        public void TheOverlayRejectsARepeatedId()
        {
            var catalog = Catalog(Valid());
            var entry = new StructuralSectionStatusOverride
            {
                SectionId = StructuralSectionId.Parse("AISC-W-W12X26"),
                IsEnabled = false
            };

            var report = Validator.ValidateOverlay(catalog, new[] { entry, entry });

            Assert.Single(report.WithCode(StructuralSectionCatalogValidator.CodeDuplicateId));
        }

        [Fact]
        public void AValidOverlayProducesNoIssue()
        {
            var catalog = Catalog(Valid());
            var report = Validator.ValidateOverlay(catalog, new[]
            {
                new StructuralSectionStatusOverride
                {
                    SectionId = StructuralSectionId.Parse("AISC-W-W12X26"),
                    IsEnabled = false,
                    Notes = "sin existencias"
                }
            });

            Assert.True(report.IsValid(strict: true), report.Format());
        }

        [Fact]
        public void AManifestReportingRejectedRowsIsAnError()
        {
            var catalog = Catalog(Valid());
            var manifest = Manifest(catalog, rejected: 3);
            var report = Validator.Validate(catalog, manifest, HashOfEverything);

            Assert.Single(report.WithCode(StructuralSectionCatalogValidator.CodeManifestRejectedRows));
        }

        [Fact]
        public void AManifestThatHashesItselfIsAnError()
        {
            var catalog = Catalog(Valid());
            var manifest = Manifest(catalog, includeSelf: true);
            var report = Validator.Validate(catalog, manifest, HashOfEverything);

            Assert.NotEmpty(report.WithCode(StructuralSectionCatalogValidator.CodeManifestMetadata));
        }

        [Fact]
        public void AManifestWithoutTheWorkbookHashIsAnError()
        {
            var catalog = Catalog(Valid());
            var manifest = Manifest(catalog, sourceSha: null);
            var report = Validator.Validate(catalog, manifest, HashOfEverything);

            Assert.NotEmpty(report.WithCode(StructuralSectionCatalogValidator.CodeManifestMalformedHash));
        }

        [Fact]
        public void TheManifestRoundTripsThroughItsJson()
        {
            var catalog = Catalog(Valid());
            var manifest = Manifest(catalog);
            var reparsed = StructuralSectionsManifest.FromJson(manifest.ToJson());

            Assert.Equal(manifest.SchemaVersion, reparsed.SchemaVersion);
            Assert.Equal(manifest.SourceSha256, reparsed.SourceSha256);
            Assert.Equal(manifest.TotalCount, reparsed.TotalCount);
            Assert.Equal(manifest.CountsByFamily["W"], reparsed.CountsByFamily["W"]);
            Assert.Equal(manifest.Files.Count, reparsed.Files.Count);
            Assert.Equal(manifest.ToJson(), reparsed.ToJson());
        }

        [Fact]
        public void TheManifestJsonUsesLineFeedsOnly_SoItIsIdenticalOnEveryOs()
        {
            // Utf8JsonWriter's indented mode emits Environment.NewLine, which would make the SAME manifest
            // differ between the Windows and Linux CI jobs. The hand-written serializer must not.
            var json = Manifest(Catalog(Valid())).ToJson();

            Assert.DoesNotContain("\r", json, StringComparison.Ordinal);
            Assert.EndsWith("}\n", json, StringComparison.Ordinal);
        }

        // ---- Helpers -----------------------------------------------------------------------------------------

        private static StructuralSectionCatalog Catalog(params StructuralSectionDefinition[] sections) =>
            StructuralSectionCatalog.Create(sections, new[] { StructuralSectionModelTests.AiscSource });

        private static StructuralSectionIdentity Identity(
            string edi,
            StructuralSectionFamily family = StructuralSectionFamily.W,
            string label = null,
            string sourceId = StructuralSectionSource.AiscShapesId,
            string revision = "16.0") =>
            new StructuralSectionIdentity
            {
                SectionId = StructuralSectionId.Create(StructuralSectionSource.AiscIdNamespace, family, edi),
                Family = family,
                EdiDesignation = edi,
                ManualLabel = label ?? edi,
                SourceId = sourceId,
                SourceRevision = revision
            };

        private static StructuralSectionDefinition Valid() => new StructuralSectionDefinition
        {
            Identity = Identity("W12X26"),
            WeightPerLength = 26,
            NativeUnitSystem = StructuralSectionUnitSystem.UsCustomary,
            Dimensions = new WSectionDimensions
            {
                Depth = 12.2, FlangeWidth = 6.49, WebThickness = 0.23, FlangeThickness = 0.38
            },
            Properties = new StructuralSectionProperties { Area = 7.65, Ix = 204, Iy = 17.3 }
        };

        private static StructuralSectionDefinition With(
            StructuralSectionDefinition section,
            StructuralSectionIdentity identity = null,
            double? weight = null,
            IStructuralSectionDimensions dimensions = null,
            StructuralSectionProperties properties = null,
            string grade = null) =>
            new StructuralSectionDefinition
            {
                Identity = identity ?? section.Identity,
                WeightPerLength = weight ?? section.WeightPerLength,
                NativeUnitSystem = section.NativeUnitSystem,
                Dimensions = dimensions ?? section.Dimensions,
                Properties = properties ?? section.Properties,
                MaterialGrade = grade ?? section.MaterialGrade,
                IsEnabled = section.IsEnabled
            };

        /// <summary>A hash that is shaped like a real one: the validator now rejects anything that is not.</summary>
        private const string ValidHash = "0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF";

        private static string HashOfEverything(string _) => ValidHash;

        private static StructuralSectionsManifest Manifest(
            StructuralSectionCatalog catalog,
            int? totalOverride = null,
            int rejected = 0,
            bool includeSelf = false,
            bool includeOverlay = false,
            bool duplicateFile = false,
            string sourceSha = "82D0CEB96A0D938AE1A6BD9637CB10A1E269225B5D668DCE5B0BDC8D86013496",
            string idNamespace = StructuralSectionSource.AiscIdNamespace,
            string catalogId = StructuralSectionsManifest.StructuralSectionsCatalogId,
            string worksheet = "Database v16.0",
            string mapperVersion = "I-36A.2")
        {
            var files = StructuralSectionCsvSchema.ImmutableFiles()
                .Select(name => new StructuralSectionsManifest.ManifestFile { Name = name, Sha256 = ValidHash })
                .ToList();

            if (includeSelf)
            {
                files.Add(new StructuralSectionsManifest.ManifestFile
                {
                    Name = StructuralSectionCsvSchema.ManifestFile,
                    Sha256 = ValidHash
                });
            }

            if (includeOverlay)
            {
                files.Add(new StructuralSectionsManifest.ManifestFile
                {
                    Name = StructuralSectionCsvSchema.StatusFile,
                    Sha256 = ValidHash
                });
            }

            if (duplicateFile)
            {
                files.Add(new StructuralSectionsManifest.ManifestFile
                {
                    Name = StructuralSectionCsvSchema.SourcesFile,
                    Sha256 = ValidHash
                });
            }

            return new StructuralSectionsManifest
            {
                SchemaVersion = StructuralSectionsManifest.CurrentSchemaVersion,
                CatalogId = catalogId,
                SourceId = StructuralSectionSource.AiscShapesId,
                SourceRevision = "16.0",
                IdNamespace = idNamespace,
                SourceFileName = "aisc-shapes-database-v16.0.xlsx",
                SourceSha256 = sourceSha,
                SourceWorksheet = worksheet,
                MapperVersion = mapperVersion,
                CountsByFamily = catalog.CountsByFamily().ToDictionary(
                    pair => StructuralSectionFamilies.ToToken(pair.Key),
                    pair => pair.Value,
                    StringComparer.Ordinal),
                TotalCount = totalOverride ?? catalog.Count,
                RejectedSelectedRows = rejected,
                ExcludedTypeCounts = new Dictionary<string, int>(StringComparer.Ordinal),
                Files = files
            };
        }
    }
}
