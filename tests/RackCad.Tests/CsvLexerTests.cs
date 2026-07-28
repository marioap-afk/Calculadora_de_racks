using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-36A pinned the RFC-4180 lexer when it was extracted VERBATIM out of <see cref="CsvCatalogReader"/>
    /// so the new strict structural reader could share it.
    ///
    /// These are REGRESSIONS, not a specification: they encode what the historical parser already did,
    /// quirks included, because the whole value of the extraction is that the tolerant reader's behaviour did
    /// not change. If a future edit "fixes" one of these, it changes how every existing catalog parses.
    /// </summary>
    public class CsvLexerTests
    {
        [Fact]
        public void EmptyText_YieldsNoRows()
        {
            Assert.Empty(CsvLexer.ParseRows(string.Empty));
            Assert.Empty(CsvLexer.ParseRows(null));
        }

        [Fact]
        public void SplitsCommasAndLines()
        {
            var rows = CsvLexer.ParseRows("a,b,c\n1,2,3\n");

            Assert.Equal(2, rows.Count);
            Assert.Equal(new[] { "a", "b", "c" }, rows[0]);
            Assert.Equal(new[] { "1", "2", "3" }, rows[1]);
        }

        [Fact]
        public void TrailingNewline_DoesNotProduceAPhantomRow()
        {
            Assert.Single(CsvLexer.ParseRows("a,b\n"));
        }

        [Fact]
        public void LastRowWithoutNewline_IsStillEmitted()
        {
            var rows = CsvLexer.ParseRows("a,b\n1,2");

            Assert.Equal(2, rows.Count);
            Assert.Equal(new[] { "1", "2" }, rows[1]);
        }

        [Fact]
        public void CarriageReturns_AreDropped_SoCrlfAndLfParseIdentically()
        {
            Assert.Equal(
                CsvLexer.ParseRows("a,b\n1,2\n").Select(row => string.Join("|", row)),
                CsvLexer.ParseRows("a,b\r\n1,2\r\n").Select(row => string.Join("|", row)));
        }

        [Fact]
        public void QuotedField_KeepsCommasAndNewlines()
        {
            var rows = CsvLexer.ParseRows("a,\"uno, dos\",c\n");

            Assert.Equal(new[] { "a", "uno, dos", "c" }, rows[0]);
        }

        [Fact]
        public void DoubledQuoteInsideQuotedField_BecomesOneQuote()
        {
            var rows = CsvLexer.ParseRows("\"dice \"\"hola\"\"\",b\n");

            Assert.Equal("dice \"hola\"", rows[0][0]);
        }

        [Fact]
        public void EmbeddedNewlineInsideQuotes_StaysInTheSameField()
        {
            var rows = CsvLexer.ParseRows("\"linea1\nlinea2\",b\n");

            Assert.Single(rows);
            Assert.Equal("linea1\nlinea2", rows[0][0]);
        }

        [Fact]
        public void BlankLine_ProducesARowWithOneEmptyField()
        {
            // Historical behaviour: a blank line is a row of one empty field, not a skipped line. Both
            // readers filter it later; the LEXER does not.
            var rows = CsvLexer.ParseRows("a\n\nb\n");

            Assert.Equal(3, rows.Count);
            Assert.Equal(new[] { string.Empty }, rows[1]);
        }

        [Fact]
        public void ExtractionPreservedBehaviour_TheTolerantReaderStillParsesTheSameWay()
        {
            // The tolerant reader delegates to the lexer now. This drives it end to end over the awkward
            // cases so the delegation cannot regress silently.
            const string text = "id,displayName\nA,\"uno, dos\"\nB,\"dice \"\"hola\"\"\"\n";

            var entries = CsvCatalogReader.Read<ProbeEntry>(text);

            Assert.Equal(2, entries.Count);
            Assert.Equal("uno, dos", entries[0].DisplayName);
            Assert.Equal("dice \"hola\"", entries[1].DisplayName);
        }

        private sealed class ProbeEntry : CatalogEntryBase
        {
        }
    }
}
