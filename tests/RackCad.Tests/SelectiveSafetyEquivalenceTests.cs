using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using RackCad.Application.Catalogs;
using RackCad.Application.Headers;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// GOLDEN characterization of the SELECTIVE safety subsystem (I-22). Freezes the FULL resolved output —
    /// every Safety/Tope/Separator/Pallet instance across frontal/lateral/planta plus the safety BOM — for
    /// representative scenarios, so the E6/E7 refactor (per-family placement services, subtype configs, a
    /// single troquel source, SelectionMatrix adoption) cannot change what is drawn or quoted. If any resolved
    /// coordinate, block, mirror flag, dynamic parameter or BOM quantity/length shifts, the matching baseline
    /// (tests/RackCad.Tests/Golden/*.txt) breaks first. Designs are built through the CURRENT public API
    /// (preserved by the refactor), so the snapshots are a true before/after lock. The baselines were captured
    /// from the pre-refactor code; regenerate them only with an explicit, reviewed reason.
    /// </summary>
    public class SelectiveSafetyEquivalenceTests
    {
        private const string PostId = TestCatalogIds.Profiles.Posts.Standard;
        private const string BeamId = TestCatalogIds.Profiles.Beams.SelectiveThreeRivet;
        private const string BotaId = TestCatalogIds.Safety.Boots.H3_16_18;
        private const string LateralId = TestCatalogIds.Safety.SideProtectors.H3_16_18;
        private const string TopeId = TestCatalogIds.Safety.Stops.Post;
        private const string ParrillaId = TestCatalogIds.Safety.Decks.Generic;
        private const string DesviadorId = TestCatalogIds.Safety.Deviators.A4;

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        // ---- Scenario 1: larguero topes across two fondos, per-fondo + off cell + custom saque + frontal ----
        private static SelectivePalletDesign TopeScenario()
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId, PostPeralte = 3.0, PalletTolerance = 4.0, VerticalClearance = 6.0,
                PalletDepth = 48.0, DepthCount = 2
            };
            design.Bays.Add(Bay(2));
            design.Bays.Add(Bay(3));
            design.ExtraFondoBays.Add(new List<SelectiveBayDesign> { Bay(2), Bay(2) });
            var tope = new SelectiveSafetySelection
            {
                ElementId = TopeId, Side = SafetySide.Both, TopeShared = false, TopeFondo = 0,
                TopeSaque = 5.0, TopeFrontal = true
            };
            tope.TopeOffCells.Add(new SelectiveGridCell { Frente = 1, Level = 1 });
            design.SafetySelections.Add(tope);
            return design;
        }

        // ---- Scenario 2: parrilla decks frontal + lateral, off cell, manual frente ----
        private static SelectivePalletDesign ParrillaScenario()
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId, PostPeralte = 3.0, PalletTolerance = 4.0, VerticalClearance = 6.0, PalletDepth = 48.0
            };
            design.Bays.Add(Bay(2, floorBeam: true));
            design.Bays.Add(Bay(2, floorBeam: true));
            var sel = new SelectiveSafetySelection
            {
                ElementId = ParrillaId, Side = SafetySide.Both, Quantity = 1,
                ParrillaFrontal = true, ParrillaLateral = true, ParrillaFrente = 50.0
            };
            sel.ParrillaOffCells.Add(new SelectiveGridCell { Frente = 0, Level = 0 });
            design.SafetySelections.Add(sel);
            return design;
        }

        // ---- Scenario 3: tarima (pallet) visual reference frontal + lateral ----
        private static SelectivePalletDesign TarimaScenario()
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId, PostPeralte = 3.0, PalletTolerance = 4.0, VerticalClearance = 6.0,
                PalletDepth = 48.0, DrawPallets = true
            };
            design.Bays.Add(Bay(2, floorBeam: false));
            design.Bays.Add(Bay(2, floorBeam: true));
            return design;
        }

        // ---- Scenario 4: separators across three fondos ----
        private static SelectivePalletDesign SeparadorScenario()
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId, PostPeralte = 3.0, PalletTolerance = 4.0, VerticalClearance = 6.0,
                PalletDepth = 48.0, DepthCount = 3
            };
            design.SeparatorLengths.Add(12.0);
            design.SeparatorLengths.Add(18.0);
            design.Bays.Add(Bay(2));
            design.Bays.Add(Bay(2));
            design.ExtraFondoBays.Add(new List<SelectiveBayDesign> { Bay(2), Bay(2) });
            design.ExtraFondoBays.Add(new List<SelectiveBayDesign> { Bay(2), Bay(2) });
            return design;
        }

        // ---- Scenario 5: the exhaustive catch-all — every refactored family at once, two fondos ----
        private static SelectivePalletDesign CombinedScenario()
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId, PostPeralte = 3.0, PalletTolerance = 4.0, VerticalClearance = 6.0,
                PalletDepth = 48.0, DepthCount = 2, DrawPallets = true
            };
            design.SeparatorLengths.Add(12.0);
            design.Bays.Add(Bay(2, floorBeam: true));
            design.Bays.Add(Bay(2, floorBeam: true));
            design.ExtraFondoBays.Add(new List<SelectiveBayDesign> { Bay(2, floorBeam: true), Bay(2, floorBeam: true) });

            design.SafetySelections.Add(new SelectiveSafetySelection { ElementId = BotaId, Side = SafetySide.Both });
            var lateral = new SelectiveSafetySelection { ElementId = LateralId, Side = SafetySide.None };
            lateral.PostSides.Add(new SafetyPostSide { PostIndex = 0, Side = SafetySide.Left });
            design.SafetySelections.Add(lateral);
            design.SafetySelections.Add(new SelectiveSafetySelection
            {
                ElementId = TopeId, Side = SafetySide.Both, TopeShared = true, TopeFrontal = true
            });
            design.SafetySelections.Add(new SelectiveSafetySelection
            {
                ElementId = ParrillaId, Side = SafetySide.Both, Quantity = 1,
                ParrillaFrontal = true, ParrillaLateral = true
            });
            design.SafetySelections.Add(new SelectiveSafetySelection
            {
                ElementId = DesviadorId, Side = SafetySide.Both,
                DesviadorLongitud = 18.0, DesviadorPrimerNivelAltura = 18.0
            });
            return design;
        }

        private static SelectiveBayDesign Bay(int levels, bool floorBeam = false)
        {
            var bay = new SelectiveBayDesign { FloorBeam = floorBeam };
            for (var l = 0; l < levels; l++)
            {
                bay.Levels.Add(new SelectiveCell
                {
                    Pallet = new Tarima { Frente = 40.0, Alto = 45.0 + l * 5.0 },
                    PalletCount = 2, BeamId = BeamId, BeamPeralte = 4.0
                });
            }

            return bay;
        }

        private static SelectiveBayDesign MedioBay(int levels, params (double Length, bool Loaded)[] tramos)
        {
            var bay = new SelectiveBayDesign { FloorBeam = true };
            foreach (var (length, loaded) in tramos) bay.Segments.Add(new SelectiveSegment { Length = length, Loaded = loaded });
            for (var l = 0; l < levels; l++)
            {
                bay.Levels.Add(new SelectiveCell
                {
                    Pallet = new Tarima { Frente = 40.0, Alto = 45.0 + l * 5.0 },
                    PalletCount = 2, BeamId = BeamId, BeamPeralte = 4.0
                });
            }

            return bay;
        }

        // ---- Scenario 6: medio frente (tramos) across two fondos — topes + parrilla follow the loaded tramos ----
        private static SelectivePalletDesign MedioFrenteScenario()
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId, PostPeralte = 3.0, PalletTolerance = 4.0, VerticalClearance = 6.0,
                PalletDepth = 48.0, DepthCount = 2
            };
            design.SeparatorLengths.Add(12.0);
            design.Bays.Add(Bay(2, floorBeam: true));
            design.Bays.Add(MedioBay(2, (40.0, true), (0.0, true)));                          // fondo 0: 2 tramos
            design.ExtraFondoBays.Add(new List<SelectiveBayDesign>
            {
                Bay(2, floorBeam: true),
                MedioBay(2, (36.0, true), (30.0, false), (0.0, true))                          // fondo 1: 3 tramos, middle empty
            });
            design.SafetySelections.Add(new SelectiveSafetySelection { ElementId = TopeId, Side = SafetySide.Both, TopeShared = true, TopeFrontal = true });
            design.SafetySelections.Add(new SelectiveSafetySelection { ElementId = ParrillaId, Side = SafetySide.Both, Quantity = 1, ParrillaFrontal = true, ParrillaLateral = true });
            return design;
        }

        // ---- Scenario 7: quadruple depth — bota (system front/back), per-fondo tope (central pair), 3 separator gaps ----
        private static SelectivePalletDesign CuadrupleProfundidadScenario()
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId, PostPeralte = 3.0, PalletTolerance = 4.0, VerticalClearance = 6.0,
                PalletDepth = 48.0, DepthCount = 4
            };
            design.SeparatorLengths.Add(12.0);
            design.SeparatorLengths.Add(10.0);
            design.SeparatorLengths.Add(14.0);
            design.Bays.Add(Bay(2, floorBeam: true));
            design.Bays.Add(Bay(2, floorBeam: true));
            design.ExtraFondoBays.Add(new List<SelectiveBayDesign> { Bay(2, floorBeam: true), Bay(2, floorBeam: true) });
            design.ExtraFondoBays.Add(new List<SelectiveBayDesign> { Bay(2, floorBeam: true), Bay(2, floorBeam: true) });
            design.ExtraFondoBays.Add(new List<SelectiveBayDesign> { Bay(2, floorBeam: true), Bay(2, floorBeam: true) });
            design.SafetySelections.Add(new SelectiveSafetySelection { ElementId = BotaId, Side = SafetySide.Both });
            design.SafetySelections.Add(new SelectiveSafetySelection { ElementId = TopeId, Side = SafetySide.Both, TopeShared = false, TopeFrontal = true });
            return design;
        }

        [Fact]
        public void Tope_MultiFondo_ResolvedOutputIsFrozen() => Assert.Equal(Baseline("tope"), Snapshot(TopeScenario()));

        [Fact]
        public void Parrilla_FrontalLateral_ResolvedOutputIsFrozen() => Assert.Equal(Baseline("parrilla"), Snapshot(ParrillaScenario()));

        [Fact]
        public void Tarima_VisualReference_ResolvedOutputIsFrozen() => Assert.Equal(Baseline("tarima"), Snapshot(TarimaScenario()));

        [Fact]
        public void Separador_MultiFondo_ResolvedOutputIsFrozen() => Assert.Equal(Baseline("separador"), Snapshot(SeparadorScenario()));

        [Fact]
        public void Combined_AllFamilies_ResolvedOutputIsFrozen() => Assert.Equal(Baseline("combined"), Snapshot(CombinedScenario()));

        /// <summary>The full resolved safety output as canonical, sorted lines: one per Safety/Tope/Separator/Pallet
        /// instance across frontal/lateral/planta, then the safety BOM components.</summary>
        private static string[] Snapshot(SelectivePalletDesign design)
        {
            var catalog = Catalog;
            var system = new SelectiveGeometryResolver().Resolve(design, catalog);
            var lines = new List<string>();

            var frontal = new SelectiveFrontalBuilder().Build(SelectiveDepthLayout.FondoSystemView(system, 0), catalog);
            foreach (var i in SafetyLike(frontal)) lines.Add(Key("FRONTAL", -1, i));

            var cortes = new SelectiveLateralBuilder().Cortes(system, catalog);
            for (var c = 0; c < cortes.Count; c++)
            {
                foreach (var i in SafetyLike(cortes[c].Largueros)) lines.Add(Key("LATERAL", c, i));
            }

            var planta = new SelectivePlantaBuilder().Build(system, catalog);
            foreach (var i in SafetyLike(planta)) lines.Add(Key("PLANTA", -1, i));

            lines.Sort(StringComparer.Ordinal);

            var bomLines = SelectiveBomBuilder.Build(system, catalog).Components
                .Where(c => IsSafetyCategory(c.Category))
                .Select(c => string.Format(CultureInfo.InvariantCulture, "BOM|{0}|{1}|{2}|{3:0.###}", c.Category, c.ProfileId, c.Quantity, c.Length))
                .OrderBy(s => s, StringComparer.Ordinal);

            return lines.Concat(bomLines).ToArray();
        }

        /// <summary>Like <see cref="Snapshot"/> but for MULTIFONDO scenarios: captures the frontal of EVERY applicable
        /// fondo (each labeled FRONTAL&lt;k&gt;), not only fondo 0, plus the lateral cortes, planta and safety BOM.</summary>
        private static string[] SnapshotPerFondo(SelectivePalletDesign design)
        {
            var catalog = Catalog;
            var system = new SelectiveGeometryResolver().Resolve(design, catalog);
            var lines = new List<string>();

            var fondoCount = SelectiveDepthLayout.Count(system);
            for (var k = 0; k < fondoCount; k++)
            {
                var frontal = new SelectiveFrontalBuilder().Build(SelectiveDepthLayout.FondoSystemView(system, k), catalog);
                foreach (var i in SafetyLike(frontal)) lines.Add(Key("FRONTAL" + k.ToString(CultureInfo.InvariantCulture), -1, i));
            }

            var cortes = new SelectiveLateralBuilder().Cortes(system, catalog);
            for (var c = 0; c < cortes.Count; c++)
            {
                foreach (var i in SafetyLike(cortes[c].Largueros)) lines.Add(Key("LATERAL", c, i));
            }

            var planta = new SelectivePlantaBuilder().Build(system, catalog);
            foreach (var i in SafetyLike(planta)) lines.Add(Key("PLANTA", -1, i));

            lines.Sort(StringComparer.Ordinal);

            var bomLines = SelectiveBomBuilder.Build(system, catalog).Components
                .Where(c => IsSafetyCategory(c.Category))
                .Select(c => string.Format(CultureInfo.InvariantCulture, "BOM|{0}|{1}|{2}|{3:0.###}", c.Category, c.ProfileId, c.Quantity, c.Length))
                .OrderBy(s => s, StringComparer.Ordinal);

            return lines.Concat(bomLines).ToArray();
        }

        [Fact]
        public void MedioFrente_TramosMultiFondo_ResolvedOutputIsFrozen()
            => Assert.Equal(Baseline("medio_frente"), SnapshotPerFondo(MedioFrenteScenario()));

        [Fact]
        public void CuadrupleProfundidad_PerFondoFrontals_ResolvedOutputIsFrozen()
            => Assert.Equal(Baseline("cuadruple"), SnapshotPerFondo(CuadrupleProfundidadScenario()));

        private static IEnumerable<HeaderBlockInstance> SafetyLike(IEnumerable<HeaderBlockInstance> instances)
            => instances.Where(i => i.Role == HeaderBlockRole.Safety
                                    || i.Role == HeaderBlockRole.Tope
                                    || i.Role == HeaderBlockRole.Separator
                                    || i.Role == HeaderBlockRole.Pallet);

        private static string Key(string view, int corte, HeaderBlockInstance i)
        {
            var pars = string.Join(";", i.DynamicParameters
                .OrderBy(k => k.Key, StringComparer.Ordinal)
                .Select(k => k.Key + "=" + k.Value.ToString("0.###", CultureInfo.InvariantCulture)));
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}{1}|{2}|{3}|{4}|{5:0.###}|{6:0.###}|{7}|{8}|{9:0.####}|{10}",
                view, corte >= 0 ? "#" + corte.ToString(CultureInfo.InvariantCulture) : string.Empty,
                (int)i.Role, i.PieceId, i.BlockName,
                i.Insertion.X, i.Insertion.Y, i.MirroredX ? 1 : 0, i.MirroredY ? 1 : 0, i.RotationRadians, pars);
        }

        private static bool IsSafetyCategory(string category)
            => category == SelectiveBomBuilder.Safety
               || category == SelectiveBomBuilder.Separador
               || category == SelectiveBomBuilder.Tope
               || category == SelectiveBomBuilder.Parrilla;

        /// <summary>Reads the committed golden baseline next to this source file. Line-based, so CRLF/LF
        /// normalization is irrelevant; a per-line diff pinpoints any drift.</summary>
        private static string[] Baseline(string name, [CallerFilePath] string thisFile = "")
            => File.ReadAllLines(Path.Combine(Path.GetDirectoryName(thisFile) ?? ".", "Golden", name + ".txt"));
    }
}
