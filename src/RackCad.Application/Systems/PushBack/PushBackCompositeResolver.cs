using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.RackFrames;
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
    /// <item>la sub-estructura de cada lado en su MARCO LOCAL, delegando integramente en el resolver dinamico. Toda
    /// ranura del rack existe en las dos sub-estructuras —la que no pertenece a un lado va EN BLANCO—, asi que la
    /// retícula transversal es UNA sola y el indice de ranura significa lo mismo en todas partes;</item>
    /// <item>la ESTRUCTURA FISICA UNICA (A + hueco + B invertido) sobre esa retícula;</item>
    /// <item>la rejilla de celdas, con sus CAMAS fisicas y la capacidad de cada una medida por separado.</item>
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
        /// <summary>
        /// Altura de referencia de la receta estandar usada SOLO para reconciliar modulos. No llega al dibujo: el
        /// resolver reconstruye la configuracion calculada de cada cabecera con la altura real del rack.
        /// </summary>
        private const double StandardReferenceHeight = 100.0;

        private readonly RackCatalog catalog;
        private readonly DynamicRackSystemResolver structureResolver;
        private readonly DynamicRackSystemBuilder structureBuilder;

        public PushBackCompositeResolver(RackCatalog catalog)
        {
            this.catalog = catalog ?? new RackCatalog();
            structureResolver = new DynamicRackSystemResolver(this.catalog);
            structureBuilder = new DynamicRackSystemBuilder(this.catalog);
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

            var localA = ResolveSide(design, sideA, sideB, layout, PushBackSide.A, resolveSide, null);
            var localB = ResolveSide(design, sideB, sideA, layout, PushBackSide.B, resolveSide, null);

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
                localA = ResolveSide(design, sideA, sideB, layout, PushBackSide.A, resolveSide, sharedHeight);
                localB = ResolveSide(design, sideB, sideA, layout, PushBackSide.B, resolveSide, sharedHeight);
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
            PushBackSideConfiguration other,
            PushBackCompositeLayout layout,
            PushBackSide which,
            Func<PushBackDesign, PushBackSystem> resolveSide,
            double? sharedHeight)
        {
            if (!side.IsPresent)
            {
                return null;
            }

            var modules = ReconcileSideModules(design, layout, which);
            var localDesign = new PushBackDesign
            {
                Structure = PushBackCompositeStructure.SideStructuralDesign(design, side, other, modules),
                LegacyHighEndBeamPeralte = side.LegacyHighEndBeamPeralte,
                RearTope = side.RearTope?.DeepCopy() ?? new PushBackRearTopeConfig()
            };
            if (sharedHeight.HasValue)
            {
                localDesign.Structure.ManualHeaderHeightOverride = sharedHeight;
            }

            // La configuracion Push Back del lado viaja por RANURA, alineada con los frentes que
            // SideStructuralDesign acaba de apilar — que son TODOS, con los ausentes en blanco. Por eso el indice de
            // ranura ES el indice local, y la rejilla de topes no necesita ninguna traduccion.
            for (var slot = 0; slot < localDesign.Structure.Fronts.Count; slot++)
            {
                localDesign.Fronts.Add(side.Config(slot)?.DeepCopy() ?? new PushBackFrontConfig());
            }

            return resolveSide(localDesign);
        }

        /// <summary>
        /// Los modulos de un lado, RECONCILIADOS fisicamente contra la receta estandar de su profundidad efectiva.
        ///
        /// <para>
        /// Es lo que conserva I-40 cuando la estructura crece o encoge. La regla anterior —«si el conteo no coincide,
        /// reconstruir todo»— tiraba toda cabecera personalizada en cuanto se movia un fondo. Aqui una pieza que
        /// sigue existiendo en la misma posicion contada desde el extremo exterior del lado, y con el mismo caracter
        /// fisico, conserva su ModuleId y su configuracion; una pieza nueva nace calculada; una que desaparecio no
        /// deja rastro.
        /// </para>
        /// </summary>
        private IReadOnlyList<DynamicRackModuleDesign> ReconcileSideModules(
            PushBackDesign design, PushBackCompositeLayout layout, PushBackSide which)
        {
            var positions = which == PushBackSide.A ? layout.PositionsA : layout.PositionsB;
            if (positions < PushBackCellDepth.MinimumPalletsDeep)
            {
                return null;
            }

            var stored = PushBackCompositeStructure.StoredSideModules(design, layout, which);
            if (stored != null && stored.Count == positions)
            {
                return stored;   // nada se movio: la secuencia almacenada describe esta estructura tal cual
            }

            var pallet = design?.Structure?.Pallet;
            if (pallet == null || pallet.Depth <= 0.0)
            {
                return null;
            }

            // La RECETA estandar de esa profundidad, construida por el builder dinamico: el patron de cabeceras y
            // separadores no se reescribe aqui, se le pregunta.
            var standard = structureBuilder.BuildDefault(
                pallet,
                positions,
                RackFrameTemplateCatalog.Default,
                string.IsNullOrWhiteSpace(design.Structure.HeaderPostCatalogId)
                    ? catalog.Defaults?.Post
                    : design.Structure.HeaderPostCatalogId,
                StandardReferenceHeight,
                design.Structure.PostPeralte);

            return PushBackCompositeStructure.Reconcile(stored, standard.Modules.ToList());
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
            var slots = Math.Max(configuration.SlotCount, structure?.Fronts.Count ?? 0);
            for (var slot = 0; slot < slots; slot++)
            {
                // Toda ranura existe en la sub-estructura del lado (las ausentes, en blanco), asi que el indice local
                // ES el de la ranura: el puente que antes hacia falta construir ya no existe.
                var present = side.IsPresent && configuration.Front(slot) != null;
                side.LocalIndexBySlot.Add(slot);
                var localFront = local?.Structure != null && slot < local.Structure.Fronts.Count
                    ? local.Structure.Fronts[slot]
                    : null;
                side.Fronts.Add(present ? localFront : null);
                side.ResolvedFronts.Add(BuildResolvedFront(local, slot, present));
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
        /// La rejilla de celdas: topologia, sentido y las CAMAS fisicas de cada una, con la capacidad de cada cama
        /// medida por separado. Una topologia que pide un lado que no existe en esa celda se DEGRADA de forma
        /// explicita al unico lado disponible; la intencion almacenada no se toca y reaparece intacta.
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
            var total = structure?.TotalLength ?? 0.0;
            for (var slot = 0; slot < slots; slot++)
            {
                var levelsA = sideA.Levels(slot);
                var levelsB = sideB.Levels(slot);
                var levels = Math.Max(levelsA, levelsB);
                for (var level = 0; level < levels; level++)
                {
                    var hasA = level < levelsA;
                    var hasB = level < levelsB;
                    var cell = new PushBackResolvedCell
                    {
                        FrontIndex = slot,
                        LevelNumber = level + 1,
                        Topology = Degrade(intent.TopologyAt(slot, level), hasA, hasB),
                        Direction = intent.DirectionAt(slot, level)
                    };

                    if (!hasA && !hasB)
                    {
                        cell.DisabledReason = "La celda no existe en ninguno de los dos lados.";
                        composite.Cells.Add(cell);
                        continue;
                    }

                    switch (cell.Topology)
                    {
                        case PushBackCellTopology.SoloA:
                            cell.Beds.Add(SideBed(sideA, localA, slot, level, PushBackSide.A));
                            break;
                        case PushBackCellTopology.SoloB:
                            cell.Beds.Add(SideBed(sideB, localB, slot, level, PushBackSide.B));
                            break;
                        case PushBackCellTopology.Encontradas:
                            // DOS camas fisicas INDEPENDIENTES: cada una se mide contra SU propia estructura. Medir
                            // la demanda de una contra el espacio de la otra inventaba errores de capacidad — es lo
                            // que hacia que un rack de 4 fondos en A y 8 en B se declarara imposible.
                            cell.Beds.Add(SideBed(sideA, localA, slot, level, PushBackSide.A));
                            cell.Beds.Add(SideBed(sideB, localB, slot, level, PushBackSide.B));
                            break;
                        case PushBackCellTopology.Corrida:
                            cell.Beds.Add(CorridaBed(sideA, sideB, layout, slot, level, cell.Direction, structure, total));
                            break;
                    }

                    cell.DisabledReason = cell.Beds
                        .Where(bed => bed != null && !bed.IsValid)
                        .Select(bed => bed.DisabledReason)
                        .FirstOrDefault();
                    composite.Cells.Add(cell);
                }
            }
        }

        /// <summary>Una cama de UN lado: su demanda contra la estructura de ESE lado, nunca contra la del otro.</summary>
        private static PushBackCellBed SideBed(
            PushBackSideConfiguration side, PushBackSystem local, int slot, int level, PushBackSide which)
        {
            var deep = side.EffectiveDeep(slot, level);
            var bed = new PushBackCellBed
            {
                LowSide = which,
                HighSide = which,
                DemandPositions = deep,
                RequiredBedLength = PushBackBedSpan.Required(local?.Structure, deep),
                AvailableBedSpan = PushBackBedSpan.Available(local?.Structure, side.SlotStructure(slot))
            };
            bed.DisabledReason = PushBackBedSpan.DisabledReason(
                bed.RequiredBedLength, bed.AvailableBedSpan, which, slot, level + 1);
            return bed;
        }

        /// <summary>
        /// La cama CORRIDA: UNA sola pieza anclada en el extremo ALTO que se desarrolla hacia el BAJO exactamente lo
        /// que su demanda exige.
        ///
        /// <para>
        /// Su longitud fisica NO es la del rack: la estructura sobrante puede existir porque OTROS niveles o frentes
        /// la necesitan, y una corrida corta simplemente no la usa. La demanda son los fondos que los dos lados
        /// declaran para esa celda —medidos SIN el hueco, que es estructura—; la capacidad, la profundidad completa
        /// que la estructura pone a disposicion, hueco INCLUIDO. Por eso un hueco mayor puede volver valida una cama
        /// sin alargarla ni cambiar su demanda.
        /// </para>
        /// </summary>
        private static PushBackCellBed CorridaBed(
            PushBackSideConfiguration sideA,
            PushBackSideConfiguration sideB,
            PushBackCompositeLayout layout,
            int slot,
            int level,
            PushBackRunDirection direction,
            DynamicRackSystem structure,
            double total)
        {
            var forward = direction == PushBackRunDirection.AToB;
            var demand = sideA.EffectiveDeep(slot, level) + sideB.EffectiveDeep(slot, level);
            var bed = new PushBackCellBed
            {
                LowSide = forward ? PushBackSide.A : PushBackSide.B,
                HighSide = forward ? PushBackSide.B : PushBackSide.A,
                DemandPositions = demand,
                RequiredBedLength = PushBackBedSpan.DemandLength(structure, demand, PushBackBedAnchor.High),
                AvailableBedSpan = total
            };
            bed.DisabledReason = PushBackBedSpan.DisabledReason(
                bed.RequiredBedLength, bed.AvailableBedSpan, bed.LowSide, slot, level + 1);
            return bed;
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
