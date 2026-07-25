using System;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Headers;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// PB-010 (I-32), segunda mitad — la regla 12"/36" también tiene que recalcularse al REDUCIR.
    ///
    /// La primera corrida fijó el crecimiento (una orilla que pasa a intermedia sube a 36"). Falta la dirección
    /// contraria, que es la que deja una defensa larga colgada en un poste que ya es orilla, y hacerlo a través del
    /// ESTADO del editor —agregar y quitar frentes— y no sólo llamando al helper: es el camino por el que el
    /// usuario cambia el conteo de postes.
    /// </summary>
    public class PushBackDefenseResizeTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static string DefenseElementId(RackCatalog catalog)
            => catalog.SafetyElements
                .First(entry => SelectiveSafetyDefaults.IsType(entry.Type, SelectiveSafetyDefaults.DefensaType)).Id;

        // ---- Reduction, on the pure rule ----

        [Fact]
        public void AutoEnd_IsRecomputed_WhenAnIntermediatePostBecomesAnEdge()
        {
            var overrides = new[] { new SafetyPostDefense { PostIndex = 1, ExitAuto = true, ExitLength = 36.0 } };

            // 3 posts: post 1 is intermediate -> 36". Shrink to 2 posts: the same post is now an edge -> 12".
            Assert.Equal(36.0, DynamicForkliftDefensePlan.At(overrides, 1, 3).ExitLength, 6);
            Assert.Equal(12.0, DynamicForkliftDefensePlan.At(overrides, 1, 2).ExitLength, 6);
        }

        [Fact]
        public void ManualEnd_KeepsItsNumber_WhenThePostCountShrinks()
        {
            var overrides = new[] { new SafetyPostDefense { PostIndex = 1, ExitLength = 30.0 } };

            Assert.Equal(30.0, DynamicForkliftDefensePlan.At(overrides, 1, 3).ExitLength, 6);
            Assert.Equal(30.0, DynamicForkliftDefensePlan.At(overrides, 1, 2).ExitLength, 6);
        }

        [Fact]
        public void GrowingAndShrinkingBack_ReturnsToTheOriginalAutomaticLength()
        {
            var overrides = new[] { new SafetyPostDefense { PostIndex = 1, ExitAuto = true } };

            Assert.Equal(12.0, DynamicForkliftDefensePlan.At(overrides, 1, 2).ExitLength, 6);
            Assert.Equal(36.0, DynamicForkliftDefensePlan.At(overrides, 1, 4).ExitLength, 6);
            Assert.Equal(12.0, DynamicForkliftDefensePlan.At(overrides, 1, 2).ExitLength, 6);
        }

        // ---- Reduction, through the editor state (the real path) ----

        /// <summary>
        /// The rack the user actually drives: adding and removing fronts through <c>SetFrontCount</c> changes the post
        /// count, and the drawn defence has to follow. The LONGITUD is read from the piece the lateral plan emits, not
        /// from the helper, so this covers the whole chain state → design → resolve → plan.
        ///
        /// The selection carries a STORED record for post 1 — which is the defect's shape: a record used to freeze the
        /// length forever. Its exit end is Auto, so it must track the post count in BOTH directions.
        /// </summary>
        [Fact]
        public void EditorState_GrowsAndShrinksTheAutomaticDefense()
        {
            var catalog = Catalog;
            var defenseId = DefenseElementId(catalog);
            var assembler = new PushBackEditorDesignAssembler(catalog);
            var resolver = new PushBackResolver(catalog);

            var state = new PushBackEditorState();
            var inputs = state.LoadNew();
            var seeded = new SelectiveSafetySelection { ElementId = defenseId, Quantity = 1, Side = SafetySide.None };
            seeded.DefensaPosts.Add(new SafetyPostDefense { PostIndex = 1, ExitAuto = true, ExitLength = 12.0 });
            foreach (var selection in new PushBackSafetyAuthority(catalog).Authorize(new[] { seeded }))
            {
                inputs.SafetySelections.Add(selection);
            }

            double MiddlePostLength(int fronts)
            {
                state.SetFrontCount(fronts);
                var system = resolver.Resolve(assembler.BuildDesign(state, inputs));
                var lengths = new PushBackSystemLateralBuilder()
                    .Build(system, catalog, 1)                      // the section at post 1
                    .Flatten().Instances
                    .Where(i => string.Equals(i.PieceId, defenseId, StringComparison.OrdinalIgnoreCase))
                    .Select(i => i.DynamicParameters[SelectiveRackDefaults.LengthParam])
                    .Distinct()
                    .ToList();
                Assert.Single(lengths);
                return lengths[0];
            }

            // 1 front  => 2 posts: post 1 is an EDGE.
            Assert.Equal(12.0, MiddlePostLength(1), 6);
            // 3 fronts => 4 posts: the same post is now INTERMEDIATE.
            Assert.Equal(36.0, MiddlePostLength(3), 6);
            // ...and back down: the length has to come back, not stay at 36".
            Assert.Equal(12.0, MiddlePostLength(1), 6);
        }

        /// <summary>A length the user typed survives the same growing and shrinking untouched.</summary>
        [Fact]
        public void EditorState_KeepsAManualLength_AcrossGrowAndShrink()
        {
            var catalog = Catalog;
            var defenseId = DefenseElementId(catalog);
            var assembler = new PushBackEditorDesignAssembler(catalog);
            var resolver = new PushBackResolver(catalog);

            var state = new PushBackEditorState();
            var inputs = state.LoadNew();
            var manual = new SelectiveSafetySelection { ElementId = defenseId, Quantity = 1, Side = SafetySide.None };
            manual.DefensaPosts.Add(new SafetyPostDefense { PostIndex = 1, ExitLength = 30.0 });
            foreach (var selection in new PushBackSafetyAuthority(catalog).Authorize(new[] { manual }))
            {
                inputs.SafetySelections.Add(selection);
            }

            double MiddlePostLength(int fronts)
            {
                state.SetFrontCount(fronts);
                var system = resolver.Resolve(assembler.BuildDesign(state, inputs));
                return new PushBackSystemLateralBuilder()
                    .Build(system, catalog, 1)
                    .Flatten().Instances
                    .Where(i => string.Equals(i.PieceId, defenseId, StringComparison.OrdinalIgnoreCase))
                    .Select(i => i.DynamicParameters[SelectiveRackDefaults.LengthParam])
                    .Distinct()
                    .Single();
            }

            Assert.Equal(30.0, MiddlePostLength(1), 6);
            Assert.Equal(30.0, MiddlePostLength(3), 6);
            Assert.Equal(30.0, MiddlePostLength(1), 6);
        }
    }
}
