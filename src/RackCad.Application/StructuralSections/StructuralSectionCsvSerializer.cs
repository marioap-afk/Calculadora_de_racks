using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RackCad.Application.StructuralSections
{
    /// <summary>
    /// Converts a <see cref="StructuralSectionDefinition"/> to and from a row of its family file.
    ///
    /// Both directions live together so they cannot drift: the importer writes with
    /// <see cref="ToCells"/>, the provider reads with <see cref="FromRow"/>, and a column added to only one of
    /// them fails to compile instead of producing a file nobody can load.
    ///
    /// Numbers are written with round-trip precision in invariant culture, which on .NET is the SHORTEST text
    /// that parses back to the very same double. That keeps the file readable (<c>16.1</c>, not
    /// <c>16.100000000000001</c>) while remaining exactly the value the source stored.
    /// </summary>
    public static class StructuralSectionCsvSerializer
    {
        public static string[] ToCells(StructuralSectionDefinition section)
        {
            if (section == null)
            {
                throw new ArgumentNullException(nameof(section));
            }

            var values = new Dictionary<string, string>(StringComparer.Ordinal);
            var identity = section.Identity;

            values[StructuralSectionCsvSchema.SectionId] = identity.SectionId.Value;
            values[StructuralSectionCsvSchema.Family] = StructuralSectionFamilies.ToToken(identity.Family);
            values[StructuralSectionCsvSchema.EdiDesignation] = identity.EdiDesignation;
            values[StructuralSectionCsvSchema.ManualLabel] = identity.ManualLabel;
            values[StructuralSectionCsvSchema.SourceId] = identity.SourceId;
            values[StructuralSectionCsvSchema.SourceRevision] = identity.SourceRevision;
            values[StructuralSectionCsvSchema.WeightPerLength] = Number(section.WeightPerLength);
            values[StructuralSectionCsvSchema.NativeUnitSystem] =
                StructuralSectionUnitSystems.ToToken(section.NativeUnitSystem);

            var properties = section.Properties ?? StructuralSectionProperties.Empty;

            values["A"] = Number(properties.Area);
            values["Ix"] = Number(properties.Ix);
            values["Zx"] = Number(properties.Zx);
            values["Sx"] = Number(properties.Sx);
            values["rx"] = Number(properties.Rx);
            values["Iy"] = Number(properties.Iy);
            values["Zy"] = Number(properties.Zy);
            values["Sy"] = Number(properties.Sy);
            values["ry"] = Number(properties.Ry);
            values["J"] = Number(properties.J);

            switch (identity.Family)
            {
                case StructuralSectionFamily.W:
                    WriteW(values, section, properties);
                    break;
                case StructuralSectionFamily.HssRectangular:
                    WriteHss(values, section, properties);
                    break;
                case StructuralSectionFamily.Channel:
                    WriteChannel(values, section, properties);
                    break;
                case StructuralSectionFamily.Angle:
                    WriteAngle(values, section, properties);
                    break;
                case StructuralSectionFamily.S:
                    WriteS(values, section, properties);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(section), identity.Family, "Familia desconocida.");
            }

            var columns = StructuralSectionCsvSchema.ColumnsFor(identity.Family);
            var cells = new string[columns.Length];

            for (var i = 0; i < columns.Length; i++)
            {
                if (!values.TryGetValue(columns[i], out var value))
                {
                    throw new InvalidOperationException(
                        "El serializador no produjo la columna '" + columns[i] + "' de la familia " +
                        identity.Family + ".");
                }

                cells[i] = value ?? string.Empty;
            }

            if (values.Count != columns.Length)
            {
                var extra = values.Keys.Except(columns, StringComparer.Ordinal).ToArray();

                throw new InvalidOperationException(
                    "El serializador produjo columnas que el esquema no declara: " + string.Join(", ", extra) + ".");
            }

            return cells;
        }

        private static void WriteW(
            IDictionary<string, string> values,
            StructuralSectionDefinition section,
            StructuralSectionProperties properties)
        {
            var dimensions = Require<WSectionDimensions>(section);

            values["T_F"] = Boolean(section.SourceSpecialNote);
            values["d"] = Number(dimensions.Depth);
            values["ddet"] = Number(dimensions.DetailingDepth);
            values["bf"] = Number(dimensions.FlangeWidth);
            values["bfdet"] = Number(dimensions.DetailingFlangeWidth);
            values["tw"] = Number(dimensions.WebThickness);
            values["twdet"] = Number(dimensions.DetailingWebThickness);
            values["twdet_2"] = Number(dimensions.HalfDetailingWebThickness);
            values["tf"] = Number(dimensions.FlangeThickness);
            values["tfdet"] = Number(dimensions.DetailingFlangeThickness);
            values["kdes"] = Number(dimensions.KDesign);
            values["kdet"] = Number(dimensions.KDetailing);
            values["k1"] = Number(dimensions.K1);
            values["T"] = Number(dimensions.DistanceBetweenFilletToes);
            values["WGi"] = Number(dimensions.WorkableGageInner);
            values["WGo"] = Number(dimensions.WorkableGageOuter);

            values["Cw"] = Number(properties.Cw);
            values["Wno"] = Number(properties.Wno);
            values["Sw1"] = Number(properties.Sw1);
            values["Qf"] = Number(properties.Qf);
            values["Qw"] = Number(properties.Qw);
            values["rts"] = Number(properties.Rts);
            values["ho"] = Number(properties.Ho);
            values["PA"] = Number(properties.PA);
            values["PB"] = Number(properties.PB);
            values["PC"] = Number(properties.PC);
            values["PD"] = Number(properties.PD);
        }

        /// <summary>
        /// S writes the same block as W without <c>T_F</c>. It is a separate method rather than a call into
        /// <see cref="WriteW"/> because the two families have separate dimension types on purpose, and a
        /// shared writer would be the first place that distinction quietly dissolved.
        /// </summary>
        private static void WriteS(
            IDictionary<string, string> values,
            StructuralSectionDefinition section,
            StructuralSectionProperties properties)
        {
            var dimensions = Require<SSectionDimensions>(section);

            values["d"] = Number(dimensions.Depth);
            values["ddet"] = Number(dimensions.DetailingDepth);
            values["bf"] = Number(dimensions.FlangeWidth);
            values["bfdet"] = Number(dimensions.DetailingFlangeWidth);
            values["tw"] = Number(dimensions.WebThickness);
            values["twdet"] = Number(dimensions.DetailingWebThickness);
            values["twdet_2"] = Number(dimensions.HalfDetailingWebThickness);
            values["tf"] = Number(dimensions.FlangeThickness);
            values["tfdet"] = Number(dimensions.DetailingFlangeThickness);
            values["kdes"] = Number(dimensions.KDesign);
            values["kdet"] = Number(dimensions.KDetailing);
            values["k1"] = Number(dimensions.K1);
            values["T"] = Number(dimensions.DistanceBetweenFilletToes);
            values["WGi"] = Number(dimensions.WorkableGageInner);
            values["WGo"] = Number(dimensions.WorkableGageOuter);

            values["Cw"] = Number(properties.Cw);
            values["Wno"] = Number(properties.Wno);
            values["Sw1"] = Number(properties.Sw1);
            values["Qf"] = Number(properties.Qf);
            values["Qw"] = Number(properties.Qw);
            values["rts"] = Number(properties.Rts);
            values["ho"] = Number(properties.Ho);
            values["PA"] = Number(properties.PA);
            values["PB"] = Number(properties.PB);
            values["PC"] = Number(properties.PC);
            values["PD"] = Number(properties.PD);
        }

        private static void WriteHss(
            IDictionary<string, string> values,
            StructuralSectionDefinition section,
            StructuralSectionProperties properties)
        {
            var dimensions = Require<HssRectangularSectionDimensions>(section);

            values["Ht"] = Number(dimensions.OverallDepth);
            values["B"] = Number(dimensions.OverallWidth);
            values["h"] = Number(dimensions.FlatDepth);
            values["b"] = Number(dimensions.FlatWidth);
            values["tnom"] = Number(dimensions.NominalThickness);
            values["tdes"] = Number(dimensions.DesignThickness);

            values["C"] = Number(properties.HssTorsionalConstant);
        }

        private static void WriteChannel(
            IDictionary<string, string> values,
            StructuralSectionDefinition section,
            StructuralSectionProperties properties)
        {
            var dimensions = Require<ChannelSectionDimensions>(section);

            values["d"] = Number(dimensions.Depth);
            values["ddet"] = Number(dimensions.DetailingDepth);
            values["bf"] = Number(dimensions.FlangeWidth);
            values["bfdet"] = Number(dimensions.DetailingFlangeWidth);
            values["tw"] = Number(dimensions.WebThickness);
            values["twdet"] = Number(dimensions.DetailingWebThickness);
            values["twdet_2"] = Number(dimensions.HalfDetailingWebThickness);
            values["tf"] = Number(dimensions.FlangeThickness);
            values["tfdet"] = Number(dimensions.DetailingFlangeThickness);
            values["kdes"] = Number(dimensions.KDesign);
            values["kdet"] = Number(dimensions.KDetailing);
            values["x"] = Number(dimensions.CentroidX);
            values["eo"] = Number(dimensions.ShearCenterX);
            values["xp"] = Number(dimensions.PlasticNeutralAxisX);
            values["T"] = Number(dimensions.DistanceBetweenFilletToes);
            values["WGi"] = Number(dimensions.WorkableGageInner);

            values["Cw"] = Number(properties.Cw);
            values["Wno"] = Number(properties.Wno);
            values["Sw1"] = Number(properties.Sw1);
            values["Sw2"] = Number(properties.Sw2);
            values["Sw3"] = Number(properties.Sw3);
            values["Qf"] = Number(properties.Qf);
            values["Qw"] = Number(properties.Qw);
            values["ro"] = Number(properties.Ro);
            values["H"] = Number(properties.FlexuralConstantH);
            values["rts"] = Number(properties.Rts);
            values["ho"] = Number(properties.Ho);
            values["PA"] = Number(properties.PA);
            values["PB"] = Number(properties.PB);
            values["PC"] = Number(properties.PC);
            values["PD"] = Number(properties.PD);
        }

        private static void WriteAngle(
            IDictionary<string, string> values,
            StructuralSectionDefinition section,
            StructuralSectionProperties properties)
        {
            var dimensions = Require<AngleSectionDimensions>(section);

            values["d"] = Number(dimensions.ShortLeg);
            values["b"] = Number(dimensions.LongLeg);
            values["t"] = Number(dimensions.Thickness);
            values["kdes"] = Number(dimensions.KDesign);
            values["kdet"] = Number(dimensions.KDetailing);
            values["x"] = Number(dimensions.CentroidX);
            values["y"] = Number(dimensions.CentroidY);
            values["xp"] = Number(dimensions.PlasticNeutralAxisX);
            values["yp"] = Number(dimensions.PlasticNeutralAxisY);

            values["Iz"] = Number(properties.Iz);
            values["rz"] = Number(properties.Rz);
            values["Sz"] = Number(properties.Sz);
            values["Cw"] = Number(properties.Cw);
            values["ro"] = Number(properties.Ro);
            values["H"] = Number(properties.FlexuralConstantH);
            values["tanAlpha"] = Number(properties.TanAlpha);
            values["Iw"] = Number(properties.Iw);
            values["zA"] = Number(properties.ZA);
            values["zB"] = Number(properties.ZB);
            values["zC"] = Number(properties.ZC);
            values["wA"] = Number(properties.WA);
            values["wB"] = Number(properties.WB);
            values["wC"] = Number(properties.WC);
            values["SwA"] = Number(properties.SwA);
            values["SwB"] = Number(properties.SwB);
            values["SwC"] = Number(properties.SwC);
            values["SzA"] = Number(properties.SzA);
            values["SzB"] = Number(properties.SzB);
            values["SzC"] = Number(properties.SzC);
            values["PA"] = Number(properties.PA);
            values["PA2"] = Number(properties.PA2);
            values["PB"] = Number(properties.PB);
        }

        /// <summary>Rebuilds a definition from a strict row. The status overlay is applied afterwards.</summary>
        public static StructuralSectionDefinition FromRow(StructuralSectionFamily family, StrictCsvTable.Row row)
        {
            if (row == null)
            {
                throw new ArgumentNullException(nameof(row));
            }

            var sectionId = row.RequiredSectionId(StructuralSectionCsvSchema.SectionId);
            row.SectionId = sectionId.Value;

            var declaredFamily = row.RequiredFamily(StructuralSectionCsvSchema.Family);

            if (declaredFamily != family)
            {
                throw row.Fail(
                    StructuralSectionCsvSchema.Family,
                    "la fila declara la familia '" + StructuralSectionFamilies.ToToken(declaredFamily) +
                    "' dentro del archivo de la familia '" + StructuralSectionFamilies.ToToken(family) + "'.");
            }

            var identity = new StructuralSectionIdentity
            {
                SectionId = sectionId,
                Family = family,
                EdiDesignation = row.RequiredText(StructuralSectionCsvSchema.EdiDesignation),
                ManualLabel = row.RequiredText(StructuralSectionCsvSchema.ManualLabel),
                SourceId = row.RequiredText(StructuralSectionCsvSchema.SourceId),
                SourceRevision = row.RequiredText(StructuralSectionCsvSchema.SourceRevision)
            };

            var properties = new StructuralSectionProperties
            {
                Area = row.OptionalDouble("A"),
                Ix = row.OptionalDouble("Ix"),
                Zx = row.OptionalDouble("Zx"),
                Sx = row.OptionalDouble("Sx"),
                Rx = row.OptionalDouble("rx"),
                Iy = row.OptionalDouble("Iy"),
                Zy = row.OptionalDouble("Zy"),
                Sy = row.OptionalDouble("Sy"),
                Ry = row.OptionalDouble("ry"),
                J = row.OptionalDouble("J"),
                Cw = family == StructuralSectionFamily.HssRectangular ? null : row.OptionalDouble("Cw"),
                HssTorsionalConstant =
                    family == StructuralSectionFamily.HssRectangular ? row.OptionalDouble("C") : null,
                Wno = HasWarping(family) ? row.OptionalDouble("Wno") : null,
                Sw1 = HasWarping(family) ? row.OptionalDouble("Sw1") : null,
                Sw2 = family == StructuralSectionFamily.Channel ? row.OptionalDouble("Sw2") : null,
                Sw3 = family == StructuralSectionFamily.Channel ? row.OptionalDouble("Sw3") : null,
                Qf = HasWarping(family) ? row.OptionalDouble("Qf") : null,
                Qw = HasWarping(family) ? row.OptionalDouble("Qw") : null,
                Ro = HasPolarRadius(family) ? row.OptionalDouble("ro") : null,
                FlexuralConstantH = HasPolarRadius(family) ? row.OptionalDouble("H") : null,
                Rts = HasWarping(family) ? row.OptionalDouble("rts") : null,
                Ho = HasWarping(family) ? row.OptionalDouble("ho") : null,
                Iz = family == StructuralSectionFamily.Angle ? row.OptionalDouble("Iz") : null,
                Rz = family == StructuralSectionFamily.Angle ? row.OptionalDouble("rz") : null,
                Sz = family == StructuralSectionFamily.Angle ? row.OptionalDouble("Sz") : null,
                TanAlpha = family == StructuralSectionFamily.Angle ? row.OptionalDouble("tanAlpha") : null,
                Iw = family == StructuralSectionFamily.Angle ? row.OptionalDouble("Iw") : null,
                ZA = family == StructuralSectionFamily.Angle ? row.OptionalDouble("zA") : null,
                ZB = family == StructuralSectionFamily.Angle ? row.OptionalDouble("zB") : null,
                ZC = family == StructuralSectionFamily.Angle ? row.OptionalDouble("zC") : null,
                WA = family == StructuralSectionFamily.Angle ? row.OptionalDouble("wA") : null,
                WB = family == StructuralSectionFamily.Angle ? row.OptionalDouble("wB") : null,
                WC = family == StructuralSectionFamily.Angle ? row.OptionalDouble("wC") : null,
                SwA = family == StructuralSectionFamily.Angle ? row.OptionalDouble("SwA") : null,
                SwB = family == StructuralSectionFamily.Angle ? row.OptionalDouble("SwB") : null,
                SwC = family == StructuralSectionFamily.Angle ? row.OptionalDouble("SwC") : null,
                SzA = family == StructuralSectionFamily.Angle ? row.OptionalDouble("SzA") : null,
                SzB = family == StructuralSectionFamily.Angle ? row.OptionalDouble("SzB") : null,
                SzC = family == StructuralSectionFamily.Angle ? row.OptionalDouble("SzC") : null,
                PA = HasPerimeters(family) ? row.OptionalDouble("PA") : null,
                PA2 = family == StructuralSectionFamily.Angle ? row.OptionalDouble("PA2") : null,
                PB = HasPerimeters(family) ? row.OptionalDouble("PB") : null,
                PC = HasWarping(family) ? row.OptionalDouble("PC") : null,
                PD = HasWarping(family) ? row.OptionalDouble("PD") : null
            };

            return new StructuralSectionDefinition
            {
                Identity = identity,
                WeightPerLength = row.RequiredDouble(StructuralSectionCsvSchema.WeightPerLength),
                NativeUnitSystem = row.RequiredUnitSystem(StructuralSectionCsvSchema.NativeUnitSystem),
                Dimensions = ReadDimensions(family, row),
                Properties = properties,
                SourceSpecialNote = family == StructuralSectionFamily.W ? row.OptionalBool("T_F") : null,
                IsEnabled = true,
                MaterialGrade = null
            };
        }

        /// <summary>
        /// W, C and S are the families whose files carry the Design Guide 9 warping block.
        ///
        /// S belongs here on the evidence, not by analogy with W: the 28 rows publish <c>Cw</c>, <c>Wno</c>,
        /// <c>Sw1</c>, <c>Qf</c>, <c>Qw</c>, <c>rts</c>, <c>ho</c>, <c>PC</c> and <c>PD</c> complete, 28 of 28.
        /// </summary>
        private static bool HasWarping(StructuralSectionFamily family) =>
            family == StructuralSectionFamily.W ||
            family == StructuralSectionFamily.Channel ||
            family == StructuralSectionFamily.S;

        /// <summary>C and L tabulate the polar radius and the flexural constant; W and HSS do not.</summary>
        private static bool HasPolarRadius(StructuralSectionFamily family) =>
            family == StructuralSectionFamily.Channel || family == StructuralSectionFamily.Angle;

        /// <summary>HSS is the only family whose file carries no Design Guide 19 perimeter.</summary>
        private static bool HasPerimeters(StructuralSectionFamily family) =>
            family != StructuralSectionFamily.HssRectangular;

        private static IStructuralSectionDimensions ReadDimensions(
            StructuralSectionFamily family,
            StrictCsvTable.Row row)
        {
            switch (family)
            {
                case StructuralSectionFamily.W:
                    return new WSectionDimensions
                    {
                        Depth = row.OptionalDouble("d"),
                        DetailingDepth = row.OptionalDouble("ddet"),
                        FlangeWidth = row.OptionalDouble("bf"),
                        DetailingFlangeWidth = row.OptionalDouble("bfdet"),
                        WebThickness = row.OptionalDouble("tw"),
                        DetailingWebThickness = row.OptionalDouble("twdet"),
                        HalfDetailingWebThickness = row.OptionalDouble("twdet_2"),
                        FlangeThickness = row.OptionalDouble("tf"),
                        DetailingFlangeThickness = row.OptionalDouble("tfdet"),
                        KDesign = row.OptionalDouble("kdes"),
                        KDetailing = row.OptionalDouble("kdet"),
                        K1 = row.OptionalDouble("k1"),
                        DistanceBetweenFilletToes = row.OptionalDouble("T"),
                        WorkableGageInner = row.OptionalDouble("WGi"),
                        WorkableGageOuter = row.OptionalDouble("WGo")
                    };

                case StructuralSectionFamily.HssRectangular:
                    return new HssRectangularSectionDimensions
                    {
                        OverallDepth = row.OptionalDouble("Ht"),
                        OverallWidth = row.OptionalDouble("B"),
                        FlatDepth = row.OptionalDouble("h"),
                        FlatWidth = row.OptionalDouble("b"),
                        NominalThickness = row.OptionalDouble("tnom"),
                        DesignThickness = row.OptionalDouble("tdes")
                    };

                case StructuralSectionFamily.Channel:
                    return new ChannelSectionDimensions
                    {
                        Depth = row.OptionalDouble("d"),
                        DetailingDepth = row.OptionalDouble("ddet"),
                        FlangeWidth = row.OptionalDouble("bf"),
                        DetailingFlangeWidth = row.OptionalDouble("bfdet"),
                        WebThickness = row.OptionalDouble("tw"),
                        DetailingWebThickness = row.OptionalDouble("twdet"),
                        HalfDetailingWebThickness = row.OptionalDouble("twdet_2"),
                        FlangeThickness = row.OptionalDouble("tf"),
                        DetailingFlangeThickness = row.OptionalDouble("tfdet"),
                        KDesign = row.OptionalDouble("kdes"),
                        KDetailing = row.OptionalDouble("kdet"),
                        CentroidX = row.OptionalDouble("x"),
                        ShearCenterX = row.OptionalDouble("eo"),
                        PlasticNeutralAxisX = row.OptionalDouble("xp"),
                        DistanceBetweenFilletToes = row.OptionalDouble("T"),
                        WorkableGageInner = row.OptionalDouble("WGi")
                    };

                case StructuralSectionFamily.S:
                    return new SSectionDimensions
                    {
                        Depth = row.OptionalDouble("d"),
                        DetailingDepth = row.OptionalDouble("ddet"),
                        FlangeWidth = row.OptionalDouble("bf"),
                        DetailingFlangeWidth = row.OptionalDouble("bfdet"),
                        WebThickness = row.OptionalDouble("tw"),
                        DetailingWebThickness = row.OptionalDouble("twdet"),
                        HalfDetailingWebThickness = row.OptionalDouble("twdet_2"),
                        FlangeThickness = row.OptionalDouble("tf"),
                        DetailingFlangeThickness = row.OptionalDouble("tfdet"),
                        KDesign = row.OptionalDouble("kdes"),
                        KDetailing = row.OptionalDouble("kdet"),
                        K1 = row.OptionalDouble("k1"),
                        DistanceBetweenFilletToes = row.OptionalDouble("T"),
                        WorkableGageInner = row.OptionalDouble("WGi"),
                        WorkableGageOuter = row.OptionalDouble("WGo")
                    };

                case StructuralSectionFamily.Angle:
                    return new AngleSectionDimensions
                    {
                        ShortLeg = row.OptionalDouble("d"),
                        LongLeg = row.OptionalDouble("b"),
                        Thickness = row.OptionalDouble("t"),
                        KDesign = row.OptionalDouble("kdes"),
                        KDetailing = row.OptionalDouble("kdet"),
                        CentroidX = row.OptionalDouble("x"),
                        CentroidY = row.OptionalDouble("y"),
                        PlasticNeutralAxisX = row.OptionalDouble("xp"),
                        PlasticNeutralAxisY = row.OptionalDouble("yp")
                    };

                default:
                    throw new ArgumentOutOfRangeException(nameof(family), family, "Familia desconocida.");
            }
        }

        private static T Require<T>(StructuralSectionDefinition section) where T : class, IStructuralSectionDimensions
        {
            if (!(section.Dimensions is T typed))
            {
                throw new InvalidOperationException(
                    "La seccion '" + section.SectionId + "' declara la familia " + section.Family +
                    " pero sus dimensiones son " + (section.Dimensions?.GetType().Name ?? "<null>") + ".");
            }

            return typed;
        }

        /// <summary>Round-trip precision, invariant culture. Null becomes an EMPTY cell, never a zero.</summary>
        private static string Number(double? value) =>
            value.HasValue ? value.Value.ToString("R", CultureInfo.InvariantCulture) : string.Empty;

        private static string Number(double value) => value.ToString("R", CultureInfo.InvariantCulture);

        private static string Boolean(bool? value) =>
            value.HasValue ? (value.Value ? "true" : "false") : string.Empty;
    }
}
