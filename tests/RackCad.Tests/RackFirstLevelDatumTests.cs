using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using RackCad.Application.Catalogs;
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
    /// I-42 — el DATUM de «Alto 1er nivel» (decision del dueño).
    ///
    /// <para>
    /// <b>0" significa el TROQUEL UTILIZABLE MAS BAJO del poste</b>, y el numero es un OFFSET sobre el. La lectura
    /// anterior trataba el valor como una elevacion ABSOLUTA y lo ajustaba al troquel MAS CERCANO: «0» no
    /// significaba nada fisico y, con un poste cuya retícula empezara mas arriba, podia caer por debajo del piso.
    /// </para>
    /// <para>
    /// La autoridad es NEUTRAL y COMPARTIDA (<see cref="RackFirstLevelDatum"/>): el dato de usuario es el mismo en el
    /// DINAMICO y en el PUSH BACK, y los dos lo resuelven en el mismo sitio
    /// (<see cref="DynamicRackSystemResolver"/>). El datum sale de la geometria del POSTE —su mate
    /// <c>TROQUEL_LARGUERO</c> y el paso de troquel—, nunca de una constante.
    /// </para>
    /// <para>
    /// La compatibilidad es ADITIVA: un documento sin marcador se lee con la semantica historica y reabre en la
    /// MISMA geometria fisica. Ningun valor historico se reinterpreta.
    /// </para>
    /// </summary>
    public class RackFirstLevelDatumTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        /// <summary>La retícula de troqueles del poste que el rack usa: su base y su paso, tal como el resolver los lee.</summary>
        private static (double Base, double Pitch) PunchGrid(RackCatalog catalog, string postId = null)
        {
            var id = string.IsNullOrWhiteSpace(postId) ? catalog.Defaults?.Post : postId;
            var entry = catalog.ConnectionLayout.FindConnectionLayout(
                id, SelectiveRackDefaults.PostBeamPoint, "FRONTAL");
            var y = SelectivePostGeometry.Resolve(
                entry,
                new Dictionary<string, double> { [SelectiveRackDefaults.PeralteParam] = 0.0 }).Y;
            return (y, SelectiveRackDefaults.TroquelPaso);
        }

        // ================= La autoridad neutral =================================================================

        /// <summary>
        /// El troquel utilizable mas bajo es el PRIMERO que no queda por debajo del piso, y sale de la retícula del
        /// poste. No es una constante: con otra base, otro datum.
        /// </summary>
        [Theory]
        [InlineData(0.6053, 2.0, 0.6053)]
        [InlineData(1.5, 2.0, 1.5)]
        [InlineData(-0.4, 2.0, 1.6)]
        [InlineData(-3.0, 2.0, 1.0)]
        [InlineData(2.0, 3.0, 2.0)]
        public void TheLowestUsablePunch_ComesFromThePostGrid(double gridBase, double pitch, double expected)
        {
            var datum = RackFirstLevelDatum.LowestUsablePunch(gridBase, pitch);

            Assert.Equal(expected, datum, 6);
            Assert.True(datum >= -1e-9, "el troquel utilizable no puede quedar bajo el piso");

            // Y es un punto REAL de la retícula: la distancia a la base es un multiplo entero del paso.
            var steps = (datum - gridBase) / pitch;
            Assert.Equal(Math.Round(steps), steps, 6);
        }

        /// <summary>Sin retícula medida no se inventa ningun datum: la lectura absoluta es la unica que no miente.</summary>
        [Fact]
        public void WithoutAMeasuredGrid_TheDatumIsZero()
        {
            Assert.Equal(0.0, RackFirstLevelDatum.LowestUsablePunch(double.NaN, 2.0), 6);
            Assert.Equal(0.0, RackFirstLevelDatum.LowestUsablePunch(1.0, 0.0), 6);
        }

        /// <summary>La lectura HISTORICA no cambia: el numero sigue siendo una elevacion absoluta.</summary>
        [Fact]
        public void TheLegacyReading_IsUntouched()
        {
            Assert.Equal(
                7.0,
                RackFirstLevelDatum.RawElevation(7.0, RackFirstLevelDatumMode.LegacyAbsolute, 0.6053, 2.0),
                6);
        }

        // ================= H: un rack NUEVO ====================================================================

        private static PushBackEditorInputs Inputs()
        {
            var inputs = PushBackEditorInputs.NewDesign();
            inputs.Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg");
            return inputs;
        }

        private static PushBackSystem NewPushBack(double firstLevel, bool composite = false)
        {
            var state = new PushBackCompositeEditorState();
            state.SideA.LoadNew();
            if (composite)
            {
                state.SetSideBPresent(true);
                state.SideB.LoadNew();
                state.SetSlotCount(2);
            }

            foreach (var side in composite
                         ? new[] { PushBackSide.A, PushBackSide.B }
                         : new[] { PushBackSide.A })
            {
                var matrix = state.Of(side).Structure;
                for (var front = 0; front < matrix.Count; front++)
                {
                    matrix.Fronts[front].FirstLevelHeight = firstLevel;
                }
            }

            return new PushBackCompositeEditorAssembler(Catalog).Build(state, Inputs(), Catalog).System;
        }

        /// <summary>La elevacion del primer larguero de carga de un frente, tal como el resolver la fija.</summary>
        private static double FirstBeamElevation(PushBackSystem system, int front = 0)
            => system.Structure.Fronts[front].LoadBeamLevels
                .OrderBy(level => level.LevelNumber)
                .First()
                .ExitElevation;

        /// <summary>
        /// PRUEBA VINCULANTE. En un rack NUEVO, «Alto 1er nivel = 0» pone el larguero EXACTAMENTE en el troquel
        /// utilizable mas bajo del poste. Ni una pulgada mas arriba.
        /// </summary>
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ANewRack_WithZero_PutsTheBeamOnTheLowestUsablePunch(bool composite)
        {
            var grid = PunchGrid(Catalog);
            var expected = RackFirstLevelDatum.LowestUsablePunch(grid.Base, grid.Pitch);

            var system = NewPushBack(0.0, composite);
            Assert.Equal(expected, FirstBeamElevation(system), 6);

            if (composite)
            {
                // Y en los DOS lados: el datum es del producto, no de un lado.
                Assert.Equal(expected, FirstBeamElevation(system.Composite.SideA.Local, 0), 6);
                Assert.Equal(expected, FirstBeamElevation(system.Composite.SideB.Local, 0), 6);
            }
        }

        /// <summary>
        /// Y un valor N se mide DESDE ese datum, resuelto despues contra la retícula: la separacion respecto del cero
        /// es N ajustado al troquel, no N contado desde un origen arbitrario.
        /// </summary>
        [Theory]
        [InlineData(2.0)]
        [InlineData(4.0)]
        [InlineData(6.0)]
        public void ANewRack_MeasuresTheValueFromThatDatum(double value)
        {
            var grid = PunchGrid(Catalog);
            var datum = RackFirstLevelDatum.LowestUsablePunch(grid.Base, grid.Pitch);

            var elevation = FirstBeamElevation(NewPushBack(value));
            var offset = elevation - datum;

            // El desplazamiento respecto del cero real es el pedido, dentro de medio paso de troquel.
            Assert.True(
                Math.Abs(offset - value) <= grid.Pitch / 2.0 + 1e-6,
                "con Alto = " + value + " el larguero quedo a " + offset + " del troquel mas bajo");

            // Y sigue cayendo sobre un troquel real.
            var steps = (elevation - grid.Base) / grid.Pitch;
            Assert.Equal(Math.Round(steps), steps, 6);
        }

        // ================= F/G: compatibilidad LEGACY ==========================================================

        /// <summary>Un diseño Push Back «historico»: sin marcador de datum, como todo documento anterior.</summary>
        private static PushBackDesign LegacyDesign(double storedFirstLevel)
        {
            var design = new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                    PalletsDeep = 4,
                    LoadLevels = 2,
                    FirstLevelHeight = storedFirstLevel,
                    BeamDepth = 4.0
                }
            };
            design.Structure.Fronts.Add(new DynamicRackFrontDesign
            {
                PalletCount = 1, LoadLevels = 2, PalletsDeep = 4, DepthStartPosition = 1,
                FirstLevelHeight = storedFirstLevel
            });
            design.Fronts.Add(new PushBackFrontConfig { DefaultPalletsDeep = 4 });
            Assert.Null(design.Structure.FirstLevelDatum);   // historico: sin marcador
            return design;
        }

        /// <summary>
        /// PRUEBA VINCULANTE de compatibilidad. Un documento historico reabre en la MISMA geometria fisica. No se
        /// compara JSON: se compara la elevacion del larguero.
        /// </summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(3.0)]
        [InlineData(4.0)]
        [InlineData(7.5)]
        public void ALegacyDocument_KeepsItsPhysicalGeometry(double stored)
        {
            var resolver = new PushBackResolver(Catalog);
            var design = LegacyDesign(stored);

            var before = FirstBeamElevation(resolver.Resolve(design));

            // Guardar y reabrir por el camino real de persistencia: el marcador sigue ausente.
            var json = JsonSerializer.Serialize(PushBackDesignDocument.FromDomain(design));
            var restored = JsonSerializer.Deserialize<PushBackDesignDocument>(json).ToDomain();
            Assert.Null(restored.Structure.FirstLevelDatum);
            Assert.DoesNotContain("FirstLevelDatum", json, StringComparison.OrdinalIgnoreCase);

            Assert.Equal(before, FirstBeamElevation(resolver.Resolve(restored)), 6);
        }

        /// <summary>
        /// Y la MIGRACION conserva la geometria: se mide la elevacion fisica real, se re-expresa como offset sobre el
        /// troquel mas bajo y se guarda con el marcador nuevo. No se resta ninguna constante.
        /// </summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(3.0)]
        [InlineData(4.0)]
        [InlineData(7.5)]
        public void MigratingALegacyDocument_DoesNotMoveIt(double stored)
        {
            var resolver = new PushBackResolver(Catalog);
            var legacy = LegacyDesign(stored);
            var before = FirstBeamElevation(resolver.Resolve(legacy));

            // La conversion: la elevacion FISICA ya resuelta, expresada desde el nuevo datum.
            var grid = PunchGrid(Catalog);
            var migratedValue = RackFirstLevelDatum.ToLowestPunchOffset(before, grid.Base, grid.Pitch);

            var migrated = LegacyDesign(stored);
            migrated.Structure.FirstLevelDatum = (int)RackFirstLevelDatumMode.LowestUsablePunch;
            migrated.Structure.FirstLevelHeight = migratedValue;
            migrated.Structure.Fronts[0].FirstLevelHeight = migratedValue;

            Assert.Equal(before, FirstBeamElevation(resolver.Resolve(migrated)), 6);

            // Y el round trip de un documento YA migrado tampoco lo mueve.
            var json = JsonSerializer.Serialize(PushBackDesignDocument.FromDomain(migrated));
            var reopened = JsonSerializer.Deserialize<PushBackDesignDocument>(json).ToDomain();
            Assert.Equal((int)RackFirstLevelDatumMode.LowestUsablePunch, reopened.Structure.FirstLevelDatum);
            Assert.Equal(before, FirstBeamElevation(resolver.Resolve(reopened)), 6);
        }

        /// <summary>
        /// PRUEBA VINCULANTE de RACKEDITAR: el datum sobrevive al SNAPSHOT. El editor reconstruye el rack desde el
        /// sistema resuelto, asi que si el marcador no viajara del diseño al sistema y del sistema al snapshot, abrir
        /// un rack nuevo para editarlo lo releeria con la semantica historica y lo moveria.
        /// </summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(4.0)]
        public void TheDatum_SurvivesTheSnapshot(double firstLevel)
        {
            var resolver = new PushBackResolver(Catalog);
            var system = NewPushBack(firstLevel);
            Assert.Equal(
                (int)RackFirstLevelDatumMode.LowestUsablePunch, system.Structure.FirstLevelDatum);

            var before = FirstBeamElevation(system);
            var snapshot = resolver.Snapshot(system);

            Assert.Equal((int)RackFirstLevelDatumMode.LowestUsablePunch, snapshot.Structure.FirstLevelDatum);
            Assert.Equal(before, FirstBeamElevation(resolver.Resolve(snapshot)), 6);
        }

        // ================= D: el sistema DINAMICO comparte el contrato =========================================

        private static DynamicRackDesign DynamicDesign(double firstLevel, int? datum)
        {
            var design = new DynamicRackDesign
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = 4,
                LoadLevels = 2,
                FirstLevelHeight = firstLevel,
                BeamDepth = 4.0,
                FirstLevelDatum = datum
            };
            design.Fronts.Add(new DynamicRackFrontDesign
            {
                PalletCount = 1, LoadLevels = 2, PalletsDeep = 4, DepthStartPosition = 1,
                FirstLevelHeight = firstLevel
            });
            return design;
        }

        private static double DynamicFirstBeam(DynamicRackDesign design)
            => new DynamicRackSystemResolver(Catalog).Resolve(design).System
                .Fronts[0].LoadBeamLevels.OrderBy(level => level.LevelNumber).First().ExitElevation;

        /// <summary>
        /// El DINAMICO usa la misma autoridad y por tanto la misma semantica: «0» es el troquel utilizable mas bajo.
        /// Es el mismo dato de usuario, resuelto en el mismo sitio — no dos offsets especiales.
        /// </summary>
        [Fact]
        public void TheDynamicSystem_SharesTheSameDatumContract()
        {
            var grid = PunchGrid(Catalog);
            var expected = RackFirstLevelDatum.LowestUsablePunch(grid.Base, grid.Pitch);

            var modern = DynamicDesign(0.0, (int)RackFirstLevelDatumMode.LowestUsablePunch);
            Assert.Equal(expected, DynamicFirstBeam(modern), 6);
        }

        /// <summary>Y un documento DINAMICO historico —sin marcador— conserva su geometria exacta.</summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(6.0)]
        [InlineData(9.5)]
        public void ALegacyDynamicDocument_KeepsItsPhysicalGeometry(double stored)
        {
            var legacy = DynamicDesign(stored, null);
            var before = DynamicFirstBeam(legacy);

            var json = JsonSerializer.Serialize(DynamicRackSystemDocument.From(legacy));
            var restored = JsonSerializer.Deserialize<DynamicRackSystemDocument>(json).ToDesign();

            Assert.Null(restored.FirstLevelDatum);
            Assert.Equal(before, DynamicFirstBeam(restored), 6);
        }
    }
}
