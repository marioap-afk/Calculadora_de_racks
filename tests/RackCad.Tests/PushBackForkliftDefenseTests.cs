using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// PB-009 / PB-010 (I-32) — the forklift defence of a Push Back rack.
    ///
    /// PB-009: Push Back only carries safety at the LOW (entrance/exit) end. Zeroing the stored records was not enough,
    /// because a brand-new rack has NO records: the 12/36 default fell straight through and a defence was drawn at the
    /// rear in the lateral, in the planta and in the BOM. The selection now carries the rule itself, and the adaptive
    /// lateral-guard default stops putting a guard on the last post's far face too.
    ///
    /// PB-010: the 12"/36" rule depends on the post being an edge or an intermediate one, which changes when fronts are
    /// added. A stored record used to freeze the length forever. An end marked Auto is recomputed from the CURRENT post
    /// count; an end the user typed keeps its number.
    /// </summary>
    public class PushBackForkliftDefenseTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static string DefenseElementId(RackCatalog catalog)
            => catalog.SafetyElements
                .First(entry => SelectiveSafetyDefaults.IsType(entry.Type, SelectiveSafetyDefaults.DefensaType)).Id;

        private static DynamicRackDesign Structure(int fronts)
        {
            var design = new DynamicRackDesign
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = 4,
                LoadLevels = 2,
                FirstLevelHeight = 6.0,
                BeamDepth = 4.0
            };
            for (var i = 0; i < fronts; i++)
            {
                design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 2, PalletsDeep = 4, DepthStartPosition = 1 });
            }

            return design;
        }

        // ---- PB-010: Auto per end recomputes; an override does not ----

        [Fact]
        public void AutoEnd_IsRecomputed_WhenAnEdgePostBecomesIntermediate()
        {
            var post = new SafetyPostDefense { PostIndex = 1, ExitAuto = true, ExitLength = 12.0 };
            var overrides = new[] { post };

            // 2 posts: post 1 is an edge -> 12". 3 posts: the same post is now intermediate -> 36".
            Assert.Equal(12.0, DynamicForkliftDefensePlan.At(overrides, 1, 2).ExitLength, 6);
            Assert.Equal(36.0, DynamicForkliftDefensePlan.At(overrides, 1, 3).ExitLength, 6);
        }

        [Fact]
        public void ManualEnd_KeepsItsNumber_WhenThePostCountChanges()
        {
            var overrides = new[] { new SafetyPostDefense { PostIndex = 1, ExitLength = 18.0 } };

            Assert.Equal(18.0, DynamicForkliftDefensePlan.At(overrides, 1, 2).ExitLength, 6);
            Assert.Equal(18.0, DynamicForkliftDefensePlan.At(overrides, 1, 3).ExitLength, 6);
        }

        [Fact]
        public void LegacyRecordWithoutFlags_BehavesExactlyAsBefore()
        {
            // Every document written before PB-010 carries neither flag: both ends are explicit overrides.
            var overrides = new[] { new SafetyPostDefense { PostIndex = 0, ExitLength = 12.0, EntranceLength = 24.0 } };
            var setting = DynamicForkliftDefensePlan.At(overrides, 0, 4);

            Assert.Equal(12.0, setting.ExitLength, 6);
            Assert.Equal(24.0, setting.EntranceLength, 6);
        }

        [Fact]
        public void AutoFlags_SurviveTheDocumentRoundTripAndTheDeepCopy()
        {
            var config = new SelectiveDefensaConfig();
            config.Posts.Add(new SafetyPostDefense { PostIndex = 2, ExitAuto = true, EntranceLength = 20.0 });

            var copy = config.DeepCopy();
            Assert.True(copy.Posts[0].ExitAuto);
            Assert.False(copy.Posts[0].EntranceAuto);

            var restored = DefensaSelectionDocument.From(config).ToDomain();
            Assert.True(restored.Posts[0].ExitAuto);
            Assert.False(restored.Posts[0].EntranceAuto);
            Assert.Equal(20.0, restored.Posts[0].EntranceLength, 6);
        }

        [Fact]
        public void ARecordWithNoAutoFlag_IsNotWrittenAsAutoInTheDocument()
        {
            var config = new SelectiveDefensaConfig();
            config.Posts.Add(new SafetyPostDefense { PostIndex = 0, ExitLength = 12.0, EntranceLength = 12.0 });

            var document = DefensaSelectionDocument.From(config);
            Assert.Null(document.Posts[0].ExitAuto);
            Assert.Null(document.Posts[0].EntranceAuto);
        }

        // ---- PB-009: nothing at the far end by default ----

        [Fact]
        public void LowEndOnlySelection_HasNoAutomaticLengthAtTheFarEnd()
        {
            var selection = new SelectiveSafetySelection { ElementId = "X", LowEndOnly = true };
            var setting = DynamicForkliftDefensePlan.ForSelection(selection, 1, 4);

            Assert.Equal(36.0, setting.ExitLength, 6);
            Assert.True(setting.DrawsExit);
            Assert.Equal(0.0, setting.EntranceLength, 6);
            Assert.False(setting.DrawsEntrance);
        }

        [Fact]
        public void LowEndOnly_StillHonoursAnExplicitFarEndOverride()
        {
            // "Off by default" is a DEFAULT, not a prohibition: a length the user typed is drawn.
            var selection = new SelectiveSafetySelection { ElementId = "X", LowEndOnly = true };
            selection.DefensaPosts.Add(new SafetyPostDefense { PostIndex = 1, ExitLength = 36.0, EntranceLength = 24.0 });

            var setting = DynamicForkliftDefensePlan.ForSelection(selection, 1, 4);
            Assert.Equal(24.0, setting.EntranceLength, 6);
            Assert.True(setting.DrawsEntrance);
        }

        [Fact]
        public void Authority_MarksTheSelection_SoADefaultRackDrawsNothingAtTheRear()
        {
            var catalog = Catalog;
            var authority = new PushBackSafetyAuthority(catalog);

            var authorized = authority.Defaults();
            Assert.NotEmpty(authorized);
            Assert.All(authorized, selection => Assert.True(selection.LowEndOnly));
        }

        /// <summary>
        /// The end-to-end shape of PB-009: a rack built with the DEFAULT safety a new Push Back opens with must place
        /// no defence beyond the low end, in the lateral, in the planta or in the BOM.
        /// </summary>
        [Fact]
        public void DefaultPushBackRack_DrawsNoDefenseAtTheRear()
        {
            var catalog = Catalog;
            var design = new PushBackDesign { Structure = Structure(2) };
            foreach (var selection in new PushBackSafetyAuthority(catalog).Defaults())
            {
                design.Structure.SafetySelections.Add(selection);
            }

            var system = new PushBackResolver(catalog).Resolve(design);
            var defenseId = DefenseElementId(catalog);
            var totalLength = system.Structure.TotalLength;

            var lateral = new PushBackSystemLateralBuilder().Build(system, catalog).Flatten().Instances;
            var planta = new PushBackSystemPlantaBuilder().BuildPlan(system, catalog).Flatten().Instances;

            var lateralDefenses = lateral.Where(i => string.Equals(i.PieceId, defenseId, StringComparison.OrdinalIgnoreCase)).ToList();
            var plantaDefenses = planta.Where(i => string.Equals(i.PieceId, defenseId, StringComparison.OrdinalIgnoreCase)).ToList();

            // Everything drawn sits in the LOW half of the rack; nothing beyond the far end.
            Assert.All(lateralDefenses, instance => Assert.True(
                instance.Insertion.X < totalLength / 2.0,
                FormattableString.Invariant($"a defence was drawn at X={instance.Insertion.X:0.##} on a {totalLength:0.##}\" rack")));
            Assert.All(plantaDefenses, instance => Assert.False(
                instance.MirroredX,
                "a mirrored (far-end) defence was drawn in the planta"));
        }

        /// <summary>
        /// La otra mitad de PB-009, CORREGIDA en el round 2 de owner-validation.
        ///
        /// Esta prueba fijaba que el protector adaptativo del ÚLTIMO poste desapareciera en un sistema de extremo
        /// bajo — y eso era justamente el defecto. Interpretaba el <c>Right</c> de la regla adaptativa como
        /// «extremo posterior» cuando ahí significa <b>orientación</b>. Un rack lleva SIEMPRE los protectores de
        /// sus dos postes extremos; en Push Back los dos van delante y lo que distingue al último es el espejo.
        ///
        /// Lo que sí sigue restringido al extremo bajo es la DEFENSA de montacargas, el otro elemento de PB-009,
        /// que comprueban las pruebas de arriba y que no cambia.
        /// </summary>
        [Fact]
        public void LowEndOnly_KeepsTheAdaptiveLateralGuard_MirroredInsteadOfGone()
        {
            var lowEnd = new SelectiveSafetySelection { ElementId = "X", Side = SafetySide.None, LowEndOnly = true };
            var ordinary = new SelectiveSafetySelection { ElementId = "X", Side = SafetySide.None };

            // El último poste conserva su protector: ese Right es su ORIENTACIÓN, no un extremo.
            Assert.Equal(SafetySide.Right, DynamicLateralGuardPlan.SideAt(lowEnd, 2, 3));
            Assert.Equal(SafetySide.Left, DynamicLateralGuardPlan.SideAt(lowEnd, 0, 3));
            Assert.Equal(SafetySide.None, DynamicLateralGuardPlan.SideAt(lowEnd, 1, 3));

            // El EXTREMO lo deciden las copias físicas: en un sistema de extremo bajo, la del último se queda
            // delante y solo cambia de cara.
            var lastLowEnd = Assert.Single(DynamicLateralGuardPlan.CopiesAt(lowEnd, 2, 3));
            Assert.False(lastLowEnd.AtHighEnd);
            Assert.True(lastLowEnd.Mirrored);

            // The dynamic system keeps its own adaptive rule, far face included.
            Assert.Equal(SafetySide.Right, DynamicLateralGuardPlan.SideAt(ordinary, 2, 3));
            Assert.Equal(SafetySide.Left, DynamicLateralGuardPlan.SideAt(ordinary, 0, 3));
            var lastOrdinary = Assert.Single(DynamicLateralGuardPlan.CopiesAt(ordinary, 2, 3));
            Assert.True(lastOrdinary.AtHighEnd);
            Assert.True(lastOrdinary.Mirrored);
        }

        // ---- Isolation: the dynamic system is untouched ----

        [Fact]
        public void OrdinarySelection_KeepsTheSymmetricAutomaticDefault()
        {
            var selection = new SelectiveSafetySelection { ElementId = "X" };
            var edge = DynamicForkliftDefensePlan.ForSelection(selection, 0, 4);
            var middle = DynamicForkliftDefensePlan.ForSelection(selection, 1, 4);

            Assert.Equal(12.0, edge.ExitLength, 6);
            Assert.Equal(12.0, edge.EntranceLength, 6);
            Assert.Equal(36.0, middle.ExitLength, 6);
            Assert.Equal(36.0, middle.EntranceLength, 6);
        }

        [Fact]
        public void LowEndOnly_IsCarriedByTheDeepCopy_AndReimposedByTheAuthority()
        {
            var selection = new SelectiveSafetySelection { ElementId = "X", LowEndOnly = true };
            Assert.True(selection.DeepCopy().LowEndOnly);

            // It is DERIVED, not persisted: whatever a caller hands over, the authority sets it, so no stored value
            // can ever go stale and no DTO field is needed.
            var fromDisk = new SelectiveSafetySelection { ElementId = "PROTECTOR_BOTA_H_3_16_18" };
            Assert.False(fromDisk.LowEndOnly);
            var authorized = new PushBackSafetyAuthority(Catalog).Authorize(new[] { fromDisk });
            Assert.All(authorized, item => Assert.True(item.LowEndOnly));
            Assert.False(fromDisk.LowEndOnly);   // the source is never mutated
        }
    }
}
