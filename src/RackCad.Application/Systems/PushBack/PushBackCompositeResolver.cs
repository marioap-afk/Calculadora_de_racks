using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.Dynamic;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;

namespace RackCad.Application.Systems.PushBack
{
    /// <summary>
    /// I-42 — el limite diseno-&gt;sistema del Push Back COMPUESTO. Resuelve, en este orden:
    ///
    /// <list type="number">
    /// <item>la lectura uniforme de los dos lados (<see cref="PushBackSideConfiguration"/>) y, con ella, la
    /// estructura PROPUESTA y la EFECTIVA de cada uno;</item>
    /// <item>la sub-estructura de cada lado en su MARCO LOCAL, delegando integramente en el resolver dinamico: ni
    /// una regla de cabeceras, separadores, postes derivados o alturas se reescribe;</item>
    /// <item>la ESTRUCTURA FISICA UNICA (A + hueco + B invertido) sobre una sola retícula transversal;</item>
    /// <item>la rejilla de celdas con su topologia, su sentido y las dos magnitudes que deciden si son
    /// construibles.</item>
    /// </list>
    ///
    /// <para>
    /// El resultado es UN plan fisico: los postes, cabeceras, placas y separadores existen una sola vez, en la
    /// estructura compuesta, y los dos lados solo aportan su contenido de almacenamiento. No hay «rack A + rack B»
    /// que deduplicar despues.
    /// </para>
    /// </summary>
    public sealed class PushBackCompositeResolver
    {
        private readonly RackCatalog catalog;
        private readonly DynamicRackSystemResolver structureResolver;

        public PushBackCompositeResolver(RackCatalog catalog)
        {
            this.catalog = catalog ?? new RackCatalog();
            structureResolver = new DynamicRackSystemResolver(this.catalog);
        }

        /// <summary>
        /// Resuelve la parte COMPUESTA y la ESTRUCTURA compartida. Devuelve la estructura fisica unica y el
        /// agregado compuesto; el llamador (<see cref="PushBackResolver"/>) los monta sobre el sistema Push Back y
        /// se encarga de la seguridad, que es del rack y no de un lado.
        /// </summary>
        public (DynamicRackSystem Structure, PushBackCompositeSystem Composite) Resolve(
            PushBackDesign design, Func<PushBackDesign, PushBackSystem> resolveSide)
        {
            var sideA = PushBackSideConfiguration.ForA(design);
            var sideB = PushBackSideConfiguration.ForB(design);
            var layout = PushBackCompositeStructure.Layout(sideA, sideB, design.Composite);

            var localA = ResolveSide(design, sideA, layout, PushBackSide.A, resolveSide, null);
            var localB = ResolveSide(design, sideB, layout, PushBackSide.B, resolveSide, null);

            var compositeDesign = PushBackCompositeStructure.Compose(
                design, sideA, sideB, layout, localA?.Structure, localB?.Structure);
            var structure = structureResolver.Resolve(compositeDesign).System;

            // La ALTURA de poste es del RACK, no de un lado: hay una sola estructura y sus cabeceras miden lo mismo.
            // Con niveles distintos entre lados, la demanda mayor la fija la estructura compuesta, asi que las dos
            // sub-estructuras se vuelven a resolver con esa altura. Sin este paso, los cortes de un lado dibujarian
            // postes mas cortos que los que el rack tiene realmente.
            var sharedHeight = structure.Fronts.Count > 0 ? structure.Fronts.Max(front => front.Height) : (double?)null;
            if (sharedHeight.HasValue && sharedHeight.Value > 0.0)
            {
                localA = ResolveSide(design, sideA, layout, PushBackSide.A, resolveSide, sharedHeight);
                localB = ResolveSide(design, sideB, layout, PushBackSide.B, resolveSide, sharedHeight);
            }

            var composite = new PushBackCompositeSystem
            {
                Gap = layout.Gap,
                CentralSeparator = layout.CentralSeparator,
                GapPosition = layout.GapPosition
            };

            composite.SideA = BuildSide(PushBackSide.A, sideA, layout, localA, structure);
            composite.SideB = BuildSide(PushBackSide.B, sideB, layout, localB, structure);

            var gapModule = structure.Modules.FirstOrDefault(module =>
                string.Equals(module?.ModuleId, PushBackCompositeStructure.GapModuleId, StringComparison.Ordinal));
            composite.GapStartX = gapModule?.StartX ?? composite.SideA.InnerX;
            composite.GapEndX = gapModule?.EndX ?? composite.SideA.InnerX;

            BuildCells(design, sideA, sideB, layout, localA, localB, structure, composite);

            return (structure, composite);
        }

        /// <summary>
        /// La sub-estructura de un lado, resuelta en su marco local por el MISMO camino que un Push Back de un solo
        /// sentido. Un lado ausente no resuelve nada: no aporta estructura ni demanda.
        /// </summary>
        private PushBackSystem ResolveSide(
            PushBackDesign design,
            PushBackSideConfiguration side,
            PushBackCompositeLayout layout,
            PushBackSide which,
            Func<PushBackDesign, PushBackSystem> resolveSide,
            double? sharedHeight)
        {
            if (!side.IsPresent)
            {
                return null;
            }

            var modules = PushBackCompositeStructure.StoredSideModules(design, layout, which);
            var localDesign = new PushBackDesign
            {
                Structure = PushBackCompositeStructure.SideStructuralDesign(design, side, modules),
                LegacyHighEndBeamPeralte = side.LegacyHighEndBeamPeralte
            };
            if (sharedHeight.HasValue)
            {
                localDesign.Structure.ManualHeaderHeightOverride = sharedHeight;
            }

            // La configuracion Push Back del lado viaja SOLO para las ranuras presentes, en el mismo orden en que
            // SideStructuralDesign las apilo: asi el resolver de un lado sigue siendo el de siempre. La rejilla de
            // topes se RE-INDEXA a la vez, porque esta escrita en ranuras compartidas y el sistema local numera solo
            // las presentes: sin ese puente, un rack con una ranura ausente desactivaria el tope equivocado.
            var localIndexBySlot = new Dictionary<int, int>();
            for (var slot = 0; slot < side.SlotCount; slot++)
            {
                if (side.Front(slot) == null)
                {
                    continue;
                }

                localIndexBySlot[slot] = localDesign.Fronts.Count;
                localDesign.Fronts.Add(side.Config(slot)?.DeepCopy() ?? new PushBackFrontConfig());
            }

            localDesign.RearTope = ReindexTope(side.RearTope, localIndexBySlot);
            return resolveSide(localDesign);
        }

        /// <summary>
        /// La rejilla de topes de un lado, re-escrita de ranuras COMPARTIDAS a indices del sistema LOCAL. Una
        /// desactivacion de una ranura que no existe en el lado se descarta: no tiene tope al que apagar, y
        /// arrastrarla movería la desactivacion a otra ranura.
        /// </summary>
        private static PushBackRearTopeConfig ReindexTope(
            PushBackRearTopeConfig source, IReadOnlyDictionary<int, int> localIndexBySlot)
        {
            var result = new PushBackRearTopeConfig
            {
                Saque = source?.Saque ?? PushBackDefaults.RearTopeSaque,
                PieceId = source?.PieceId
            };

            foreach (var cell in source?.OffCells ?? new List<RackCad.Domain.Systems.Selective.SelectiveGridCell>())
            {
                if (cell != null && localIndexBySlot.TryGetValue(cell.Frente, out var local))
                {
                    result.Disable(local, cell.Level);
                }
            }

            return result;
        }

        private static PushBackSideSystem BuildSide(
            PushBackSide which,
            PushBackSideConfiguration configuration,
            PushBackCompositeLayout layout,
            PushBackSystem local,
            DynamicRackSystem structure)
        {
            var side = new PushBackSideSystem
            {
                Side = which,
                IsPresent = configuration.IsPresent && local != null,
                Local = local,
                RearTope = configuration.RearTope?.DeepCopy() ?? new PushBackRearTopeConfig(),
                ProposedStructure = configuration.ProposedStructure(),
                StructureOverride = configuration.StructureOverride,
                EffectiveStructure = configuration.EffectiveStructure(),
                FirstPosition = which == PushBackSide.A ? 1 : layout.FirstPositionB,
                LastPosition = which == PushBackSide.A ? layout.PositionsA : layout.TotalPositions
            };

            var total = structure?.TotalLength ?? 0.0;
            var localIndex = 0;
            var slots = Math.Max(configuration.SlotCount, structure?.Fronts.Count ?? 0);
            for (var slot = 0; slot < slots; slot++)
            {
                var present = side.IsPresent && configuration.Front(slot) != null;
                side.LocalIndexBySlot.Add(present ? localIndex : -1);
                var localFront = present && local?.Structure != null && localIndex < local.Structure.Fronts.Count
                    ? local.Structure.Fronts[localIndex]
                    : null;
                side.Fronts.Add(localFront);
                side.ResolvedFronts.Add(BuildResolvedFront(local, present ? localIndex : -1, present));
                if (present)
                {
                    localIndex++;
                }
            }

            // El extremo EXTERIOR de A es el origen; el de B, el final del rack. El INTERIOR de cada lado es su
            // linea de postes terminal: son DOS lineas distintas, tambien con hueco 0.
            var localLength = local?.Structure?.TotalLength ?? 0.0;
            side.OuterX = which == PushBackSide.A ? 0.0 : total;
            side.InnerX = which == PushBackSide.A ? localLength : total - localLength;
            return side;
        }

        private static PushBackResolvedFront BuildResolvedFront(PushBackSystem local, int localIndex, bool present)
        {
            var resolved = new PushBackResolvedFront { IsPresent = present };
            if (!present || local == null || localIndex < 0 || localIndex >= local.HighEndBeams.Count)
            {
                return resolved;
            }

            var source = local.HighEndBeams[localIndex];
            resolved.DefaultPalletsDeep = source.DefaultPalletsDeep;
            foreach (var peralte in source.HighEndBeamPeraltes)
            {
                resolved.HighEndBeamPeraltes.Add(peralte);
            }

            foreach (var deep in source.PalletsDeep)
            {
                resolved.PalletsDeep.Add(deep);
            }

            foreach (var draw in source.DrawPallets)
            {
                resolved.DrawPallets.Add(draw);
            }

            return resolved;
        }

        /// <summary>
        /// La rejilla de celdas: topologia y sentido por (ranura, nivel), con las dos magnitudes de capacidad ya
        /// medidas. Una topologia que pide un lado que no existe en esa celda se DEGRADA de forma explicita y
        /// declarada (no en silencio) al unico lado disponible, o se marca imposible si no hay ninguno.
        /// </summary>
        private static void BuildCells(
            PushBackDesign design,
            PushBackSideConfiguration sideA,
            PushBackSideConfiguration sideB,
            PushBackCompositeLayout layout,
            PushBackSystem localA,
            PushBackSystem localB,
            DynamicRackSystem structure,
            PushBackCompositeSystem composite)
        {
            var intent = design.Composite ?? new PushBackCompositeDesign();
            var slots = structure?.Fronts.Count ?? 0;
            for (var slot = 0; slot < slots; slot++)
            {
                var levelsA = sideA.Levels(slot);
                var levelsB = sideB.Levels(slot);
                var levels = Math.Max(levelsA, levelsB);
                for (var level = 0; level < levels; level++)
                {
                    var hasA = level < levelsA;
                    var hasB = level < levelsB;
                    var topology = Degrade(intent.TopologyAt(slot, level), hasA, hasB);
                    var direction = intent.DirectionAt(slot, level);
                    var cell = new PushBackResolvedCell
                    {
                        FrontIndex = slot,
                        LevelNumber = level + 1,
                        Topology = topology,
                        Direction = direction
                    };

                    if (!hasA && !hasB)
                    {
                        cell.DisabledReason = "La celda no existe en ninguno de los dos lados.";
                        composite.Cells.Add(cell);
                        continue;
                    }

                    var requiredA = hasA
                        ? PushBackBedSpan.Required(localA?.Structure, sideA.EffectiveDeep(slot, level))
                        : 0.0;
                    var requiredB = hasB
                        ? PushBackBedSpan.Required(localB?.Structure, sideB.EffectiveDeep(slot, level))
                        : 0.0;
                    var availableA = hasA
                        ? PushBackBedSpan.Available(localA?.Structure, sideA.SlotStructure(slot))
                        : 0.0;
                    var availableB = hasB
                        ? PushBackBedSpan.Available(localB?.Structure, sideB.SlotStructure(slot))
                        : 0.0;

                    switch (topology)
                    {
                        case PushBackCellTopology.SoloA:
                            cell.RequiredBedLength = requiredA;
                            cell.AvailableBedSpan = availableA;
                            break;
                        case PushBackCellTopology.SoloB:
                            cell.RequiredBedLength = requiredB;
                            cell.AvailableBedSpan = availableB;
                            break;
                        case PushBackCellTopology.Encontradas:
                            // Dos camas fisicas: la limitante es la que peor lo tiene.
                            cell.RequiredBedLength = Math.Max(requiredA, requiredB);
                            cell.AvailableBedSpan = Math.Min(availableA, availableB);
                            break;
                        case PushBackCellTopology.Corrida:
                            // UNA cama que atraviesa A + hueco + B. La demanda son los fondos de los dos lados; el
                            // hueco NO es demanda pero SI es longitud disponible, y por eso puede volverla valida.
                            cell.RequiredBedLength = requiredA + requiredB;
                            cell.AvailableBedSpan = availableA + layout.Gap + availableB;
                            break;
                    }

                    cell.DisabledReason = PushBackBedSpan.DisabledReason(cell.RequiredBedLength, cell.AvailableBedSpan);
                    composite.Cells.Add(cell);
                }
            }
        }

        /// <summary>
        /// La topologia que una celda puede REALMENTE tener con los lados que existen en ella. Un nivel que solo
        /// existe en B no puede ser «encontradas» ni «corrida»: es «solo B». La degradacion es explicita y no
        /// modifica la intencion almacenada — esa queda DORMANTE y reaparece en cuanto el otro lado vuelve a tener
        /// el nivel.
        /// </summary>
        public static PushBackCellTopology Degrade(PushBackCellTopology stored, bool hasA, bool hasB)
        {
            if (hasA && hasB)
            {
                return stored;
            }

            if (hasA)
            {
                return PushBackCellTopology.SoloA;
            }

            return hasB ? PushBackCellTopology.SoloB : stored;
        }
    }
}
