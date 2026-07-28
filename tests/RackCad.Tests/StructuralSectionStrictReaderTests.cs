using System;
using System.Linq;
using RackCad.Application.StructuralSections;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// The strict reader and the status overlay.
    ///
    /// Every case here is one the TOLERANT reader would swallow — that contrast is the whole reason a second
    /// reader exists. <see cref="CsvLexerTests"/> proves the tolerant one did not change.
    /// </summary>
    public class StructuralSectionStrictReaderTests
    {
        private static readonly string[] Columns =
            StructuralSectionCsvSchema.ColumnsFor(StructuralSectionFamily.W);

        private static string Header() => string.Join(",", Columns) + "\n";

        /// <summary>A minimal, valid W row keyed by column name, so a test can corrupt exactly one cell.</summary>
        private static string Row(params (string Column, string Value)[] overrides)
        {
            var values = Columns.ToDictionary(column => column, column => string.Empty, StringComparer.Ordinal);

            values[StructuralSectionCsvSchema.SectionId] = "AISC-W-W12X26";
            values[StructuralSectionCsvSchema.Family] = "W";
            values[StructuralSectionCsvSchema.EdiDesignation] = "W12X26";
            values[StructuralSectionCsvSchema.ManualLabel] = "W12X26";
            values[StructuralSectionCsvSchema.SourceId] = "AISC-SHAPES";
            values[StructuralSectionCsvSchema.SourceRevision] = "16.0";
            values[StructuralSectionCsvSchema.WeightPerLength] = "26";
            values[StructuralSectionCsvSchema.NativeUnitSystem] = "US_CUSTOMARY";
            values["A"] = "7.65";
            values["d"] = "12.2";
            values["bf"] = "6.49";
            values["tw"] = "0.23";
            values["tf"] = "0.38";

            foreach (var entry in overrides)
            {
                values[entry.Column] = entry.Value;
            }

            return string.Join(",", Columns.Select(column => values[column])) + "\n";
        }

        private static StrictCsvTable Parse(string text) =>
            StrictCsvTable.Parse(StructuralSectionCsvSchema.WFile, text, Columns);

        [Fact]
        public void AValidFileParsesAndRoundTripsThroughTheSerializer()
        {
            var table = Parse(Header() + Row());
            var section = StructuralSectionCsvSerializer.FromRow(StructuralSectionFamily.W, table.Rows[0]);

            Assert.Equal("AISC-W-W12X26", section.SectionId.Value);
            Assert.Equal(26, section.WeightPerLength);

            var reparsed = Parse(StructuralSectionCsvWriter.WriteFamily(StructuralSectionFamily.W, new[] { section }));
            var again = StructuralSectionCsvSerializer.FromRow(StructuralSectionFamily.W, reparsed.Rows[0]);

            Assert.Equal(section.SectionId, again.SectionId);
            Assert.Equal(section.WeightPerLength, again.WeightPerLength);
            Assert.Equal(
                ((WSectionDimensions)section.Dimensions).Depth,
                ((WSectionDimensions)again.Dimensions).Depth);
        }

        [Fact]
        public void AMissingHeaderIsAnError()
        {
            var text = string.Join(",", Columns.Skip(1)) + "\n";
            var error = Assert.Throws<StructuralSectionCsvException>(() => Parse(text));

            Assert.Equal(StructuralSectionCsvSchema.SectionId, error.Column);
            Assert.Contains("obligatorio", error.Reason, StringComparison.Ordinal);
        }

        [Fact]
        public void ADuplicateHeaderIsAnError()
        {
            var text = string.Join(",", Columns) + "," + StructuralSectionCsvSchema.SectionId + "\n";
            var error = Assert.Throws<StructuralSectionCsvException>(() => Parse(text));

            Assert.Contains("duplicado", error.Reason, StringComparison.Ordinal);
        }

        [Fact]
        public void AnUnknownHeaderIsAnError()
        {
            // The tolerant reader would drop it into the open Properties bag; here an unexpected column means
            // the file came from a version this build does not understand.
            var text = string.Join(",", Columns) + ",columnaInventada\n";
            var error = Assert.Throws<StructuralSectionCsvException>(() => Parse(text));

            Assert.Equal("columnaInventada", error.Column);
            Assert.Contains("desconocido", error.Reason, StringComparison.Ordinal);
        }

        [Fact]
        public void AnEmptyHeaderCellIsAnError()
        {
            var text = string.Join(",", Columns) + ",\n";
            Assert.Throws<StructuralSectionCsvException>(() => Parse(text));
        }

        [Fact]
        public void ARowWithTheWrongNumberOfCellsIsAnError()
        {
            var error = Assert.Throws<StructuralSectionCsvException>(() => Parse(Header() + "AISC-W-W12X26,W\n"));

            Assert.Equal(2, error.Line);
            Assert.Contains("celdas", error.Reason, StringComparison.Ordinal);
        }

        [Fact]
        public void AnEmptyIdIsAnError()
        {
            var table = Parse(Header() + Row((StructuralSectionCsvSchema.SectionId, string.Empty)));
            var error = Assert.Throws<StructuralSectionCsvException>(
                () => StructuralSectionCsvSerializer.FromRow(StructuralSectionFamily.W, table.Rows[0]));

            Assert.Equal(StructuralSectionCsvSchema.SectionId, error.Column);
        }

        [Fact]
        public void AnInvalidNumberIsAnError_AndNamesFileLineAndColumn()
        {
            var table = Parse(Header() + Row(("A", "siete")));
            var error = Assert.Throws<StructuralSectionCsvException>(
                () => StructuralSectionCsvSerializer.FromRow(StructuralSectionFamily.W, table.Rows[0]));

            Assert.Equal(StructuralSectionCsvSchema.WFile, error.FileName);
            Assert.Equal(2, error.Line);
            Assert.Equal("A", error.Column);
            Assert.Equal("AISC-W-W12X26", error.SectionId);
        }

        [Theory]
        [InlineData("NaN")]
        [InlineData("Infinity")]
        [InlineData("-Infinity")]
        public void NaNAndInfinityAreErrors(string value)
        {
            var table = Parse(Header() + Row(("A", value)));

            Assert.Throws<StructuralSectionCsvException>(
                () => StructuralSectionCsvSerializer.FromRow(StructuralSectionFamily.W, table.Rows[0]));
        }

        [Fact]
        public void ANumberWithAThousandsSeparatorIsAnError()
        {
            // Excel would happily write "1,234" and the invariant parse must NOT accept it as 1234.
            var table = Parse(Header() + Row(("A", "\"1,234\"")));

            Assert.Throws<StructuralSectionCsvException>(
                () => StructuralSectionCsvSerializer.FromRow(StructuralSectionFamily.W, table.Rows[0]));
        }

        [Fact]
        public void AMissingRequiredValueIsAnError()
        {
            var table = Parse(Header() + Row((StructuralSectionCsvSchema.WeightPerLength, string.Empty)));

            Assert.Throws<StructuralSectionCsvException>(
                () => StructuralSectionCsvSerializer.FromRow(StructuralSectionFamily.W, table.Rows[0]));
        }

        [Fact]
        public void AnEmptyOptionalCellBecomesNull_NeverZero()
        {
            var table = Parse(Header() + Row(("Ix", string.Empty), ("kdes", string.Empty)));
            var section = StructuralSectionCsvSerializer.FromRow(StructuralSectionFamily.W, table.Rows[0]);

            Assert.Null(section.Properties.Ix);
            Assert.Null(((WSectionDimensions)section.Dimensions).KDesign);
        }

        [Fact]
        public void AnInvalidFamilyTokenIsAnError()
        {
            var table = Parse(Header() + Row((StructuralSectionCsvSchema.Family, "POSTE")));

            var error = Assert.Throws<StructuralSectionCsvException>(
                () => StructuralSectionCsvSerializer.FromRow(StructuralSectionFamily.W, table.Rows[0]));

            Assert.Equal(StructuralSectionCsvSchema.Family, error.Column);
        }

        [Fact]
        public void ARowDeclaringADifferentFamilyThanItsFileIsAnError()
        {
            var table = Parse(Header() + Row((StructuralSectionCsvSchema.Family, "C")));

            var error = Assert.Throws<StructuralSectionCsvException>(
                () => StructuralSectionCsvSerializer.FromRow(StructuralSectionFamily.W, table.Rows[0]));

            Assert.Contains("familia", error.Reason, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void AnInvalidUnitSystemTokenIsAnError()
        {
            var table = Parse(Header() + Row((StructuralSectionCsvSchema.NativeUnitSystem, "PULGADAS")));

            Assert.Throws<StructuralSectionCsvException>(
                () => StructuralSectionCsvSerializer.FromRow(StructuralSectionFamily.W, table.Rows[0]));
        }

        [Fact]
        public void AnInvalidBooleanIsAnError()
        {
            var table = Parse(Header() + Row(("T_F", "SI")));

            Assert.Throws<StructuralSectionCsvException>(
                () => StructuralSectionCsvSerializer.FromRow(StructuralSectionFamily.W, table.Rows[0]));
        }

        [Fact]
        public void AnEmptyFileIsAnError()
        {
            Assert.Throws<StructuralSectionCsvException>(() => Parse(string.Empty));
        }

        [Fact]
        public void BlankLinesBetweenRowsAreIgnored()
        {
            var table = Parse(Header() + Row() + "\n" + Row((StructuralSectionCsvSchema.SectionId, "AISC-W-W12X30"),
                (StructuralSectionCsvSchema.EdiDesignation, "W12X30"),
                (StructuralSectionCsvSchema.ManualLabel, "W12X30")));

            Assert.Equal(2, table.Rows.Count);
        }

        // ---- Status overlay --------------------------------------------------------------------------------

        [Fact]
        public void TheOverlayDisablesWithoutDeleting()
        {
            var sections = new[]
            {
                StructuralSectionModelTests.Section(StructuralSectionFamily.W, "W12X26", "W12X26", 26),
                StructuralSectionModelTests.Section(StructuralSectionFamily.W, "W12X30", "W12X30", 30)
            };

            var applied = CsvStructuralSectionCatalogProvider.ApplyStatus(sections, new[]
            {
                new StructuralSectionStatusOverride
                {
                    SectionId = StructuralSectionId.Parse("AISC-W-W12X26"),
                    IsEnabled = false,
                    Notes = "sin existencias"
                }
            });

            Assert.Equal(2, applied.Count);

            var catalog = StructuralSectionCatalog.Create(applied, new[] { StructuralSectionModelTests.AiscSource });

            Assert.True(catalog.TryGetById("AISC-W-W12X26", out var disabled));
            Assert.False(disabled.IsEnabled);
            Assert.Equal("sin existencias", disabled.StatusNotes);
            Assert.Single(catalog.Enabled);
        }

        [Fact]
        public void AnUnknownIdInTheOverlayIsAnError()
        {
            var sections = new[]
            {
                StructuralSectionModelTests.Section(StructuralSectionFamily.W, "W12X26", "W12X26", 26)
            };

            var error = Assert.Throws<StructuralSectionCsvException>(
                () => CsvStructuralSectionCatalogProvider.ApplyStatus(sections, new[]
                {
                    new StructuralSectionStatusOverride
                    {
                        SectionId = StructuralSectionId.Parse("AISC-W-W99X999"),
                        IsEnabled = false
                    }
                }));

            Assert.Contains("no existe", error.Reason, StringComparison.Ordinal);
        }

        [Fact]
        public void ADuplicatedIdInTheOverlayIsAnError()
        {
            var sections = new[]
            {
                StructuralSectionModelTests.Section(StructuralSectionFamily.W, "W12X26", "W12X26", 26)
            };

            var entry = new StructuralSectionStatusOverride
            {
                SectionId = StructuralSectionId.Parse("AISC-W-W12X26"),
                IsEnabled = false
            };

            var error = Assert.Throws<StructuralSectionCsvException>(
                () => CsvStructuralSectionCatalogProvider.ApplyStatus(sections, new[] { entry, entry }));

            Assert.Contains("mas de una vez", error.Reason, StringComparison.Ordinal);
        }

        [Fact]
        public void AnEmptyOverlayLeavesEverythingEnabled()
        {
            var sections = new[]
            {
                StructuralSectionModelTests.Section(StructuralSectionFamily.W, "W12X26", "W12X26", 26)
            };

            var applied = CsvStructuralSectionCatalogProvider.ApplyStatus(
                sections, new StructuralSectionStatusOverride[0]);

            Assert.All(applied, section => Assert.True(section.IsEnabled));
        }

        [Fact]
        public void TheOverlayRoundTripsThroughItsFile()
        {
            var text = StructuralSectionCsvWriter.WriteStatus(new[]
            {
                new StructuralSectionStatusOverride
                {
                    SectionId = StructuralSectionId.Parse("AISC-W-W12X26"),
                    IsEnabled = false,
                    Notes = "motivo con, coma"
                }
            });

            var table = StrictCsvTable.Parse(
                StructuralSectionCsvSchema.StatusFile, text, StructuralSectionCsvSchema.StatusColumns);
            var row = Assert.Single(table.Rows);

            Assert.Equal("AISC-W-W12X26", row.RequiredText(StructuralSectionCsvSchema.SectionId));
            Assert.False(row.RequiredBool(StructuralSectionCsvSchema.IsEnabled));
            Assert.Equal("motivo con, coma", row.OptionalText(StructuralSectionCsvSchema.Notes));
        }
    }
}
