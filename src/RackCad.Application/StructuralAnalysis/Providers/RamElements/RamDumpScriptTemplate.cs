using System;

namespace RackCad.Application.StructuralAnalysis.Providers.RamElements
{
    /// <summary>
    /// The fixed text of the dump script (I-65 D-11, A65-V3-01, CD-I65-F4-02). It never changes with the model: every datum travels in the
    /// payload, embedded as ONE string literal that <c>json.loads</c> decodes. The script is READ-ONLY: it never adds, changes or deletes
    /// anything in the model, runs no analysis and does not store the model. It passes its capability gate before it touches the model,
    /// opens ONE single file (exclusive mode) in the embedded folder, reads in the pair LB_IN (changing the pair only when the model is in
    /// another one, and putting the initial pair back in a finally block whatever happens), and writes the dump as JSON.
    ///
    /// <para>
    /// The text is plain ASCII with LF line ends whatever the line ends of this source file are. Every call and attribute it uses was either
    /// executed in P1 or read from the installed package (see the F4-T3 delivery), and every core function it needs is among the 207 of P1.
    /// </para>
    /// </summary>
    public static class RamDumpScriptTemplate
    {
        /// <summary>The marker, inside the double quotes of the payload line, that <see cref="Render"/> replaces.</summary>
        public const string PayloadMarker = "@@PAYLOAD@@";

        /// <summary>The start of the line that carries the payload in a rendered script.</summary>
        public const string PayloadLinePrefix = "PAYLOAD = \"";

        /// <summary>The status of a dump that was read and written whole.</summary>
        public const string StatusOk = "DUMP OK";

        /// <summary>The status of a dump that failed, whatever the cause; the errors say which.</summary>
        public const string StatusFailed = "DUMP FAILED";

        /// <summary>The only file the script creates, inside the embedded folder.</summary>
        public const string DumpFileName = "ram-dump.json";

        /// <summary>The fixed text, with LF line ends and the marker in place of the payload.</summary>
        public static string Text { get; } = Normalize(Source) + "\n";

        /// <summary>The script for one payload: the payload as a string literal in double quotes.</summary>
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

            return Text.Replace(PayloadMarker, RamBuildScriptTemplate.Escape(payloadJson));
        }

        private static string Normalize(string text) => text.Replace("\r\n", "\n").Replace("\r", "\n");

        private const string Source = """
# RackCad dump script for the structural analysis program. GENERATED: do not edit.
# Use: the run-script command of the console, on the model that is OPEN. It is READ-ONLY: it adds, changes and deletes nothing in the
# model, runs no analysis and does not store the model. It opens one single file for writing: ram-dump.json, inside the dump folder.
# It reads in the pair LB_IN: only when the model is in another pair it switches to LB_IN and, whatever happens, it puts the initial
# pair back before it writes.
import json
import math
import ramelements as rm
from ramelements._p4d.engine import get_core

PAYLOAD = "@@PAYLOAD@@"
FAILED = "DUMP FAILED"
PASSED = "DUMP OK"
DUMP_NAME = "ram-dump.json"


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


def payload_failures(data):
    failures = []
    if data["dumpFileName"] != DUMP_NAME:
        failures.append("the dump file name is not " + DUMP_NAME)
    if not isinstance(data["dumpFolder"], str) or not data["dumpFolder"]:
        failures.append("the dump folder is missing")
    return failures


def read_nodes(m):
    nodes = []
    for node in m.nodes:
        flags = node.restraints
        nodes.append({
            "id": node.id,
            "description": node.description,
            "coordinates": list(node.get_coordinates()),
            "restraints": [flags.dx, flags.dy, flags.dz, flags.rx, flags.ry, flags.rz],
        })
    return nodes


def read_members(members):
    rows = []
    for member in members:
        l1, l2, l3 = member.get_local_axes()
        releases = member.end_releases
        rows.append({
            "id": member.id,
            "description": member.description,
            "nodeI": member.node_i,
            "nodeJ": member.node_j,
            "section": member.section,
            "material": member.material,
            "rotation": member.rotation,
            "l1": list(l1),
            "l2": list(l2),
            "l3": list(l3),
            "cardinalPoint": int(member.cardinal_point),
            "offsetI": list(member.rigid_end_offset_i),
            "offsetJ": list(member.rigid_end_offset_j),
            "stiffnessFactors": list(member.get_stiffness_factors()),
            "axialBehavior": int(member.axial_behavior),
            "springI": member.rotational_stiffness_i,
            "springJ": member.rotational_stiffness_j,
            "releases": [
                releases.i_m33, releases.i_m22, releases.i_v2, releases.i_v3,
                releases.j_m33, releases.j_m22, releases.j_v2, releases.j_v3,
                releases.torsion, releases.axial,
            ],
        })
    return rows


def read_case(case, members):
    member_loads = []
    for member in members:
        loads = []
        for index in range(member.point_load_count(case.id)):
            load = member.get_point_load(case.id, index)
            loads.append({
                "direction": str(load.direction.value),
                "value": load.value,
                "distance": load.distance,
                "isPercentage": bool(load.is_percentage),
            })
        if loads:
            member_loads.append({"member": member.id, "loads": loads})
    return {
        "id": case.id,
        "category": str(case.category),
        "selfWeight": list(case.self_weight),
        "memberLoads": member_loads,
    }


def read_cases(m, members, result):
    for case in m.load_condition:
        if case.is_combination:
            terms = [{"case": term.case, "factor": term.factor} for term in case.terms]
            result["combinations"].append({"id": case.id, "terms": terms})
        else:
            result["loadCases"].append(read_case(case, members))


def read_model(m, initial, result):
    changed = False
    try:
        if initial != rm.Units.LB_IN:
            changed = True
            m.units.set(rm.Units.LB_IN)
        result["readUnits"] = m.units.current.name
        if result["readUnits"] != rm.Units.LB_IN.name:
            raise ValueError("the pair of units of the reading is not LB_IN")
        result["nodes"] = read_nodes(m)
        members = list(m.members)
        result["members"] = read_members(members)
        read_cases(m, members, result)
        if not all(math.isfinite(value) for node in result["nodes"] for value in node["coordinates"]):
            raise ValueError("a node coordinate is not finite")
    finally:
        if changed:
            m.units.set(initial)
            result["unitsRestored"] = True


def persist_dump(out, result):
    if out is None:
        return
    try:
        try:
            text = json.dumps(result, indent=2, sort_keys=True, allow_nan=False)
            out.write(text + "\n")
        except Exception as exc:
            error = type(exc).__name__ + ": " + str(exc)
            result["status"] = FAILED
            result["errors"].append("dump: " + error)
            backup = {
                "schema": result["schema"],
                "status": FAILED,
                "adapterVersion": result["adapterVersion"],
                "errors": result["errors"],
            }
            try:
                out.write(json.dumps(backup, indent=2, sort_keys=True) + "\n")
            except Exception:
                pass
    finally:
        out.close()


def main():
    errors = []
    result = {
        "schema": "rackcad-ram-dump/v1",
        "status": FAILED,
        "adapterVersion": None,
        "initialUnits": None,
        "readUnits": None,
        "unitsRestored": False,
        "nodes": [],
        "members": [],
        "loadCases": [],
        "combinations": [],
        "errors": errors,
    }
    out = None
    try:
        data = json.loads(PAYLOAD)
        result["adapterVersion"] = data["adapterVersion"]
        gate = capability_failures(data)
        m = None
        initial = None
        if not gate:
            m = rm.model()
            initial = m.units.current
            result["initialUnits"] = initial.name
        gate = gate + payload_failures(data)
        out = open(data["dumpFolder"] + "\\" + DUMP_NAME, "x", encoding="utf-8", newline="\n")
        if gate:
            errors.extend(["gate: " + item for item in gate])
        else:
            read_model(m, initial, result)
    except Exception as exc:
        errors.append("exception: " + type(exc).__name__ + ": " + str(exc))
    if not errors:
        result["status"] = PASSED
    try:
        persist_dump(out, result)
    except Exception as exc:
        result["status"] = FAILED
        errors.append("dump: " + type(exc).__name__ + ": " + str(exc))
    print(result["status"])
    for item in errors:
        print(" - " + item)


main()
""";
    }
}
