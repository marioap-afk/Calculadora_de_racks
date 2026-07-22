using System.Collections.Generic;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// Characterizes the extracted dynamic-editor safety rules (I-21): the "draws" predicate that drives the toolbar count
    /// and the design snapshot, and the drawable-copy that the design assembler applies. Behavior mirrors the window's old
    /// private SafetyDraws/ReplaceSafetySelections exactly.
    /// </summary>
    public class DynamicEditorSafetyTests
    {
        [Fact]
        public void Draws_Null_IsFalse()
        {
            Assert.False(DynamicEditorSafety.Draws(null));
        }

        [Fact]
        public void Draws_NoQuantityNoSideNoPostSides_IsFalse()
        {
            var selection = new SelectiveSafetySelection { ElementId = "BOTA", Quantity = 0, Side = SafetySide.None };
            Assert.False(DynamicEditorSafety.Draws(selection));
        }

        [Fact]
        public void Draws_PositiveQuantity_IsTrue()
        {
            var selection = new SelectiveSafetySelection { ElementId = "BOTA", Quantity = 2, Side = SafetySide.None };
            Assert.True(DynamicEditorSafety.Draws(selection));
        }

        [Fact]
        public void Draws_RackWideSide_IsTrue()
        {
            var selection = new SelectiveSafetySelection { ElementId = "BOTA", Quantity = 0, Side = SafetySide.Both };
            Assert.True(DynamicEditorSafety.Draws(selection));
        }

        [Fact]
        public void Draws_PerPostSide_IsTrue()
        {
            var selection = new SelectiveSafetySelection { ElementId = "BOTA", Quantity = 0, Side = SafetySide.None };
            selection.PostSides.Add(new SafetyPostSide { PostIndex = 1, Side = SafetySide.Left });
            Assert.True(DynamicEditorSafety.Draws(selection));
        }

        [Fact]
        public void CopyDrawable_KeepsOnlyDrawableElementBoundSelections_AsDeepCopies()
        {
            var source = new List<SelectiveSafetySelection>
            {
                new SelectiveSafetySelection { ElementId = "DRAWS", Quantity = 1, Side = SafetySide.None },       // kept
                new SelectiveSafetySelection { ElementId = "SILENT", Quantity = 0, Side = SafetySide.None },      // dropped: no draw
                new SelectiveSafetySelection { ElementId = "", Quantity = 3, Side = SafetySide.Both },            // dropped: empty id
                new SelectiveSafetySelection { ElementId = null, Quantity = 3, Side = SafetySide.Both }           // dropped: null id
            };
            var target = new List<SelectiveSafetySelection>
            {
                new SelectiveSafetySelection { ElementId = "STALE", Quantity = 9 }
            };

            DynamicEditorSafety.CopyDrawable(target, source);

            Assert.Single(target);
            Assert.Equal("DRAWS", target[0].ElementId);
            Assert.NotSame(source[0], target[0]); // deep copy, not the same reference
        }

        [Fact]
        public void CopyDrawable_NullSource_ClearsTarget()
        {
            var target = new List<SelectiveSafetySelection>
            {
                new SelectiveSafetySelection { ElementId = "X", Quantity = 1 }
            };

            DynamicEditorSafety.CopyDrawable(target, null);

            Assert.Empty(target);
        }
    }
}
