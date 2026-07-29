using System.Collections.Generic;
using RackCad.Application.StructuralSections;

namespace RackCad.StructuralSections.Import
{
    /// <summary>
    /// Turns one selected AISC row into a <see cref="StructuralSectionDefinition"/>.
    ///
    /// It COPIES; it never derives. No dimension is inferred from a designation, no missing value is
    /// substituted by a computed one, and no radius the source does not publish is invented — I-36A preserves
    /// data and its fidelity, and I-36B is where a documented parametric geometry may be built on top.
    ///
    /// A value the source marks as not applicable becomes <c>null</c>, and a value a family REQUIRES but the
    /// row lacks becomes a fatal rejection with its row number: no selected row may be dropped in silence.
    /// </summary>
    public static class AiscRowMapper
    {
        public static StructuralSectionDefinition Map(
            AiscColumnMap columns,
            IReadOnlyDictionary<int, string> row,
            int rowNumber,
            StructuralSectionFamily family,
            StructuralSectionSource source)
        {
            var edi = columns.Text(row, AiscColumnMap.EdiNomenclature);
            var label = columns.Text(row, AiscColumnMap.ManualLabel);

            if (string.IsNullOrEmpty(edi))
            {
                throw new AiscRowRejectedException(rowNumber, null, "la designacion EDI esta vacia.");
            }

            if (string.IsNullOrEmpty(label))
            {
                throw new AiscRowRejectedException(rowNumber, edi, "la etiqueta del manual AISC esta vacia.");
            }

            // The id is stamped with the authority the SOURCE declares, never with a hard-coded prefix.
            if (!StructuralSectionId.TryCreate(source.IdNamespace, family, edi, out var sectionId))
            {
                throw new AiscRowRejectedException(
                    rowNumber, edi,
                    "la designacion EDI no puede normalizarse a un id valido bajo el namespace '" +
                    source.IdNamespace + "'.");
            }

            var weight = columns.Number(row, AiscColumnMap.Weight, rowNumber);

            if (!weight.HasValue || weight.Value <= 0)
            {
                throw new AiscRowRejectedException(rowNumber, edi, "el peso lineal falta o no es positivo.");
            }

            var area = columns.Number(row, AiscColumnMap.Area, rowNumber);

            if (!area.HasValue || area.Value <= 0)
            {
                throw new AiscRowRejectedException(rowNumber, edi, "el area falta o no es positiva.");
            }

            var identity = new StructuralSectionIdentity
            {
                SectionId = sectionId,
                Family = family,
                EdiDesignation = edi,
                ManualLabel = label,
                SourceId = source.SourceId,
                SourceRevision = source.Revision
            };

            return new StructuralSectionDefinition
            {
                Identity = identity,
                WeightPerLength = weight.Value,
                NativeUnitSystem = source.NativeUnitSystem,
                Dimensions = MapDimensions(columns, row, rowNumber, family, edi),
                Properties = MapProperties(columns, row, rowNumber, family, area.Value),
                SourceSpecialNote = MapSpecialNote(columns, row, rowNumber, family, edi),
                IsEnabled = true,
                MaterialGrade = null
            };
        }

        private static bool? MapSpecialNote(
            AiscColumnMap columns,
            IReadOnlyDictionary<int, string> row,
            int rowNumber,
            StructuralSectionFamily family,
            string edi)
        {
            if (family != StructuralSectionFamily.W)
            {
                return null;
            }

            var flag = columns.Text(row, AiscColumnMap.SpecialNoteFlag);

            switch (flag)
            {
                case null: return null;
                case "T": return true;
                case "F": return false;
                default:
                    throw new AiscRowRejectedException(
                        rowNumber, edi, "el indicador T_F vale '" + flag + "' y solo admite 'T' o 'F'.");
            }
        }

        private static IStructuralSectionDimensions MapDimensions(
            AiscColumnMap columns,
            IReadOnlyDictionary<int, string> row,
            int rowNumber,
            StructuralSectionFamily family,
            string edi)
        {
            switch (family)
            {
                case StructuralSectionFamily.W:
                    return new WSectionDimensions
                    {
                        Depth = Required(columns, row, rowNumber, edi, "d"),
                        DetailingDepth = columns.Number(row, "ddet", rowNumber),
                        FlangeWidth = Required(columns, row, rowNumber, edi, "bf"),
                        DetailingFlangeWidth = columns.Number(row, "bfdet", rowNumber),
                        WebThickness = Required(columns, row, rowNumber, edi, "tw"),
                        DetailingWebThickness = columns.Number(row, "twdet", rowNumber),
                        HalfDetailingWebThickness = columns.Number(row, "twdet/2", rowNumber),
                        FlangeThickness = Required(columns, row, rowNumber, edi, "tf"),
                        DetailingFlangeThickness = columns.Number(row, "tfdet", rowNumber),
                        KDesign = columns.Number(row, "kdes", rowNumber),
                        KDetailing = columns.Number(row, "kdet", rowNumber),
                        K1 = columns.Number(row, "k1", rowNumber),
                        DistanceBetweenFilletToes = columns.Number(row, "T", rowNumber),
                        WorkableGageInner = columns.Number(row, "WGi", rowNumber),
                        WorkableGageOuter = columns.Number(row, "WGo", rowNumber)
                    };

                case StructuralSectionFamily.S:
                    return new SSectionDimensions
                    {
                        Depth = Required(columns, row, rowNumber, edi, "d"),
                        DetailingDepth = columns.Number(row, "ddet", rowNumber),
                        FlangeWidth = Required(columns, row, rowNumber, edi, "bf"),
                        DetailingFlangeWidth = columns.Number(row, "bfdet", rowNumber),
                        WebThickness = Required(columns, row, rowNumber, edi, "tw"),
                        DetailingWebThickness = columns.Number(row, "twdet", rowNumber),
                        HalfDetailingWebThickness = columns.Number(row, "twdet/2", rowNumber),
                        FlangeThickness = Required(columns, row, rowNumber, edi, "tf"),
                        DetailingFlangeThickness = columns.Number(row, "tfdet", rowNumber),
                        KDesign = columns.Number(row, "kdes", rowNumber),
                        KDetailing = columns.Number(row, "kdet", rowNumber),
                        K1 = columns.Number(row, "k1", rowNumber),
                        DistanceBetweenFilletToes = columns.Number(row, "T", rowNumber),
                        WorkableGageInner = columns.Number(row, "WGi", rowNumber),
                        WorkableGageOuter = columns.Number(row, "WGo", rowNumber)
                    };

                case StructuralSectionFamily.HssRectangular:
                    return new HssRectangularSectionDimensions
                    {
                        OverallDepth = Required(columns, row, rowNumber, edi, "Ht"),
                        OverallWidth = Required(columns, row, rowNumber, edi, "B"),
                        FlatDepth = columns.Number(row, "h", rowNumber),
                        FlatWidth = columns.Number(row, "b", rowNumber),
                        NominalThickness = Required(columns, row, rowNumber, edi, "tnom"),
                        DesignThickness = Required(columns, row, rowNumber, edi, "tdes")
                    };

                case StructuralSectionFamily.Channel:
                    return new ChannelSectionDimensions
                    {
                        Depth = Required(columns, row, rowNumber, edi, "d"),
                        DetailingDepth = columns.Number(row, "ddet", rowNumber),
                        FlangeWidth = Required(columns, row, rowNumber, edi, "bf"),
                        DetailingFlangeWidth = columns.Number(row, "bfdet", rowNumber),
                        WebThickness = Required(columns, row, rowNumber, edi, "tw"),
                        DetailingWebThickness = columns.Number(row, "twdet", rowNumber),
                        HalfDetailingWebThickness = columns.Number(row, "twdet/2", rowNumber),
                        FlangeThickness = Required(columns, row, rowNumber, edi, "tf"),
                        DetailingFlangeThickness = columns.Number(row, "tfdet", rowNumber),
                        KDesign = columns.Number(row, "kdes", rowNumber),
                        KDetailing = columns.Number(row, "kdet", rowNumber),
                        CentroidX = columns.Number(row, "x", rowNumber),
                        ShearCenterX = columns.Number(row, "eo", rowNumber),
                        PlasticNeutralAxisX = columns.Number(row, "xp", rowNumber),
                        DistanceBetweenFilletToes = columns.Number(row, "T", rowNumber),
                        WorkableGageInner = columns.Number(row, "WGi", rowNumber)
                    };

                case StructuralSectionFamily.Angle:
                    return new AngleSectionDimensions
                    {
                        ShortLeg = Required(columns, row, rowNumber, edi, "d"),
                        LongLeg = Required(columns, row, rowNumber, edi, "b"),
                        Thickness = Required(columns, row, rowNumber, edi, "t"),
                        KDesign = columns.Number(row, "kdes", rowNumber),
                        KDetailing = columns.Number(row, "kdet", rowNumber),
                        CentroidX = columns.Number(row, "x", rowNumber),
                        CentroidY = columns.Number(row, "y", rowNumber),
                        PlasticNeutralAxisX = columns.Number(row, "xp", rowNumber),
                        PlasticNeutralAxisY = columns.Number(row, "yp", rowNumber)
                    };

                default:
                    throw new AiscRowRejectedException(rowNumber, edi, "familia desconocida.");
            }
        }

        private static StructuralSectionProperties MapProperties(
            AiscColumnMap columns,
            IReadOnlyDictionary<int, string> row,
            int rowNumber,
            StructuralSectionFamily family,
            double area)
        {
            var isW = family == StructuralSectionFamily.W;
            var isHss = family == StructuralSectionFamily.HssRectangular;
            var isChannel = family == StructuralSectionFamily.Channel;
            var isAngle = family == StructuralSectionFamily.Angle;
            var isS = family == StructuralSectionFamily.S;
            // S publishes the same Design Guide 9 block as W and C, complete in all 28 rows. Measured, not
            // assumed by analogy: see the I-36D evidence.
            var hasWarpingBlock = isW || isChannel || isS;

            return new StructuralSectionProperties
            {
                Area = area,
                Ix = columns.Number(row, "Ix", rowNumber),
                Zx = columns.Number(row, "Zx", rowNumber),
                Sx = columns.Number(row, "Sx", rowNumber),
                Rx = columns.Number(row, "rx", rowNumber),
                Iy = columns.Number(row, "Iy", rowNumber),
                Zy = columns.Number(row, "Zy", rowNumber),
                Sy = columns.Number(row, "Sy", rowNumber),
                Ry = columns.Number(row, "ry", rowNumber),
                J = columns.Number(row, "J", rowNumber),
                Cw = isHss ? null : columns.Number(row, "Cw", rowNumber),
                HssTorsionalConstant = isHss ? columns.Number(row, "C", rowNumber) : null,
                Wno = hasWarpingBlock ? columns.Number(row, "Wno", rowNumber) : null,
                Sw1 = hasWarpingBlock ? columns.Number(row, "Sw1", rowNumber) : null,
                Sw2 = isChannel ? columns.Number(row, "Sw2", rowNumber) : null,
                Sw3 = isChannel ? columns.Number(row, "Sw3", rowNumber) : null,
                Qf = hasWarpingBlock ? columns.Number(row, "Qf", rowNumber) : null,
                Qw = hasWarpingBlock ? columns.Number(row, "Qw", rowNumber) : null,
                Ro = isChannel || isAngle ? columns.Number(row, "ro", rowNumber) : null,
                FlexuralConstantH = isChannel || isAngle ? columns.Number(row, "H", rowNumber) : null,
                Rts = hasWarpingBlock ? columns.Number(row, "rts", rowNumber) : null,
                Ho = hasWarpingBlock ? columns.Number(row, "ho", rowNumber) : null,
                Iz = isAngle ? columns.Number(row, "Iz", rowNumber) : null,
                Rz = isAngle ? columns.Number(row, "rz", rowNumber) : null,
                Sz = isAngle ? columns.Number(row, "Sz", rowNumber) : null,
                TanAlpha = isAngle ? columns.Number(row, AiscColumnMap.TanAlpha, rowNumber) : null,
                Iw = isAngle ? columns.Number(row, "Iw", rowNumber) : null,
                ZA = isAngle ? columns.Number(row, "zA", rowNumber) : null,
                ZB = isAngle ? columns.Number(row, "zB", rowNumber) : null,
                ZC = isAngle ? columns.Number(row, "zC", rowNumber) : null,
                WA = isAngle ? columns.Number(row, "wA", rowNumber) : null,
                WB = isAngle ? columns.Number(row, "wB", rowNumber) : null,
                WC = isAngle ? columns.Number(row, "wC", rowNumber) : null,
                SwA = isAngle ? columns.Number(row, "SwA", rowNumber) : null,
                SwB = isAngle ? columns.Number(row, "SwB", rowNumber) : null,
                SwC = isAngle ? columns.Number(row, "SwC", rowNumber) : null,
                SzA = isAngle ? columns.Number(row, "SzA", rowNumber) : null,
                SzB = isAngle ? columns.Number(row, "SzB", rowNumber) : null,
                SzC = isAngle ? columns.Number(row, "SzC", rowNumber) : null,
                PA = isHss ? null : columns.Number(row, "PA", rowNumber),
                PA2 = isAngle ? columns.Number(row, "PA2", rowNumber) : null,
                PB = isHss ? null : columns.Number(row, "PB", rowNumber),
                PC = hasWarpingBlock ? columns.Number(row, "PC", rowNumber) : null,
                PD = hasWarpingBlock ? columns.Number(row, "PD", rowNumber) : null
            };
        }

        private static double Required(
            AiscColumnMap columns,
            IReadOnlyDictionary<int, string> row,
            int rowNumber,
            string edi,
            string header)
        {
            var value = columns.Number(row, header, rowNumber);

            if (!value.HasValue)
            {
                throw new AiscRowRejectedException(
                    rowNumber, edi, "falta la dimension obligatoria '" + header + "' de su familia.");
            }

            return value.Value;
        }
    }
}
