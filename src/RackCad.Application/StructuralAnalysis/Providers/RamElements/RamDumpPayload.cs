using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RackCad.Application.StructuralAnalysis.Providers.RamElements
{
    /// <summary>
    /// The payload of the dump script (I-65 D-11, A65-V3-01): compact JSON in plain ASCII, schema <c>rackcad-ram-dump-payload/v1</c>. It
    /// carries the version of the adapter, the absolute folder the dump is written to, the name of the dump file and the capabilities the gate
    /// demands; the script holds no datum of its own. Pure: the same version and folder always give the same characters.
    /// </summary>
    public static class RamDumpPayload
    {
        public const string Schema = "rackcad-ram-dump-payload/v1";

        /// <summary>
        /// The functions of the core of the package that the dump script uses, in ordinal order; the capability gate demands all of them. Every
        /// one is among the 207 functions measured in P1. Six of them were not EXECUTED in P1 (nodes_get_id, nodes_get_description,
        /// members_get_nodes, lc_term_count, lc_term_factor and lc_term_lc_index): they come from the installed source of the package, read
        /// and not run.
        /// </summary>
        public static IReadOnlyList<string> CoreFunctions { get; } = new ReadOnlyCollection<string>(
            new[]
            {
                "lc_count",
                "lc_get_case_category",
                "lc_get_id",
                "lc_get_self_weight",
                "lc_is_combo",
                "lc_term_count",
                "lc_term_factor",
                "lc_term_lc_index",
                "members_count",
                "members_get_axial_behavior",
                "members_get_cardinal_point",
                "members_get_description",
                "members_get_id",
                "members_get_local_axes",
                "members_get_material",
                "members_get_nodes",
                "members_get_point_load",
                "members_get_point_load_count",
                "members_get_releases",
                "members_get_rigid_end_offset",
                "members_get_rotation",
                "members_get_rotational_stiffness",
                "members_get_section",
                "members_get_stiffness_factors",
                "nodes_count",
                "nodes_get_coordinates",
                "nodes_get_description",
                "nodes_get_id",
                "nodes_get_restraints",
                "units_get",
                "units_set",
            }.OrderBy(name => name, StringComparer.Ordinal).ToList());

        public static string Create(string adapterVersion, string dumpFolder)
        {
            var writer = new RamJsonWriter(asciiOnly: true, indented: false);

            writer.BeginObject();
            writer.Property("schema", Schema);
            writer.Property("adapterVersion", adapterVersion);
            writer.Property("dumpFolder", dumpFolder);
            writer.Property("dumpFileName", RamDumpScriptTemplate.DumpFileName);

            writer.Name("capabilities").BeginObject();
            writer.Property("version", RamBuildPayload.RequiredPackageVersion);
            writer.Name("coreFunctions").StringArray(CoreFunctions);
            writer.EndObject();

            writer.EndObject();

            return writer.ToString();
        }
    }
}
