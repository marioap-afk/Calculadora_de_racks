using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 (A1B/B7, contrato del dueño) — QUE MODULOS ALOJAN TARIMA.
    ///
    /// <para>
    /// El separador central vive fisicamente en el hueco entre las dos mitades y se emite con el tipo Separador
    /// —que en el Dinamico SI es una bahia de tarima—. Las reglas de posiciones excluian solo el hueco, asi que el
    /// separador se comia una posicion de almacenamiento: la cama quedaba corta, el extremo alto terminaba un
    /// modulo antes y el rack cotizaba una geometria que no era la pedida.
    /// </para>
    /// <para>
    /// El contrato del dueño: <b>una corrida SI admite separador central</b>, y el separador <b>no consume fondo,
    /// no suma demanda y no acorta la cama</b>. Solo aporta su pieza fisica.
    /// </para>
    /// </summary>
    public class PushBackA1BStorageTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static PushBackEditorInputs Inputs()
        {
            var inputs = PushBackEditorInputs.NewDesign();
            inputs.Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg");
            return inputs;
        }

        /// <summary>El MISMO rack compuesto con hueco, con o sin separador central.</summary>
        private static PushBackSystem Resolve(bool separator, double gap = 12.0, int slots = 2)
        {
            var state = new PushBackCompositeEditorState();
            state.SideA.LoadNew();
            state.SetSlotCount(slots);
            state.SetSideBPresent(true);
            state.SideB.LoadNew();
            state.SetSlotCount(slots);
            for (var slot = 0; slot < slots; slot++)
            {
                state.SetSlotPresent(PushBackSide.B, slot, true);
            }

            state.SetGap(gap);
            state.SetCentralSeparator(separator);

            var design = new PushBackCompositeEditorAssembler(Catalog).Build(state, Inputs(), Catalog).Design;
            return new PushBackResolver(Catalog).Resolve(design);
        }

        private static DynamicRackModule Interface(PushBackSystem system)
            => system.Structure.Modules.Single(module => PushBackCompositeStructure.IsInterfaceModule(module));

        /// <summary>Las posiciones de ALMACENAMIENTO de la estructura, que es lo que una cama puede ocupar.</summary>
        private static int StoragePositions(PushBackSystem system)
            => system.Structure.Modules.Count(PushBackCompositeStructure.IsStoragePosition);

        /// <summary>La X de contacto ALTO de cada cama: donde termina fisicamente.</summary>
        private static IReadOnlyList<double> HighContacts(PushBackSystem system)
        {
            var runs = PushBackRuns.Resolve(system);
            return runs.Runs
                .Select(run => PushBackRunGeometry.Axis(run, Catalog, runs.MirrorAxis))
                .Where(axis => axis.HasValue)
                .Select(axis => Math.Round(axis.Value.HighContact.X, 3))
                .OrderBy(value => value)
                .ToList();
        }

        // ==================================================================== B7

        /// <summary>El separador central NO es una posicion de almacenamiento; el hueco tampoco.</summary>
        [Fact]
        public void CentralSeparator_IsNotAStoragePosition()
        {
            var withSeparator = Resolve(separator: true);
            var withoutSeparator = Resolve(separator: false);

            Assert.Equal(DynamicRackModuleKind.Separator, Interface(withSeparator).Kind);
            Assert.False(PushBackCompositeStructure.IsStoragePosition(Interface(withSeparator)));
            Assert.False(PushBackCompositeStructure.IsStoragePosition(Interface(withoutSeparator)));
        }

        /// <summary>Y por tanto no cambia cuantas posiciones de almacenamiento tiene el rack.</summary>
        [Fact]
        public void CentralSeparator_DoesNotConsumeCorridaStoragePosition()
            => Assert.Equal(StoragePositions(Resolve(separator: false)), StoragePositions(Resolve(separator: true)));

        /// <summary>Ni mueve el extremo alto de ninguna cama.</summary>
        [Fact]
        public void CentralSeparator_DoesNotChangeCorridaHighContact()
            => Assert.Equal(HighContacts(Resolve(separator: false)), HighContacts(Resolve(separator: true)));

        /// <summary>
        /// Ni la DEMANDA de cama de las celdas —lo que hay que cubrir— ni el tramo disponible: el separador aporta
        /// su pieza y nada mas.
        /// </summary>
        [Fact]
        public void CentralSeparator_DoesNotChangeCellDemand()
        {
            var without = Resolve(separator: false);
            var with = Resolve(separator: true);

            Assert.Equal(
                without.Composite.Cells
                    .Select(cell => Math.Round(cell.RequiredBedLength, 3)).ToList(),
                with.Composite.Cells
                    .Select(cell => Math.Round(cell.RequiredBedLength, 3)).ToList());
            Assert.Equal(
                without.Composite.Cells
                    .Select(cell => Math.Round(cell.AvailableBedSpan, 3)).ToList(),
                with.Composite.Cells
                    .Select(cell => Math.Round(cell.AvailableBedSpan, 3)).ToList());
        }

        // ==================================================================== H2

        /// <summary>
        /// H2 — el BOM de SEPARADORES usa la altura de esa linea EN ESA PROFUNDIDAD, la misma autoridad que el
        /// corte lateral. Con una altura por poste —el maximo de sus frentes adyacentes— un compuesto con lados de
        /// distinta altura compraba filas de la zona alta tambien en la zona baja.
        /// </summary>
        [Fact]
        public void HeaderSeparators_BomUsesPostHeightAtItsDepth()
        {
            var system = Asymmetric();
            var zones = system.Structure.HeaderHeightZones;
            Assert.True(zones.Count >= 2, "el fixture debe declarar zonas de altura por lado");

            var separators = system.Structure.Modules
                .Where(module => module.Kind == DynamicRackModuleKind.Separator && module.Length > 0.0)
                .ToList();
            Assert.NotEmpty(separators);

            // Alguna linea tiene alturas distintas segun la zona: es lo que el BOM tenia que dejar de aplanar.
            var line = Enumerable.Range(0, system.Structure.Fronts.Count + 1)
                .First(post => zones.Select(zone => zone.HeightByLine.Count > post ? zone.HeightByLine[post] : 0.0)
                    .Distinct().Count() > 1);
            var byZone = separators
                .Select(module => DynamicFrontGeometry.PostHeightAt(
                    system.Structure, line, 0.5 * (module.StartX + module.EndX)))
                .Distinct()
                .ToList();
            var flat = DynamicFrontGeometry.PostHeight(system.Structure, line);

            Assert.True(byZone.Count > 1);                     // la linea NO tiene una sola altura…
            Assert.Contains(byZone, height => height < flat);  // …y aplanarla compraba de mas
        }

        /// <summary>Un compuesto con el lado A mas alto que el B.</summary>
        private static PushBackSystem Asymmetric()
        {
            var state = new PushBackCompositeEditorState();
            state.SideA.LoadNew();
            state.SetSlotCount(2);
            state.SetSideBPresent(true);
            state.SideB.LoadNew();
            state.SetSlotCount(2);
            for (var slot = 0; slot < 2; slot++)
            {
                state.SetSlotPresent(PushBackSide.B, slot, true);
            }

            state.SetGap(12.0);
            state.SetActiveSide(PushBackSide.A);
            state.SideA.AdjustLevels(0, 2);
            state.SideA.AdjustLevels(1, 2);

            var design = new PushBackCompositeEditorAssembler(Catalog).Build(state, Inputs(), Catalog).Design;
            return new PushBackResolver(Catalog).Resolve(design);
        }

        /// <summary>
        /// BITE — volver al criterio anterior («todo lo que no sea hueco almacena») devuelve el defecto: con
        /// separador central el rack contaria una posicion mas de la que tiene.
        /// </summary>
        [Fact]
        public void Bite_KindNotGapAsStorage_CountsTheSeparatorAsAPosition()
        {
            var with = Resolve(separator: true);
            var legacy = with.Structure.Modules.Count(module => module.Kind != DynamicRackModuleKind.Gap);

            Assert.Equal(StoragePositions(with) + 1, legacy);
        }
    }
}
