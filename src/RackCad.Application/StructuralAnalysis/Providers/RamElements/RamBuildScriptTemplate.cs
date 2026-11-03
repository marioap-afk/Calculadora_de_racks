using System;
using System.Text;

namespace RackCad.Application.StructuralAnalysis.Providers.RamElements
{
    /// <summary>
    /// The fixed text of the build script (I-65 D-09 rules 1-10, CD-I65-F3-08). It never changes with the model: every datum travels
    /// in the payload, embedded as ONE string literal that <c>json.loads</c> decodes. The text uses only the calls and attributes of
    /// section 7 of the F3 document, writes nothing before its three gates (capability, empty model, payload), opens one single file (exclusive mode) before the first
    /// write, never analyses and never stores the model, and reads everything back in the pair of units it wrote.
    ///
    /// <para>
    /// The failed state is written with the Python escape <c>\u2014</c> so that the script stays plain ASCII. The text is kept with
    /// LF line ends whatever the line ends of this source file are.
    /// </para>
    /// </summary>
    public static class RamBuildScriptTemplate
    {
        /// <summary>The marker, inside the double quotes of the payload line, that <see cref="Render"/> replaces.</summary>
        public const string PayloadMarker = "@@PAYLOAD@@";

        /// <summary>The start of the line that carries the payload in a rendered script.</summary>
        public const string PayloadLinePrefix = "PAYLOAD = \"";

        /// <summary>The fixed text, with LF line ends and the marker in place of the payload.</summary>
        public static string Text { get; } = Normalize(Source) + "\n";

        /// <summary>The script for one payload: the payload as a Python string literal in double quotes.</summary>
        public static string Render(string payloadJson)
        {
            if (payloadJson == null)
            {
                throw new ArgumentNullException(nameof(payloadJson));
            }

            foreach (var character in payloadJson)
            {
                if (character < 0x20 || character > 0x7E)
                {
                    throw new ArgumentException("The payload must be plain ASCII on one line.", nameof(payloadJson));
                }
            }

            return Text.Replace(PayloadMarker, Escape(payloadJson));
        }

        /// <summary>The escape of a Python string literal in double quotes: the backslash first, then the quote.</summary>
        public static string Escape(string text) => text.Replace("\\", "\\\\").Replace("\"", "\\\"");

        private static string Normalize(string text) => text.Replace("\r\n", "\n").Replace("\r", "\n");

        private const string Source = """
# RackCad build script for the structural analysis program. GENERATED: do not edit.
# Use: the run-script command of the console, on a NEW and EMPTY model. It runs no analysis and does not store the
# model. It opens one single file for writing: ram-readback.json, inside the package folder.
import json
import math
import ramelements as rm
from ramelements._p4d.engine import get_core

PAYLOAD = "@@PAYLOAD@@"
FAILED = "FAILED \u2014 DISCARD MODEL"
PASSED = "READBACK OK"
READBACK_NAME = "ram-readback.json"
CASE_DESCRIPTION = "RackCad reference case"


def near(actual, expected, tolerance):
    return math.isfinite(actual) and abs(actual - expected) <= tolerance * max(1.0, abs(expected))


def near_abs(actual, expected, tolerance):
    return math.isfinite(actual) and abs(actual - expected) <= tolerance


def angle_near(actual, expected, tolerance):
    difference = actual - expected
    if not math.isfinite(difference):
        return False
    wrapped = math.fmod(difference, 360.0)
    if wrapped > 180.0:
        wrapped -= 360.0
    elif wrapped <= -180.0:
        wrapped += 360.0
    return abs(wrapped) <= tolerance * max(1.0, abs(expected))


def vector_near(actual, expected, tolerance):
    return len(actual) == len(expected) and all(near_abs(a, e, tolerance) for a, e in zip(actual, expected))


def check(differences, label, condition):
    if not condition:
        differences.append(label)


def capability_failures(data):
    failures = []
    capabilities = data["capabilities"]
    version = getattr(rm, "__version__", None)
    if version != capabilities["version"]:
        failures.append("package version is " + repr(version) + ", expected " + repr(capabilities["version"]))
    try:
        core = object.__getattribute__(get_core(), "_module")
        present = set(name for name in dir(core) if not name.startswith("__"))
    except Exception as exc:
        failures.append("the core functions cannot be read: " + type(exc).__name__ + ": " + str(exc))
        return failures
    for name in capabilities["coreFunctions"]:
        if name not in present:
            failures.append("core function missing: " + name)
    return failures


def empty_model_failures(m, case_id):
    failures = []
    if len(m.nodes) != 0:
        failures.append("the model has " + str(len(m.nodes)) + " nodes")
    if len(m.members) != 0:
        failures.append("the model has " + str(len(m.members)) + " members")
    cases = [lc.id for lc in m.load_condition]
    if len(cases) > 1:
        failures.append("the model has " + str(len(cases)) + " load cases")
    if case_id in cases:
        failures.append("the load case " + case_id + " already exists")
    return failures


def field(spec, key):
    return spec[key] if key in spec else None


def repeated_ids(kind, ids):
    seen = set()
    failures = []
    for item in ids:
        if item in seen:
            failures.append(kind + " id repeated: " + repr(item))
        seen.add(item)
    return failures


def payload_failures(data):
    failures = []
    if data["units"] != "LB_IN":
        failures.append("the unit pair of the payload is not LB_IN")
    node_ids = [field(spec, "id") for spec in data["nodes"]]
    member_ids = [field(spec, "id") for spec in data["members"]]
    load_ids = [field(spec, "id") for spec in data["loads"]]
    failures.extend(repeated_ids("node", node_ids))
    failures.extend(repeated_ids("member", member_ids))
    failures.extend(repeated_ids("load", load_ids))
    for spec in data["nodes"]:
        flags = field(spec, "restraints")
        if not isinstance(flags, list) or len(flags) != 6 or not all(isinstance(flag, bool) for flag in flags):
            failures.append("node " + str(field(spec, "id")) + " needs six boolean restraint flags")
    for spec in data["members"]:
        label = "member " + str(field(spec, "id"))
        if not field(spec, "section"):
            failures.append(label + " needs a section")
        if not field(spec, "material"):
            failures.append(label + " needs a material")
        if field(spec, "nodeI") not in node_ids:
            failures.append(label + " has a node I that does not exist")
        if field(spec, "nodeJ") not in node_ids:
            failures.append(label + " has a node J that does not exist")
    for spec in data["loads"]:
        label = "load " + str(field(spec, "id"))
        if field(spec, "member") not in member_ids:
            failures.append(label + " belongs to a member that does not exist")
        if field(spec, "direction") != "GLOBAL_Y":
            failures.append(label + " has a direction other than GLOBAL_Y")
    return failures


def write_model(m, data):
    case_id = data["loadCase"]["id"]
    m.units.set(rm.Units.LB_IN)
    nodes = {}
    for spec in data["nodes"]:
        f = spec["restraints"]
        restraints = rm.Restraints(f[0], f[1], f[2], f[3], f[4], f[5])
        nodes[spec["id"]] = m.nodes.add(spec["x"], spec["y"], spec["z"], restraints=restraints, description=spec["id"])
    members = {}
    for spec in data["members"]:
        member = m.members.add(
            nodes[spec["nodeI"]].id,
            nodes[spec["nodeJ"]].id,
            section=spec["section"],
            material=spec["material"],
            rotation=spec["rotation"],
            description=spec["id"],
        )
        member.cardinal_point = rm.CardinalPoint.CENTROID
        members[spec["id"]] = member
    m.load_condition.add_load_case(case_id, CASE_DESCRIPTION, data["loadCase"]["category"])
    for spec in data["loads"]:
        members[spec["member"]].add_point_load(
            case_id, rm.LoadDirection.GLOBAL_Y, spec["value"], distance=spec["percent"], is_percentage=True
        )
    return nodes, members


def read_back(m, data, nodes, members, differences, values):
    tolerances = data["tolerances"]
    relative = tolerances["relative"]
    percent = tolerances["percent"]
    axes_tolerance = tolerances["axes"]
    case_id = data["loadCase"]["id"]
    check(differences, "units: the pair is not LB_IN", m.units.current == rm.Units.LB_IN)
    check(differences, "the number of nodes is not " + str(len(data["nodes"])), len(m.nodes) == len(data["nodes"]))
    check(differences, "the number of members is not " + str(len(data["members"])), len(m.members) == len(data["members"]))
    for spec in data["nodes"]:
        node = nodes[spec["id"]]
        label = "node " + spec["id"]
        coordinates = tuple(node.get_coordinates())
        stored = node.restraints
        flags = [stored.dx, stored.dy, stored.dz, stored.rx, stored.ry, stored.rz]
        values["nodes"].append({
            "id": spec["id"],
            "ramId": node.id,
            "coordinates": list(coordinates),
            "restraints": flags,
        })
        expected = (spec["x"], spec["y"], spec["z"])
        check(differences, label + ": coordinates " + repr(coordinates),
              len(coordinates) == 3 and all(near(a, e, relative) for a, e in zip(coordinates, expected)))
        check(differences, label + ": restraints " + repr(flags), flags == list(spec["restraints"]))
    for spec in data["members"]:
        member = members[spec["id"]]
        label = "member " + spec["id"]
        l1, l2, l3 = member.get_local_axes()
        releases = member.end_releases
        flags = [
            releases.i_m33, releases.i_m22, releases.i_v2, releases.i_v3,
            releases.j_m33, releases.j_m22, releases.j_v2, releases.j_v3,
            releases.torsion, releases.axial,
        ]
        offset_i = tuple(member.rigid_end_offset_i)
        offset_j = tuple(member.rigid_end_offset_j)
        factors = tuple(member.get_stiffness_factors())
        values["members"].append({
            "id": spec["id"],
            "ramId": member.id,
            "section": member.section,
            "material": member.material,
            "rotation": member.rotation,
            "cardinalPoint": int(member.cardinal_point),
            "offsetI": list(offset_i),
            "offsetJ": list(offset_j),
            "stiffnessFactors": list(factors),
            "axialBehavior": int(member.axial_behavior),
            "springI": member.rotational_stiffness_i,
            "springJ": member.rotational_stiffness_j,
            "releases": flags,
            "l1": list(l1),
            "l2": list(l2),
            "l3": list(l3),
            "description": member.description,
        })
        check(differences, label + ": section " + repr(member.section), member.section == spec["section"])
        check(differences, label + ": material " + repr(member.material), member.material == spec["material"])
        check(differences, label + ": rotation " + repr(member.rotation), angle_near(member.rotation, spec["rotation"], relative))
        check(differences, label + ": local axis 1 " + repr(tuple(l1)), vector_near(l1, spec["l1"], axes_tolerance))
        check(differences, label + ": local axis 2 " + repr(tuple(l2)), vector_near(l2, spec["l2"], axes_tolerance))
        check(differences, label + ": local axis 3 " + repr(tuple(l3)), vector_near(l3, spec["l3"], axes_tolerance))
        check(differences, label + ": cardinal point", int(member.cardinal_point) == 0)
        check(differences, label + ": rigid end offset I", offset_i == (0.0, 0.0, 0.0))
        check(differences, label + ": rigid end offset J", offset_j == (0.0, 0.0, 0.0))
        check(differences, label + ": stiffness factors", factors == (1.0, 1.0, 1.0, 1.0))
        check(differences, label + ": axial behavior", int(member.axial_behavior) == 0)
        check(differences, label + ": rotational stiffness", member.rotational_stiffness_i == 0 and member.rotational_stiffness_j == 0)
        check(differences, label + ": releases", not any(flags))
        expected_loads = [load for load in data["loads"] if load["member"] == spec["id"]]
        count = member.point_load_count(case_id)
        check(differences, label + ": " + str(count) + " point loads", count == len(expected_loads))
        for index, load in enumerate(expected_loads):
            if index >= count:
                break
            stored = member.get_point_load(case_id, index)
            values["loads"].append({
                "id": load["id"],
                "member": spec["id"],
                "index": index,
                "direction": str(stored.direction.value),
                "value": stored.value,
                "percent": stored.distance,
                "isPercentage": bool(stored.is_percentage),
            })
            where = "load " + load["id"]
            check(differences, where + ": direction", stored.direction == rm.LoadDirection.GLOBAL_Y)
            check(differences, where + ": value " + repr(stored.value), near(stored.value, load["value"], relative))
            check(differences, where + ": position " + repr(stored.distance), near_abs(stored.distance, load["percent"], percent))
            check(differences, where + ": percentage flag", bool(stored.is_percentage))
    reference = m.load_condition[case_id]
    values["loadCase"] = {"id": case_id, "category": str(reference.category), "selfWeight": list(reference.self_weight)}
    check(differences, "the load case has self weight", tuple(reference.self_weight) == (0.0, 0.0, 0.0))


def persist_readback(out, result):
    if out is None:
        return
    try:
        try:
            text = json.dumps(result, indent=2, sort_keys=True)
            out.write(text + "\n")
        except Exception as exc:
            error = type(exc).__name__ + ": " + str(exc)
            result["status"] = FAILED
            result["differences"].append("readback: " + error)
            backup = {
                "schema": result["schema"],
                "status": FAILED,
                "analysisInputFingerprint": result["analysisInputFingerprint"],
                "error": error,
            }
            try:
                out.write(json.dumps(backup, indent=2, sort_keys=True) + "\n")
            except Exception:
                pass
    finally:
        out.close()


def main():
    differences = []
    values = {"nodes": [], "members": [], "loads": [], "loadCase": None}
    result = {
        "schema": "rackcad-ram-readback/v1",
        "status": FAILED,
        "adapterVersion": None,
        "analysisInputFingerprint": None,
        "differences": differences,
        "values": values,
    }
    out = None
    try:
        data = json.loads(PAYLOAD)
        result["adapterVersion"] = data["adapterVersion"]
        result["analysisInputFingerprint"] = data["analysisInputFingerprint"]
        gate = capability_failures(data)
        m = None
        if not gate:
            m = rm.model()
            gate = empty_model_failures(m, data["loadCase"]["id"])
        gate = gate + payload_failures(data)
        out = open(data["packageFolder"] + "\\" + READBACK_NAME, "x", encoding="utf-8", newline="\n")
        if gate:
            differences.extend(["gate: " + item for item in gate])
        else:
            nodes, members = write_model(m, data)
            read_back(m, data, nodes, members, differences, values)
    except Exception as exc:
        differences.append("exception: " + type(exc).__name__ + ": " + str(exc))
    if not differences:
        result["status"] = PASSED
    try:
        persist_readback(out, result)
    except Exception as exc:
        result["status"] = FAILED
        differences.append("readback: " + type(exc).__name__ + ": " + str(exc))
    print(result["status"])
    for item in differences:
        print(" - " + item)
    print("AnalysisInputFingerprint " + str(result["analysisInputFingerprint"]))


main()
""";
    }
}
