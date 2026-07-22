using RackCad.Application.Persistence;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// Covers the per-family safety configuration subtypes (I-22, E7): each config clones itself independently, the
    /// selection's DeepCopy delegates to every config (including DEFENSA, previously unasserted), the flat compatibility
    /// accessors stay in lockstep with the configs, and each family round-trips through the document with its exact
    /// legacy fallback preserved (the wire format is unchanged, so a legacy document loads to the same defaults it did
    /// before the decomposition).
    /// </summary>
    public class SelectiveSafetyConfigTests
    {
        // ---- Per-config DeepCopy independence ----

        [Fact]
        public void TopeConfig_DeepCopy_CopiesScalarsAndIsolatesOffCells()
        {
            var config = new SelectiveTopeConfig { Shared = false, Fondo = 2, Saque = 7.5, Frontal = true };
            config.OffCells.Add(new SelectiveGridCell { Frente = 1, Level = 3 });

            var copy = config.DeepCopy();
            Assert.False(copy.Shared);
            Assert.Equal(2, copy.Fondo);
            Assert.Equal(7.5, copy.Saque);
            Assert.True(copy.Frontal);
            Assert.Equal(1, Assert.Single(copy.OffCells).Frente);

            copy.OffCells.Clear();
            copy.Saque = 1.0;
            Assert.Single(config.OffCells);   // original untouched
            Assert.Equal(7.5, config.Saque);
        }

        [Fact]
        public void DesviadorConfig_DeepCopy_CopiesScalarsAndIsolatesOffCells()
        {
            var config = new SelectiveDesviadorConfig { Longitud = 22.0, PrimerNivelAltura = 20.0 };
            config.OffCells.Add(new SelectiveGridCell { Frente = 2, Level = 1 });

            var copy = config.DeepCopy();
            Assert.Equal(22.0, copy.Longitud);
            Assert.Equal(20.0, copy.PrimerNivelAltura);
            Assert.Single(copy.OffCells);

            copy.OffCells.Add(new SelectiveGridCell { Frente = 9, Level = 9 });
            Assert.Single(config.OffCells);
        }

        [Fact]
        public void ParrillaConfig_DeepCopy_CopiesScalarsAndIsolatesOffCells()
        {
            var config = new SelectiveParrillaConfig { Frontal = false, Lateral = true, Frente = 36.0, Cantidad = 2 };
            config.OffCells.Add(new SelectiveGridCell { Frente = 0, Level = 1 });

            var copy = config.DeepCopy();
            Assert.False(copy.Frontal);
            Assert.True(copy.Lateral);
            Assert.Equal(36.0, copy.Frente);
            Assert.Equal(2, copy.Cantidad);
            Assert.False(copy.At(0, 1));   // the off cell survives the copy
            Assert.True(copy.At(0, 0));

            copy.OffCells.Clear();
            Assert.Single(config.OffCells);
        }

        [Fact]
        public void GuiaConfig_DeepCopy_IsolatesOffCells()
        {
            var config = new SelectiveGuiaConfig();
            config.OffCells.Add(new SelectiveGridCell { Frente = 1, Level = 0 });

            var copy = config.DeepCopy();
            Assert.False(copy.At(1, 0));
            copy.OffCells.Clear();
            Assert.Single(config.OffCells);
        }

        [Fact]
        public void DefensaConfig_DeepCopy_IsolatesPosts()
        {
            var config = new SelectiveDefensaConfig();
            config.Posts.Add(new SafetyPostDefense { PostIndex = 1, ExitLength = 12.0, EntranceLength = 24.0 });

            var copy = config.DeepCopy();
            var post = Assert.Single(copy.Posts);
            Assert.Equal(1, post.PostIndex);
            Assert.Equal(12.0, post.ExitLength);
            Assert.Equal(24.0, post.EntranceLength);

            post.ExitLength = 99.0;                         // mutate the copy's post
            Assert.Equal(12.0, config.Posts[0].ExitLength); // original post untouched (distinct instance)
        }

        [Fact]
        public void SelectionDeepCopy_DelegatesToEveryConfig_IncludingDefensaAndPostSides()
        {
            var selection = new SelectiveSafetySelection { ElementId = "X", Quantity = 3, Side = SafetySide.Left };
            selection.PostSides.Add(new SafetyPostSide { PostIndex = 0, Side = SafetySide.Right });
            selection.Tope.OffCells.Add(new SelectiveGridCell { Frente = 1, Level = 1 });
            selection.Desviador.Longitud = 20.0;
            selection.Parrilla.Frente = 40.0;
            selection.Guia.OffCells.Add(new SelectiveGridCell { Frente = 0, Level = 0 });
            selection.Defensa.Posts.Add(new SafetyPostDefense { PostIndex = 2, ExitLength = 12.0, EntranceLength = 36.0 });

            var copy = selection.DeepCopy();

            // Config instances are distinct (no shared references).
            Assert.NotSame(selection.Tope, copy.Tope);
            Assert.NotSame(selection.Desviador, copy.Desviador);
            Assert.NotSame(selection.Parrilla, copy.Parrilla);
            Assert.NotSame(selection.Guia, copy.Guia);
            Assert.NotSame(selection.Defensa, copy.Defensa);

            // Mutating the copy leaves the original intact across every family.
            copy.Tope.OffCells.Clear();
            copy.Guia.OffCells.Clear();
            copy.Defensa.Posts.Clear();
            copy.PostSides.Clear();
            Assert.Single(selection.Tope.OffCells);
            Assert.Single(selection.Guia.OffCells);
            Assert.Single(selection.Defensa.Posts);   // the gap the legacy DeepCopy test never covered
            Assert.Single(selection.PostSides);
        }

        [Fact]
        public void ConfigSetter_NullCoalescesToAFreshConfig()
        {
            var selection = new SelectiveSafetySelection { Tope = null, Desviador = null, Parrilla = null, Defensa = null, Guia = null };
            Assert.NotNull(selection.Tope);
            Assert.True(selection.Tope.Shared);          // fresh default
            Assert.NotNull(selection.Desviador);
            Assert.NotNull(selection.Parrilla);
            Assert.NotNull(selection.Defensa);
            Assert.NotNull(selection.Guia);
        }

        [Fact]
        public void FlatAccessors_StayInLockstepWithTheConfigs()
        {
            var selection = new SelectiveSafetySelection();

            selection.TopeSaque = 9.0;                    // via the flat accessor
            Assert.Equal(9.0, selection.Tope.Saque);      // reflected in the config
            selection.Tope.Frontal = true;                // via the config
            Assert.True(selection.TopeFrontal);           // reflected in the flat accessor

            selection.ParrillaFrente = 50.0;
            Assert.Equal(50.0, selection.Parrilla.Frente);

            // The off-cell list is the SAME instance, so a legacy caller's .Add still mutates the config.
            Assert.Same(selection.Tope.OffCells, selection.TopeOffCells);
            selection.TopeOffCells.Add(new SelectiveGridCell { Frente = 2, Level = 2 });
            Assert.False(selection.Tope.At(2, 2));
        }

        // ---- Round-trip through the document (JSON) preserves every family's config ----

        [Fact]
        public void RoundTrip_EachFamilyConfig_PreservedThroughTheStore()
        {
            var design = new SelectivePalletDesign { PostId = "P", PostPeralte = 3.0 };
            design.Bays.Add(new SelectiveBayDesign());
            var selection = new SelectiveSafetySelection { ElementId = "E", Quantity = 4, Side = SafetySide.Right };
            selection.PostSides.Add(new SafetyPostSide { PostIndex = 1, Side = SafetySide.Left });
            selection.Tope.Shared = false;
            selection.Tope.Fondo = 1;
            selection.Tope.Saque = 6.0;
            selection.Tope.Frontal = true;
            selection.Tope.OffCells.Add(new SelectiveGridCell { Frente = 0, Level = 1 });
            selection.Desviador.Longitud = 24.0;
            selection.Desviador.PrimerNivelAltura = 20.0;
            selection.Desviador.OffCells.Add(new SelectiveGridCell { Frente = 2, Level = 0 });
            selection.Parrilla.Frontal = false;
            selection.Parrilla.Lateral = true;
            selection.Parrilla.Frente = 42.0;
            selection.Parrilla.Cantidad = 3;
            selection.Parrilla.OffCells.Add(new SelectiveGridCell { Frente = 1, Level = 0 });
            selection.Guia.OffCells.Add(new SelectiveGridCell { Frente = 0, Level = 2 });
            selection.Defensa.Posts.Add(new SafetyPostDefense { PostIndex = 0, ExitLength = 12.0, EntranceLength = 36.0 });
            design.SafetySelections.Add(selection);

            var store = new SelectivePalletDesignStore();
            var restored = store.Deserialize(store.Serialize(SelectivePalletDesignDocument.From(design, "id", "R"))).ToDomain();
            var r = Assert.Single(restored.SafetySelections);

            Assert.Equal(SafetySide.Right, r.Side);
            Assert.Equal(SafetySide.Left, r.SideForPost(1));
            Assert.False(r.Tope.Shared);
            Assert.Equal(1, r.Tope.Fondo);
            Assert.Equal(6.0, r.Tope.Saque);
            Assert.True(r.Tope.Frontal);
            Assert.False(r.Tope.At(0, 1));
            Assert.Equal(24.0, r.Desviador.Longitud);
            Assert.Equal(20.0, r.Desviador.PrimerNivelAltura);
            Assert.False(r.Desviador.At(2, 0));
            Assert.False(r.Parrilla.Frontal);
            Assert.True(r.Parrilla.Lateral);
            Assert.Equal(42.0, r.Parrilla.Frente);
            Assert.Equal(3, r.Parrilla.Cantidad);
            Assert.False(r.Parrilla.At(1, 0));
            Assert.False(r.Guia.At(0, 2));
            var defense = Assert.Single(r.Defensa.Posts);
            Assert.Equal(12.0, defense.ExitLength);
            Assert.Equal(36.0, defense.EntranceLength);
        }

        // ---- Legacy documents: a missing family field loads to the SAME default as before the decomposition ----

        [Fact]
        public void LegacyDocument_MissingEveryFamilyField_LoadsSubtypeDefaults()
        {
            var restored = new SafetySelectionDocument { ElementId = "E", Quantity = 1 }.ToDomain();

            Assert.Equal(SafetySide.Both, restored.Side);              // null side -> Both
            Assert.True(restored.Tope.Shared);
            Assert.Equal(-1, restored.Tope.Fondo);
            Assert.Equal(SelectiveSafetyDefaults.TopeSaque, restored.Tope.Saque);
            Assert.False(restored.Tope.Frontal);
            Assert.Empty(restored.Tope.OffCells);
            Assert.Equal(SelectiveSafetyDefaults.DesviadorLongitud, restored.Desviador.Longitud);
            Assert.Equal(SelectiveSafetyDefaults.DesviadorPrimerNivelAltura, restored.Desviador.PrimerNivelAltura);
            Assert.True(restored.Parrilla.Frontal);
            Assert.True(restored.Parrilla.Lateral);
            Assert.Equal(0.0, restored.Parrilla.Frente);
            Assert.Equal(0, restored.Parrilla.Cantidad);
            Assert.Empty(restored.Defensa.Posts);
            Assert.Empty(restored.Guia.OffCells);
        }

        [Fact]
        public void LegacyDocument_NonPositiveGuardedNumerics_FallBackToDomainDefault()
        {
            // Tope.Saque, Desviador.Longitud and Desviador.PrimerNivelAltura keep the "<= 0 OR null -> default" guard.
            var restored = new SafetySelectionDocument
            {
                ElementId = "E", Quantity = 1, TopeSaque = 0.0, DesviadorLongitud = 0.0, DesviadorPrimerNivelAltura = -5.0
            }.ToDomain();

            Assert.Equal(SelectiveSafetyDefaults.TopeSaque, restored.Tope.Saque);
            Assert.Equal(SelectiveSafetyDefaults.DesviadorLongitud, restored.Desviador.Longitud);
            Assert.Equal(SelectiveSafetyDefaults.DesviadorPrimerNivelAltura, restored.Desviador.PrimerNivelAltura);
        }

        [Fact]
        public void LegacyDocument_PresentZeroParrillaFrente_IsPreserved_UnlikeTheGuardedFamilies()
        {
            // Parrilla.Frente uses a plain "?? 0.0": a present 0 stays 0 (the editor means "auto"), which is the pre-
            // decomposition behavior and distinct from the guarded Tope/Desviador numerics above.
            var restored = new SafetySelectionDocument { ElementId = "E", Quantity = 1, ParrillaFrente = 0.0, ParrillaCantidad = 0 }.ToDomain();
            Assert.Equal(0.0, restored.Parrilla.Frente);
            Assert.Equal(0, restored.Parrilla.Cantidad);
        }
    }
}
