"""I-62 F1, control C-04 (manual reproducible control, class (ii)).

Applies the aggregation procedure of docs/automation/agent-execution/README.md §12, step by step, to the design data of
c04-design-data.json (E1-E12 of Proposal V14 §4.2 in the shape of Anexo B.4, without schema validation), and compares
ConfigurationStatus, Causes and Disposition with the expected values written in the data. The procedure never reads the
expected values. Usage: python c04-aggregation-mc.py <c04-design-data.json> <c04-result.json>
"""
import json
import sys

MATCH, ABOVE, BELOW, UNKNOWN = "MATCH", "ABOVE_REQUIRED", "BELOW_REQUIRED", "UNKNOWN"


def requirement_key(requirement_id):
    return requirement_id.split(".", 1)[1]


def evaluate(evaluation, data):
    scales = data["Scales"]
    assurance = data["AssuranceOrder"]
    profile = dict(data["Profiles"][evaluation["Profile"]][evaluation["Action"]])
    profile.update(evaluation.get("ProfileOverride", {}))
    rows = [dict(r) for r in evaluation["Requirements"]]

    # Step 1: the mandatory requirements of the action come from the profile; a mandatory one without a row is added as NOT_OBSERVED.
    present = {r["RequirementId"] for r in rows}
    for requirement_id, required in profile.items():
        if requirement_id not in present:
            rows.append({"RequirementId": requirement_id, "Mandatory": True, "Required": required,
                         "Observation": {"State": "NOT_OBSERVED", "Value": None, "Source": None, "Assurance": "NONE", "ObservedUtc": None},
                         "AddedByStep1": True})

    # Step 2: Status of each requirement.
    by_id = {}
    for row in rows:
        by_id.setdefault(row["RequirementId"], []).append(row)
    statuses = {}
    contradictions = []
    for requirement_id, group in by_id.items():
        observed_values = {r["Observation"]["Value"] for r in group if r["Observation"]["State"] == "OBSERVED"}
        if len(group) > 1 and len(observed_values) > 1:
            statuses[requirement_id] = UNKNOWN
            contradictions.append(requirement_id)
            continue
        row = group[0]
        observation = row["Observation"]
        if (observation["State"] == "NOT_OBSERVED"
                or assurance.index(observation["Assurance"]) < assurance.index("RUNTIME_OBSERVED")
                or observation.get("Invalidated", False)):
            statuses[requirement_id] = UNKNOWN
            continue
        scale = scales[requirement_key(requirement_id)]
        observed, required = scale.index(observation["Value"]), scale.index(row["Required"])
        statuses[requirement_id] = BELOW if observed < required else ABOVE if observed > required else MATCH

    mandatory = sorted({r["RequirementId"] for r in rows if r["Mandatory"]})
    optional = sorted({r["RequirementId"] for r in rows if not r["Mandatory"]} - set(mandatory))

    # Step 3: aggregate over the mandatory requirements only (AUTOMATION_PLAN 16.16).
    mandatory_statuses = [statuses[i] for i in mandatory]
    if BELOW in mandatory_statuses:
        aggregate = BELOW
    elif UNKNOWN in mandatory_statuses:
        aggregate = UNKNOWN
    elif ABOVE in mandatory_statuses:
        aggregate = ABOVE
    else:
        aggregate = MATCH

    # Step 4: Causes = mandatory requirements whose Status is not MATCH, plus the contradictions.
    causes = sorted(set(i for i in mandatory if statuses[i] != MATCH) | set(contradictions))

    # Step 5: Disposition according to the table of AUTOMATION_PLAN 16.16.
    rules = []
    if contradictions:
        rules.append("S-04")
    if evaluation["Role"] == "PRINCIPAL_COORDINATOR" and aggregate == BELOW:
        rules.append("P-09")
    if rules:
        disposition = "STOP"
    elif aggregate in (UNKNOWN, BELOW):
        disposition, rules = "NOT_ELIGIBLE", ["P-10"]
    else:
        disposition = "ELIGIBLE"

    return {
        "Statuses": {i: statuses[i] for i in sorted(statuses)},
        "Mandatory": mandatory,
        "OptionalRecorded": {i: statuses[i] for i in optional},
        "AddedByStep1": sorted(r["RequirementId"] for r in rows if r.get("AddedByStep1")),
        "Contradictions": sorted(contradictions),
        "ConfigurationStatus": aggregate,
        "Causes": causes,
        "Disposition": disposition,
        "Rules": sorted(rules),
    }


def main(data_path, result_path):
    data = json.load(open(data_path, encoding="utf-8"))
    results, failures = [], 0
    for case in data["Cases"]:
        for evaluation in case["Evaluations"]:
            computed = evaluate(evaluation, data)  # Step 6: each action is computed apart.
            expected = evaluation["Expected"]
            checks = {
                "ConfigurationStatus": computed["ConfigurationStatus"] == expected["ConfigurationStatus"],
                "Causes": computed["Causes"] == sorted(expected["Causes"]),
                "Disposition": computed["Disposition"] == expected["Disposition"],
                "Rules": computed["Rules"] == sorted(expected["Rules"]),
            }
            if "Contradictions" in expected:
                checks["Contradictions"] = computed["Contradictions"] == sorted(expected["Contradictions"])
            if "RecordedOptional" in expected:
                checks["RecordedOptional"] = computed["OptionalRecorded"] == expected["RecordedOptional"]
            verdict = "PASS" if all(checks.values()) else "FAIL"
            failures += verdict == "FAIL"
            results.append({"Case": case["Case"], "Label": evaluation.get("Label"), "Role": evaluation["Role"], "Action": evaluation["Action"],
                            "Freeze": case["Freeze"], "Computed": computed, "Expected": expected, "Checks": checks, "Verdict": verdict})
    summary = {"Evaluations": len(results), "Pass": len(results) - failures, "Fail": failures,
               "Cases": sorted({r["Case"] for r in results}, key=lambda c: int(c[1:]))}
    with open(result_path, "w", encoding="utf-8", newline="\n") as handle:
        json.dump({"Summary": summary, "Results": results}, handle, ensure_ascii=False, indent=2)
        handle.write("\n")
    print(json.dumps(summary, ensure_ascii=False))
    return 1 if failures else 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1], sys.argv[2]))
