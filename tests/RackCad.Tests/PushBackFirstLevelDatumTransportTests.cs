using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.Selective;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 (corrección aislada 3) — el DATUM de «Alto 1er nivel» se TRANSPORTA, no se vuelve a decidir.
    ///
    /// <para>
    /// La decisión del dueño ya está cerrada: LOW es la autoridad vertical y <c>0"</c> significa el troquel
    /// físicamente utilizable más bajo. Lo que estas pruebas fijan es que esa lectura viaja intacta por persistencia
    /// → ventana → estado → ensamblador → estructura compuesta → resolver → elevaciones, y que la ÚNICA conversión
    /// del producto —un documento sin marcador re-expresado sobre el datum nuevo— ocurre en una sola frontera y no
    /// mueve ni una milésima.
    /// </para>
    /// <para>
    /// Los valores son IMPARES a propósito (5", 7"): con valores pares las dos lecturas caen en el mismo troquel y
    /// una prueba puede pasar sin comprobar nada.
    /// </para>
    /// </summary>
    public class PushBackFirstLevelDatumTransportTests
    {
        private const int NewDatum = (int)RackFirstLevelDatumMode.LowestUsablePunch;

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static PushBackEditorInputs Inputs()
        {
            var inputs = PushBackEditorInputs.NewDesign();
            inputs.Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg");
            foreach (var selection in new PushBackSafetyAuthority(Catalog).Defaults())
            {
                inputs.SafetySelections.Add(selection);
            }

            return inputs;
        }

        // ---- la retícula real del poste, medida como la mide el producto ----------------------------------------

        private static double GridBase(DynamicRackSystem structure) => PushBackTroquelGrid.Base(structure, Catalog);

        private static double Pitch => SelectiveRackDefaults.TroquelPaso;

        private static double LowestPunch(DynamicRackSystem structure)
            => RackFirstLevelDatum.LowestUsablePunch(GridBase(structure), Pitch);

        /// <summary>El ÍNDICE del troquel que ocupa una elevación, contando desde el utilizable más bajo.</summary>
        private static int PunchIndex(DynamicRackSystem structure, double elevation)
        {
            var offset = elevation - LowestPunch(structure);
            var index = (int)Math.Round(offset / Pitch, MidpointRounding.AwayFromZero);
            Assert.True(
                Math.Abs(offset - index * Pitch) < 1e-6,
                $"la elevación {elevation:0.####} no cae en un troquel de la retícula (base {GridBase(structure):0.####}, paso {Pitch})");
            return index;
        }

        // ---- lecturas físicas ------------------------------------------------------------------------------------

        private static double LowZ(PushBackSystem system, int front = 0, int level = 1)
        {
            var target = system.Structure.Fronts[front];
            return Math.Round(PushBackElevations.LowInsertions(system, Catalog, target)[level], 4);
        }

        /// <summary>La Y del larguero de entrada en el corte LATERAL: el oráculo que el dueño validó.</summary>
        private static IReadOnlyList<double> LateralLow(PushBackSystem system)
            => new PushBackSystemLateralBuilder().Build(system, Catalog).Flatten().Instances
                .Where(instance => instance.Role == HeaderBlockRole.Beam
                                   && instance.PieceId == DynamicRackDefaults.InOutBeamCatalogId)
                .Select(instance => Math.Round(instance.Insertion.Y, 4))
                .Distinct()
                .OrderBy(y => y)
                .ToList();

        /// <summary>La Y del larguero de entrada en el corte FRONTAL de entrada/salida.</summary>
        private static IReadOnlyList<double> FrontalLow(PushBackSystem system, PushBackSide side = PushBackSide.A)
            => new PushBackSystemFrontalBuilder()
                .BuildPlan(system, Catalog, PushBackFrontalEnd.EntradaSalida, side)
                .Flatten().Instances
                .Where(instance => instance.Role == HeaderBlockRole.Beam
                                   && instance.PieceId == DynamicRackDefaults.InOutBeamCatalogId)
                .Select(instance => Math.Round(instance.Insertion.Y, 4))
                .Distinct()
                .OrderBy(y => y)
                .ToList();

        // ---- fábricas -------------------------------------------------------------------------------------------

        private static PushBackEditorState SingleSided(double firstLevelHeight, int fronts = 2)
        {
            var state = new PushBackEditorState();
            state.LoadNew();
            state.Structure.SetFrontCount(fronts);
            for (var index = 0; index < state.Structure.Count; index++)
            {
                state.Structure.Fronts[index].FirstLevelHeight = firstLevelHeight;
                state.Structure.AdjustLevels(index, 2 - state.Structure.Fronts[index].LoadLevels);
            }

            return state;
        }

        private static PushBackCompositeEditorState Composite(double firstA, double firstB, bool declareB)
        {
            var state = new PushBackCompositeEditorState();
            state.SideA.LoadNew();
            state.SetSlotCount(2);
            for (var index = 0; index < state.SideA.Structure.Count; index++)
            {
                state.SideA.Structure.Fronts[index].FirstLevelHeight = firstA;
            }

            if (declareB)
            {
                state.SetSideBPresent(true);
                for (var slot = 0; slot < 2; slot++)
                {
                    state.SetSlotPresent(PushBackSide.B, slot, true);
                }

                for (var index = 0; index < state.SideB.Structure.Count; index++)
                {
                    state.SideB.Structure.Fronts[index].FirstLevelHeight = firstB;
                }
            }

            return state;
        }

        private static PushBackDesign RoundTrip(PushBackDesign design)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(PushBackDesignDocument.FromDomain(design));
            return System.Text.Json.JsonSerializer.Deserialize<PushBackDesignDocument>(json).ToDomain();
        }

        // ================= rack NUEVO ============================================================================

        /// <summary>«0» es exactamente el troquel utilizable más bajo, y las tres vistas lo dicen igual.</summary>
        [Fact]
        public void NewPushBack_FirstLevelZeroMeansLowestUsablePunch()
        {
            var computation = new PushBackEditorDesignAssembler(Catalog).Build(SingleSided(0.0), Inputs());
            var system = computation.System;

            Assert.Equal(NewDatum, system.Structure.FirstLevelDatum);
            Assert.Equal(LowestPunch(system.Structure), LowZ(system), 6);
            Assert.Equal(0, PunchIndex(system.Structure, LowZ(system)));
            Assert.Contains(LowZ(system), LateralLow(system));
            Assert.Contains(LowZ(system), FrontalLow(system));
        }

        /// <summary>
        /// Con un valor IMPAR, la lectura del producto y la histórica caen en troqueles DISTINTOS. El rack nuevo usa
        /// la del producto: troquel utilizable más bajo + el valor, ajustado a la retícula.
        /// </summary>
        [Theory]
        [InlineData(5.0)]
        [InlineData(7.0)]
        public void NewPushBack_OddFirstLevelHeightUsesLowestUsablePunchDatum(double first)
        {
            var system = new PushBackEditorDesignAssembler(Catalog).Build(SingleSided(first), Inputs()).System;
            var structure = system.Structure;

            Assert.Equal(NewDatum, structure.FirstLevelDatum);

            var expected = PushBackTroquelGrid.Snap(LowestPunch(structure) + first, GridBase(structure));
            Assert.Equal(Math.Round(expected, 4), LowZ(system), 6);

            // Y NO coincide con la lectura histórica: si coincidiera, la prueba no comprobaría nada.
            var legacy = PushBackTroquelGrid.Snap(first, GridBase(structure));
            Assert.NotEqual(Math.Round(legacy, 4), LowZ(system));

            Assert.Contains(LowZ(system), LateralLow(system));
            Assert.Contains(LowZ(system), FrontalLow(system));
        }

        /// <summary>Recalcular no vuelve a decidir el datum ni mueve el larguero.</summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(5.0)]
        [InlineData(7.0)]
        public void Recompute_DoesNotChangeFirstLevelDatum(double first)
        {
            var assembler = new PushBackEditorDesignAssembler(Catalog);
            var state = SingleSided(first);

            var once = assembler.Build(state, Inputs()).System;
            var twice = assembler.Build(state, Inputs()).System;
            var thrice = assembler.Build(state, Inputs()).System;

            Assert.Equal(once.Structure.FirstLevelDatum, twice.Structure.FirstLevelDatum);
            Assert.Equal(once.Structure.FirstLevelDatum, thrice.Structure.FirstLevelDatum);
            Assert.Equal(LowZ(once), LowZ(twice), 6);
            Assert.Equal(LowZ(once), LowZ(thrice), 6);
            Assert.Equal(PunchIndex(once.Structure, LowZ(once)), PunchIndex(thrice.Structure, LowZ(thrice)));
        }

        /// <summary>Reabrir y volver a recalcular —el camino de «Actualizar»— deja el larguero en su troquel.</summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(5.0)]
        [InlineData(7.0)]
        public void Update_DoesNotMoveThePhysicalLowBeam(double first)
        {
            var assembler = new PushBackEditorDesignAssembler(Catalog);
            var before = assembler.Build(SingleSided(first), Inputs());
            var expected = LowZ(before.System);

            var reopened = new PushBackEditorState();
            var recovered = reopened.LoadFromDesign(before.Design, assembler.Resolver);
            var after = assembler.Build(reopened, recovered).System;

            Assert.Equal(expected, LowZ(after), 6);
            Assert.Equal(NewDatum, after.Structure.FirstLevelDatum);
            Assert.Equal(LateralLow(before.System), LateralLow(after));
            Assert.Equal(FrontalLow(before.System), FrontalLow(after));
        }

        /// <summary>Guardar, abrir y volver a guardar conserva el marcador y el troquel.</summary>
        [Theory]
        [InlineData(5.0)]
        [InlineData(7.0)]
        public void SaveLoadSave_PreservesDatumAndPhysicalLow(double first)
        {
            var assembler = new PushBackEditorDesignAssembler(Catalog);
            var first_ = assembler.Build(SingleSided(first), Inputs());
            var expected = LowZ(first_.System);

            var reloaded = RoundTrip(first_.Design);
            Assert.Equal(NewDatum, reloaded.Structure.FirstLevelDatum);

            var reopened = new PushBackEditorState();
            var recovered = reopened.LoadFromDesign(reloaded, assembler.Resolver);
            var resaved = RoundTrip(assembler.BuildDesign(reopened, recovered));

            Assert.Equal(NewDatum, resaved.Structure.FirstLevelDatum);
            Assert.Equal(expected, LowZ(new PushBackResolver(Catalog).Resolve(resaved)), 6);
        }

        // ================= compuesto A/B =========================================================================

        /// <summary>
        /// La intención estructural compartida lleva el datum a la sub-estructura de cada lado y a la compuesta. Sin
        /// eso, las tres lo leían con la semántica histórica.
        /// </summary>
        [Theory]
        [InlineData(5.0)]
        [InlineData(7.0)]
        public void Composite_CopySharedStructuralIntentPreservesDatum(double first)
        {
            var system = new PushBackCompositeEditorAssembler(Catalog)
                .Build(Composite(first, first, declareB: true), Inputs(), Catalog).System;

            Assert.Equal(NewDatum, system.Structure.FirstLevelDatum);
            foreach (var side in new[] { PushBackSide.A, PushBackSide.B })
            {
                Assert.Equal(NewDatum, system.Composite.Of(side).Local.Structure.FirstLevelDatum);
            }
        }

        /// <summary>
        /// EL DEFECTO. Declarar el lado B no puede mover el primer nivel del lado A. Medido antes de la corrección:
        /// con «Alto 1er nivel» = 7 el LOW de A pasaba de 8.6053 a 6.6053 — un troquel entero — sólo por declarar B.
        /// </summary>
        [Theory]
        [InlineData(5.0)]
        [InlineData(7.0)]
        public void AddingSideB_DoesNotMoveSideA(double first)
        {
            var assembler = new PushBackCompositeEditorAssembler(Catalog);
            var soloA = assembler.Build(Composite(first, first, declareB: false), Inputs(), Catalog).System;
            var withB = assembler.Build(Composite(first, first, declareB: true), Inputs(), Catalog).System;

            Assert.Equal(LowZ(soloA), LowZ(withB), 6);
            Assert.Equal(
                PunchIndex(soloA.Structure, LowZ(soloA)),
                PunchIndex(withB.Structure, LowZ(withB)));
            Assert.Equal(soloA.Structure.FirstLevelDatum, withB.Structure.FirstLevelDatum);

            var localA = withB.Composite.Of(PushBackSide.A).Local;
            Assert.Equal(LowZ(soloA), LowZ(localA), 6);
        }

        /// <summary>La misma intención visible en los dos lados cae en el MISMO troquel.</summary>
        [Theory]
        [InlineData(5.0)]
        [InlineData(7.0)]
        public void EqualVisibleIntent_AAndBAlignOnTheSamePunch(double first)
        {
            var system = new PushBackCompositeEditorAssembler(Catalog)
                .Build(Composite(first, first, declareB: true), Inputs(), Catalog).System;

            var localA = system.Composite.Of(PushBackSide.A).Local;
            var localB = system.Composite.Of(PushBackSide.B).Local;

            Assert.Equal(LowZ(localA), LowZ(localB), 6);
            Assert.Equal(
                PunchIndex(localA.Structure, LowZ(localA)),
                PunchIndex(localB.Structure, LowZ(localB)));

            // Y los dos cortes frontales lo dicen igual.
            Assert.Equal(FrontalLow(system, PushBackSide.A), FrontalLow(system, PushBackSide.B));
        }

        // ================= documentos anteriores ==================================================================

        /// <summary>Un documento SIN marcador conserva su geometría física exacta al resolverse.</summary>
        [Theory]
        [InlineData(5.0)]
        [InlineData(7.0)]
        public void LegacyPushBackWithoutMarker_PreservesPhysicalGeometry(double first)
        {
            var design = new PushBackEditorDesignAssembler(Catalog).BuildDesign(SingleSided(first), Inputs());
            design.Structure.FirstLevelDatum = null;

            var system = new PushBackResolver(Catalog).Resolve(design);
            var historic = PushBackTroquelGrid.Snap(first, GridBase(system.Structure));

            Assert.Equal(Math.Round(historic, 4), LowZ(system), 6);
            Assert.Null(system.Structure.FirstLevelDatum);
            Assert.Contains(LowZ(system), LateralLow(system));
        }

        /// <summary>
        /// Al abrirlo, ese documento se re-expresa sobre el datum del producto SIN moverse: el número guardado
        /// cambia porque se midió la geometría real, y el troquel es exactamente el mismo antes, después de migrar y
        /// después de reabrir.
        /// </summary>
        [Theory]
        [InlineData(5.0)]
        [InlineData(7.0)]
        public void LegacyPushBack_MigrationWritesNewDatumWithoutMovingGeometry(double first)
        {
            var assembler = new PushBackEditorDesignAssembler(Catalog);
            var design = assembler.BuildDesign(SingleSided(first), Inputs());
            design.Structure.FirstLevelDatum = null;

            var before = new PushBackResolver(Catalog).Resolve(design);
            var physicalBefore = LowZ(before);
            var punchBefore = PunchIndex(before.Structure, physicalBefore);

            var reopened = new PushBackEditorState();
            var recovered = reopened.LoadFromDesign(design, assembler.Resolver);

            // La conversión ocurre UNA vez, en la carga: marcador nuevo y número re-expresado desde la retícula.
            Assert.Equal(NewDatum, recovered.FirstLevelDatum);
            Assert.Equal(
                RackFirstLevelDatum.ToLowestPunchOffset(physicalBefore, GridBase(before.Structure), Pitch),
                reopened.Structure.Fronts[0].FirstLevelHeight,
                6);
            Assert.NotEqual(first, reopened.Structure.Fronts[0].FirstLevelHeight);

            var migrated = assembler.Build(reopened, recovered);
            Assert.Equal(physicalBefore, LowZ(migrated.System), 6);
            Assert.Equal(punchBefore, PunchIndex(migrated.System.Structure, LowZ(migrated.System)));

            var afterReopen = new PushBackResolver(Catalog).Resolve(RoundTrip(migrated.Design));
            Assert.Equal(physicalBefore, LowZ(afterReopen), 6);
            Assert.Equal(NewDatum, afterReopen.Structure.FirstLevelDatum);
            Assert.Equal(LateralLow(before), LateralLow(afterReopen));
            Assert.Equal(FrontalLow(before), FrontalLow(afterReopen));
        }

        /// <summary>RACKEDITAR —cargar el sistema resuelto— conserva el marcador y el troquel.</summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(5.0)]
        [InlineData(7.0)]
        public void RackEditar_PreservesFirstLevelDatum(double first)
        {
            var assembler = new PushBackEditorDesignAssembler(Catalog);
            var built = assembler.Build(SingleSided(first), Inputs());
            var expected = LowZ(built.System);

            var reopened = new PushBackEditorState();
            var recovered = reopened.LoadFromSystem(built.System, assembler.Resolver);

            Assert.Equal(NewDatum, recovered.FirstLevelDatum);
            Assert.Equal(first, reopened.Structure.Fronts[0].FirstLevelHeight, 6);
            Assert.Equal(expected, LowZ(assembler.Build(reopened, recovered).System), 6);
        }

        // ================= el DINÁMICO, que comparte el helper ====================================================

        private static DynamicRackDesign DynamicDesign(double first, int? datum)
            => new DynamicRackDesign
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = 2,
                LoadLevels = 2,
                FirstLevelHeight = first,
                BeamDepth = DynamicRackDefaults.DefaultBeamDepth,
                FirstLevelDatum = datum
            };

        private static double DynamicLow(DynamicRackSystem system)
            => Math.Round(system.Fronts[0].LoadBeamLevels.OrderBy(level => level.LevelNumber).First().ExitElevation, 4);

        /// <summary>Un Dinámico NUEVO mide desde el troquel utilizable más bajo, igual que antes de esta corrida.</summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(5.0)]
        [InlineData(7.0)]
        public void Dynamic_NewDatumStillMatchesItsApprovedGeometry(double first)
        {
            var system = new DynamicRackSystemResolver(Catalog).Resolve(DynamicDesign(first, NewDatum)).System;
            var expected = PushBackTroquelGrid.Snap(LowestPunch(system) + first, GridBase(system));

            Assert.Equal(NewDatum, system.FirstLevelDatum);
            Assert.Equal(Math.Round(expected, 4), DynamicLow(system), 6);
        }

        /// <summary>Y un Dinámico ANTERIOR conserva su geometría histórica: el resolver no lo reinterpreta.</summary>
        [Theory]
        [InlineData(5.0)]
        [InlineData(7.0)]
        public void Dynamic_LegacyStillPreservesPhysicalGeometry(double first)
        {
            var system = new DynamicRackSystemResolver(Catalog).Resolve(DynamicDesign(first, null)).System;
            var historic = PushBackTroquelGrid.Snap(first, GridBase(system));

            Assert.Null(system.FirstLevelDatum);
            Assert.Equal(Math.Round(historic, 4), DynamicLow(system), 6);
            Assert.NotEqual(
                Math.Round(PushBackTroquelGrid.Snap(LowestPunch(system) + first, GridBase(system)), 4),
                DynamicLow(system));
        }
    }
}
