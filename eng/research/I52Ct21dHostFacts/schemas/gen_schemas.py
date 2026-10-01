#!/usr/bin/env python3
"""Generates the JSON schemas (JSON Schema 2020-12 subset, see core/JsonSchemaLite.cs) of the I52Ct21dHostFacts records.

The generated *.json files are the artifacts the instruments embed and validate against; this script is only the
convenient authoring form (it keeps the shared tuple definition in one place). Run: python gen_schemas.py
It writes LF, UTF-8 without BOM. It is not an instrument and is never run on a host.
"""
import json
import os

HERE = os.path.dirname(os.path.abspath(__file__))


def w(name, obj):
    with open(os.path.join(HERE, name), "w", encoding="utf-8", newline="\n") as f:
        json.dump(obj, f, indent=2, ensure_ascii=False)
        f.write("\n")


SHA = {"type": "string", "pattern": "^[0-9a-f]{64}$"}
BLOB = {"type": "string", "pattern": "^[0-9a-f]{40}$"}
HEX64 = {"type": "string", "pattern": "^[0-9A-F]{16}$"}
LABEL = {"type": "string", "pattern": "^(MC-[0-9a-f]{12}|UNSET)$"}
RUNID = {"type": "string", "pattern": "^HGP-H[0-9]+-[0-9]{8}T[0-9]{6}Z-[0-9]{2}$"}
HEX3 = {"type": "array", "items": {"$ref": "#/$defs/hex64"}, "minItems": 3, "maxItems": 3}
STAT = {"enum": ["OBSERVED", "OBSERVED_DIFFERS", "NOT_OBSERVED", "NOT_OBSERVABLE", "UNKNOWN", "INVALID"]}
SCHEMA_URI = "https://json-schema.org/draft/2020-12/schema"


def tuple_def():
    # TB-C / TB-B / TB-S / TB-L / TB-R of design 3.1. pid and processStartUtc are volatile (outside the hashed part).
    return {
        "type": "object", "additionalProperties": False,
        "required": ["TB-C", "TB-B", "TB-S", "TB-L", "TB-R"],
        "properties": {
            "TB-C": {"type": "object", "additionalProperties": False, "required": ["machineClassLabel"],
                     "properties": {"machineClassLabel": {"$ref": "#/$defs/label"}}},
            "TB-B": {"type": "object", "additionalProperties": False,
                     "required": ["buildTupleDigest", "packageManifestSha256", "declaredSetSha256", "instrument"],
                     "properties": {
                         "buildTupleDigest": {"$ref": "#/$defs/sha256"},
                         "packageManifestSha256": {"$ref": "#/$defs/sha256"},
                         "declaredSetSha256": {"$ref": "#/$defs/sha256"},
                         "instrument": {"type": "object", "additionalProperties": False,
                                        "required": ["name", "sha256", "selfPinStatus"],
                                        "properties": {"name": {"type": "string", "minLength": 1},
                                                       "sha256": {"$ref": "#/$defs/sha256"},
                                                       "selfPinStatus": {"type": "string", "minLength": 1}}}}},
            "TB-S": {"type": "object", "additionalProperties": False, "required": ["sessionId", "runId", "attempt"],
                     "properties": {"sessionId": {"type": "string", "minLength": 1}, "runId": {"$ref": "#/$defs/runId"},
                                    "attempt": {"type": "integer", "minimum": 1}}},
            "TB-L": {"type": "object", "additionalProperties": False,
                     "properties": {"notApplicable": {"type": "boolean"}, "privateCopyPath": {"type": "string"},
                                    "privateCopySha256Before": {"$ref": "#/$defs/sha256"},
                                    "privateCopySha256After": {"$ref": "#/$defs/sha256"},
                                    "libraryPath": {"type": "string"},
                                    "libraryFileSha256": {"$ref": "#/$defs/sha256"}}},
            "TB-R": {"type": "object", "additionalProperties": False, "required": ["designBlob", "ba05Blob"],
                     "properties": {"designBlob": {"$ref": "#/$defs/blob"}, "ba05Blob": {"$ref": "#/$defs/blob"}}},
        },
    }


def defs():
    return {"sha256": SHA, "blob": BLOB, "hex64": HEX64, "hex3": HEX3, "label": LABEL, "runId": RUNID, "status": STAT}


VOLATILE = {"type": "object", "required": ["capturedUtc", "pid", "processStartUtc"],
            "properties": {"capturedUtc": {"type": "string", "format": "date-time"}, "pid": {"type": "integer"},
                           "processStartUtc": {"type": "string"}}}

IDENT_BUILD = {"type": "object", "additionalProperties": False, "required": ["name", "sha256"],
               "properties": {"name": {"type": "string"}, "sha256": {"$ref": "#/$defs/sha256"}}}

HOST_TYPE_COUNTS = {"type": "array", "items": {"type": "object", "additionalProperties": False,
                                               "required": ["hostType", "count"],
                                               "properties": {"hostType": {"type": "string"},
                                                              "count": {"type": "integer", "minimum": 1}}}}


def common(schema_id, extra_required, extra_props):
    return {
        "$schema": SCHEMA_URI, "$id": schema_id, "type": "object", "additionalProperties": False,
        "required": ["schema", "governing", "gate", "tuple"] + extra_required + ["contentSha256", "volatile"],
        "properties": dict({"schema": {"const": schema_id}, "governing": {"const": False},
                            "gate": {"const": "HOST_GATE_BA11_3_3"}, "tuple": tuple_def(),
                            "contentSha256": {"$ref": "#/$defs/sha256"}, "volatile": VOLATILE}, **extra_props),
        "$defs": defs(),
    }


# --- designation (input of a run) ---------------------------------------------------------------------------------
w("ct21d.designation.v1.json", {
    "$schema": SCHEMA_URI, "$id": "ct21d.designation.v1", "type": "object", "additionalProperties": False,
    "required": ["schema", "runId", "attempt", "sessionId", "evidenceFolder", "privateCopyPath", "privateCopySha256",
                 "libraryPath", "libraryFileSha256", "declaredSet", "tupleBinding"],
    "properties": {
        "schema": {"const": "ct21d.designation.v1"}, "runId": {"$ref": "#/$defs/runId"},
        "attempt": {"type": "integer", "minimum": 1}, "sessionId": {"type": "string", "minLength": 1},
        "evidenceFolder": {"type": "string", "minLength": 1}, "privateCopyPath": {"type": "string", "minLength": 1},
        "privateCopySha256": {"$ref": "#/$defs/sha256"}, "libraryPath": {"type": "string", "minLength": 1},
        "libraryFileSha256": {"$ref": "#/$defs/sha256"},
        # The declared set (instrument DLLs and their dependencies) as a PLAIN LIST supplied by the CAD manager: nothing in this
        # folder generates it or a manifest (the generator of I-8 belongs to package v2). It is data, not a derived artefact.
        "declaredSet": {"type": "array", "minItems": 1, "items": {
            "type": "object", "additionalProperties": False, "required": ["path", "sha256"],
            "properties": {"path": {"type": "string", "minLength": 1}, "sha256": {"$ref": "#/$defs/sha256"}}}},
        "tupleBinding": {
            "type": "object", "additionalProperties": False,
            "required": ["machineClassLabel", "buildTupleDigest", "packageManifestSha256", "declaredSetSha256",
                         "designBlob", "ba05Blob"],
            "properties": {"machineClassLabel": {"$ref": "#/$defs/label"}, "buildTupleDigest": {"$ref": "#/$defs/sha256"},
                           "packageManifestSha256": {"$ref": "#/$defs/sha256"},
                           "declaredSetSha256": {"$ref": "#/$defs/sha256"},
                           "designBlob": {"$ref": "#/$defs/blob"}, "ba05Blob": {"$ref": "#/$defs/blob"}}}},
    "$defs": defs()})

# --- common host-fact record (design 3.1) -------------------------------------------------------------------------
w("ct21d.hostfact.v1.json", {
    "$schema": SCHEMA_URI, "$id": "ct21d.hostfact.v1", "type": "object", "additionalProperties": False,
    "required": ["schema", "factId", "gate", "governing", "tuple", "observation", "status", "rawRefs", "contentSha256",
                 "volatile"],
    "properties": {
        "schema": {"const": "ct21d.hostfact.v1"}, "factId": {"type": "string", "pattern": "^HF-[A-Z][0-9]+[a-z]?$"},
        "gate": {"const": "HOST_GATE_BA11_3_3"}, "governing": {"const": False}, "tuple": tuple_def(),
        "observation": {"type": "object"}, "status": {"$ref": "#/$defs/status"}, "reason": {"type": "string"},
        "rawRefs": {"type": "array", "items": {
            "type": "object", "additionalProperties": False, "required": ["path", "sha256"],
            "properties": {"path": {"type": "string", "pattern": "^[A-Za-z0-9][A-Za-z0-9._-]{0,119}$"},
                           "sha256": {"$ref": "#/$defs/sha256"}}}},
        "contentSha256": {"$ref": "#/$defs/sha256"}, "volatile": VOLATILE},
    "$defs": defs()})

# --- census (I-2, CT21DHG_CENSUS_R): BA-05 V5 section 5 schema + rbTypes ------------------------------------------
cls = {"type": "object", "additionalProperties": False, "required": ["rxClassName", "dxfName", "count"],
       "properties": {"rxClassName": {"type": "string"}, "dxfName": {"type": "string"},
                      "count": {"type": "integer", "minimum": 1}}}
names = {"type": "array", "items": {"type": "string"}}
block = {
    "type": "object", "additionalProperties": False,
    "required": ["index", "blockName", "isLayout", "isAnonymous", "isDynamic", "entityClasses", "nestedBlocks",
                 "referencesAnonymousStatic", "symbolRecords", "dimensionOverrides", "containsProxyOrCustom", "status",
                 "reason"],
    "properties": {
        "index": {"type": "integer", "minimum": 0}, "blockName": {"type": "string"}, "isLayout": {"type": "boolean"},
        "isAnonymous": {"type": "boolean"}, "isDynamic": {"type": "boolean"},
        "entityClasses": {"type": "array", "items": cls}, "nestedBlocks": names,
        "referencesAnonymousStatic": {"type": "boolean"},
        "symbolRecords": {"type": "object", "additionalProperties": False,
                          "required": ["layers", "linetypes", "textStyles", "dimStyles"],
                          "properties": {"layers": names, "linetypes": names, "textStyles": names, "dimStyles": names}},
        "dimensionOverrides": {"type": "array", "items": {
            "type": "object", "additionalProperties": False,
            "required": ["entityIndex", "rxClassName", "hasDstyleSection", "wellFormed", "pairs"],
            "properties": {"entityIndex": {"type": "integer", "minimum": 0}, "rxClassName": {"type": "string"},
                           "hasDstyleSection": {"type": "boolean"}, "wellFormed": {"type": "boolean"},
                           "pairs": {"type": "array", "items": {
                               "type": "object", "additionalProperties": False, "required": ["index", "code", "hostValueType"],
                               "properties": {"index": {"type": "integer", "minimum": 0}, "code": {"type": "integer"},
                                              "hostValueType": {"type": "string"}}}}}}},
        "containsProxyOrCustom": {"type": "boolean"}, "status": {"enum": ["OBSERVED", "UNKNOWN"]},
        "reason": {"type": "string"}}}
census = common("ct21d.census.v1",
                ["identity", "blocks", "classUniverse", "classesWithoutTable", "rbTypes", "rbOutsideTable", "counts"],
                {
                    "identity": {"type": "object", "additionalProperties": False,
                                 "required": ["libraryFileSha256", "libraryPath", "instrumentBuild", "dynamicPropertyMethod"],
                                 "properties": {"libraryFileSha256": {"$ref": "#/$defs/sha256"}, "libraryPath": {"type": "string"},
                                                "instrumentBuild": IDENT_BUILD,
                                                "dynamicPropertyMethod": {"const": "NOT_READ_BY_R0"}}},
                    "blocks": {"type": "array", "items": block},
                    "classUniverse": {"type": "array", "items": {
                        "type": "object", "additionalProperties": False, "required": ["rxClassName", "dxfNames"],
                        "properties": {"rxClassName": {"type": "string"}, "dxfNames": names}}},
                    "classesWithoutTable": {"type": "array", "items": {
                        "type": "object", "additionalProperties": False, "required": ["rxClassName", "marking"],
                        "properties": {"rxClassName": {"type": "string"},
                                       "marking": {"enum": ["named", "anonymousOrLayoutOnly"]}}}},
                    "rbTypes": {"type": "array", "minItems": 32, "maxItems": 32, "items": {
                        "type": "object", "additionalProperties": False,
                        "required": ["range", "expectedKind", "expectedHostTypes", "codesSeen", "hostTypes", "count", "status"],
                        "properties": {"range": {"type": "string", "pattern": "^[0-9]+-[0-9]+$"},
                                       "expectedKind": {"type": "string"}, "expectedHostTypes": names,
                                       "codesSeen": {"type": "array", "items": {"type": "integer"}},
                                       "hostTypes": HOST_TYPE_COUNTS, "count": {"type": "integer", "minimum": 0},
                                       "status": {"enum": ["OBSERVED", "OBSERVED_DIFFERS", "NOT_OBSERVED"]}}}},
                    "rbOutsideTable": {"type": "array", "items": {
                        "type": "object", "additionalProperties": False, "required": ["code", "hostTypes", "count"],
                        "properties": {"code": {"type": "integer"}, "hostTypes": HOST_TYPE_COUNTS,
                                       "count": {"type": "integer", "minimum": 1}}}},
                    "counts": {"type": "object", "additionalProperties": False,
                               "required": ["blocks", "blocksUnknown", "entities", "storedBuffersRead",
                                            "storedBufferReadFailures", "storedBufferEntries"],
                               "properties": {k: {"type": "integer", "minimum": 0} for k in
                                              ["blocks", "blocksUnknown", "entities", "storedBuffersRead",
                                               "storedBufferReadFailures", "storedBufferEntries"]}}})
w("ct21d.census.v1.json", census)

# --- inventory (I-2, CT21DHG_INVENTORY; HF-T2) --------------------------------------------------------------------
ref = {"type": "object", "additionalProperties": False,
       "required": ["index", "containerBlock", "entityIndex", "referencedBlock", "scale", "nearestMember", "status", "reason"],
       "properties": {"index": {"type": "integer", "minimum": 0}, "containerBlock": {"type": "string"},
                      "entityIndex": {"type": "integer", "minimum": 0}, "referencedBlock": {"type": "string"},
                      "scale": {"oneOf": [{"$ref": "#/$defs/hex3"}, {"type": "null"}]},
                      "nearestMember": {"oneOf": [{"type": "array", "minItems": 3, "maxItems": 3,
                                                   "items": {"enum": ["+1", "-1"]}}, {"type": "null"}]},
                      "status": {"enum": ["OBSERVED", "UNKNOWN"]}, "reason": {"type": "string"}}}
inventory = common("ct21d.inventory.v1", ["identity", "references", "summary"], {
    "identity": {"type": "object", "additionalProperties": False,
                 "required": ["libraryFileSha256", "libraryPath", "instrumentBuild"],
                 "properties": {"libraryFileSha256": {"$ref": "#/$defs/sha256"}, "libraryPath": {"type": "string"},
                                "instrumentBuild": IDENT_BUILD}},
    "references": {"type": "array", "items": ref},
    "summary": {"type": "object", "additionalProperties": False,
                "required": ["count", "observed", "unknown", "distinctTriples", "ImaxHex", "ImaxDecimalForReadingOnly", "status"],
                "properties": {"count": {"type": "integer", "minimum": 0}, "observed": {"type": "integer", "minimum": 0},
                               "unknown": {"type": "integer", "minimum": 0},
                               "distinctTriples": {"type": "integer", "minimum": 0},
                               "ImaxHex": {"oneOf": [{"$ref": "#/$defs/hex64"}, {"type": "null"}]},
                               "ImaxDecimalForReadingOnly": {"oneOf": [{"type": "string"}, {"type": "null"}]},
                               "status": {"enum": ["OBSERVED", "NOT_OBSERVED", "UNKNOWN"]}}}})
w("ct21d.inventory.v1.json", inventory)

# --- context variables (I-4; HF-G3) -------------------------------------------------------------------------------
cv = {"type": "object", "additionalProperties": False,
      "required": ["name", "hostType", "valueText", "valueBitsHex", "expectedType", "matches", "status", "reason"],
      "properties": {"name": {"type": "string"}, "hostType": {"oneOf": [{"type": "string"}, {"type": "null"}]},
                     "valueText": {"oneOf": [{"type": "string"}, {"type": "null"}]},
                     "valueBitsHex": {"oneOf": [{"$ref": "#/$defs/hex64"}, {"type": "null"}]},
                     "expectedType": {"oneOf": [{"type": "string"}, {"type": "null"}]},
                     "matches": {"oneOf": [{"type": "boolean"}, {"type": "null"}]},
                     "status": {"enum": ["OBSERVED", "OBSERVED_DIFFERS", "UNKNOWN"]}, "reason": {"type": "string"}}}
ctx = common("ct21d.ctxvars.v1", ["ctxVars", "checks"], {
    "ctxVars": {"type": "array", "minItems": 10, "items": cv},
    "checks": {"type": "object", "additionalProperties": False,
               "required": ["dbmodBefore", "dbmodAfter", "documentIdentityBefore", "documentIdentityAfter", "selfPin"],
               "properties": {"dbmodBefore": {"type": "string"}, "dbmodAfter": {"type": "string"},
                              "documentIdentityBefore": {"type": "string"}, "documentIdentityAfter": {"type": "string"},
                              "selfPin": {"type": "string"}}}})
w("ct21d.ctxvars.v1.json", ctx)

# --- machine label record (I-1 label helper, HF-M7) ---------------------------------------------------------------
w("ct21d.machine-label.v1.json", {
    "$schema": SCHEMA_URI, "$id": "ct21d.machine-label.v1", "type": "object", "additionalProperties": False,
    "required": ["schema", "fact", "gate", "governing", "label", "serializationSha256", "attributes"],
    "properties": {
        "schema": {"const": "ct21d.machine-label.v1"}, "fact": {"const": "HF-M7"}, "gate": {"const": "HOST_GATE_BA11_3_3"},
        "governing": {"const": False}, "label": {"$ref": "#/$defs/label"},
        "serializationSha256": {"oneOf": [{"$ref": "#/$defs/sha256"}, {"type": "null"}]},
        "attributes": {"type": "array", "minItems": 6, "maxItems": 6, "items": {
            "type": "object", "additionalProperties": False, "required": ["key", "status", "reason", "characters"],
            "properties": {
                "key": {"enum": ["AutoCadProduct", "AutoCadProfile", "MachineGuid", "OsVersionBuild", "SECURELOAD", "TRUSTEDPATHS"]},
                "status": {"enum": ["OBSERVED", "UNKNOWN"]}, "reason": {"type": "string"},
                "characters": {"oneOf": [{"type": "integer", "minimum": 0}, {"type": "null"}]}}}}},
    "$defs": defs()})
