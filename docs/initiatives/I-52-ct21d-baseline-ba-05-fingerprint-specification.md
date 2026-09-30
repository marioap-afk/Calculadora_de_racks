# I-52 — CT-21D Baseline Artifact BA-05: Fingerprint Specification (DRAFT V1)

> **BASELINE ARTIFACT BA-05 — DRAFT, NOT HASHED, NOT AGREED. Design / documentation only. Nothing here is executed or authorized.**
>
> ```text
> ARTIFACT_ID               = CT21D-BASE-FINGERPRINT-SPEC
> FINGERPRINT_SPEC_VERSION  = FPSPEC-V1-DRAFT   (a tuple field of the contract; a change is a change of the baseline)
> ARTIFACT_STATUS           = DRAFT (phase 1 of baseline preparation)
> ARTIFACT_HASH             = TO_BE_RECORDED_AT_SEAL   (see BA-11)
> AUTHORITY_CONTRACT        = CT-21D V2.1 section 28 + V2.2 PA-8, PA-9 + V2.3 (AGREED_FOR_BASELINE_ARTIFACT_PREPARATION)
> BLOCKER                   = NB-3  (ADVANCES_TO_BASELINE_ARTIFACT_PREPARATION; OPEN until this artifact is hashed and agreed)
> CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
> ```

## 1. Two layers with different jobs

| Layer | Name | Job | Decides equivalence? |
|---|---|---|---|
| 1 | `EXACT_STATE_FINGERPRINT` | preserved-state comparison: `PRE_COMMAND_STATE_RECORD` against the state after an abort, pre-existing definitions, `REUSED_BOUND` records, exact residue (manifest additions), cross-session determinism of the provider | **no**: equal digests mean equal state; unequal digests mean the state differs; it never says two different states are "the same in domain terms" |
| 2 | `SEMANTIC_COMPARISON` | expected-versus-persisted product semantics: S-1 staged reads, S-2 pass 1, S-3 pass 2, transforms, payload and domain equality, the independent comparator of SL-6 | **yes**, by a typed domain comparator, never by a digest |

**The digest of either layer never decides semantic equivalence.** The exact layer must not create O4 for domain-equivalent values: a signed-zero or other bit-level difference between domain-equivalent values is not a difference of layer 2 (it may be recorded as the secondary finding `BIT_LEVEL_DIFFERENCE_DOMAIN_EQUIVALENT`).

## 2. Layer 1: `EXACT_STATE_FINGERPRINT`

### 2.1 Canonical byte stream (normative)

The stream of a record is the concatenation of:

1. the **domain separator** `FPSPEC-V1|EXACT|<record type>|` (ASCII);
2. for each field of the record type, **in the order of the schema of section 2.3**: `<field name>` (ASCII), the byte `=`, the encoded value, and the byte `\n` (0x0A).

| Value kind | Encoding |
|---|---|
| integer / enum | decimal ASCII, no leading zeros, `-` for negatives; an enum is encoded by its **stored name** as a string, never by its numeric value |
| boolean | `1` or `0` |
| double | **16 lowercase hexadecimal digits of the IEEE-754 binary64 bit pattern** (sign bit first, big-endian). `-0.0` and `+0.0` **differ**. A non-finite value (NaN, +infinity, -infinity) is **unsupported**: the record fingerprint is `UNKNOWN`, never a digest |
| string | `S`, the decimal count of UTF-8 bytes, `:`, then the UTF-8 bytes of the **exact stored text** (no normalization, no case folding) |
| absent / null | `~` |
| sequence | `[`, the decimal element count, `\n`, each element encoded by its own kind and terminated by `\n`, then `]` |
| nested record | `{`, the record type, `\n`, its fields, `}`; **or**, for a nested definition referenced by name, the **name** (string) and its own digest as a lowercase hexadecimal string |

The digest is **SHA-256** of the stream, lowercase hexadecimal (64 characters).

### 2.2 Ordering (normative)

| Container | Order |
|---|---|
| **ordered entity containers** (the entity sequence of a block table record, which is draw order) | **host iteration / drawing order** as reported by the host; the order is part of the state |
| **unordered containers** (symbol-table name sets, dictionary entries, sibling sets) | **stored-name order**, ordinal comparison of the stored name (or canonical key) |
| property keys inside a record | sorted, ordinal |

### 2.3 Record types (schema, version 1)

Fields are listed in stream order. Exclusions of section 2.4 apply to every type.

| Record type | Fields |
|---|---|
| `DEFINITION_DUMP` (block table record) | `name` (stored case), `originX`, `originY`, `originZ`, `explodable`, `scaling`, `units`, `annotative`, then `entities` = the ordered sequence of `ENTITY` records; nested definitions appear as `BLOCK_REFERENCE` records that name them and carry the nested definition digest |
| `ENTITY` | `dxfName`, `layer`, `linetype`, `colorSource`, `colorValue`, `lineweight`, `transparency`, `linetypeScale`, `visible`, then the fields of the concrete type below |
| `BLOCK_REFERENCE` (concrete) | `blockName` (stored case), `positionX/Y/Z`, `scaleX/Y/Z`, `rotation`, `normalX/Y/Z`, `dynamicProperties` = sequence of (`name`, `value`) sorted by `name`; **anonymous block names are excluded**: a dynamic reference is identified by the parent dynamic block name and its property values |
| `TEXT` (concrete, single-line text) | `string`, `height`, `rotation`, `styleName`, `horizontalMode`, `verticalMode`, `positionX/Y/Z`, `alignmentX/Y/Z` |
| `ROTATED_DIMENSION` (concrete) | `xLine1X/Y/Z`, `xLine2X/Y/Z`, `dimLineX/Y/Z`, `rotation`, `dimStyleName`, `overrides` = sequence of (`name`, `value`) sorted by `name`, `textOverride`; the **anonymous `*D` block that the host derives is excluded** |
| `LINE`, `ARC`, `CIRCLE`, `POLYLINE`, `MTEXT`, `HATCH`, `ATTRIBUTE_DEFINITION` (concrete) | **listed as candidates**: the property lists are fixed by the library entity-type census (section 5, item L-1) because the Selective creation path itself draws none of them and the imported library blocks may contain them |
| `SYMBOL_RECORD` | `table`, `name` (stored case), then the properties of the record; for a layer: `color`, `linetype`, `lineweight`, `on`, `frozen`, `locked`, `plot`, `transparency`; for a dimension style: the full variable set of the style; for a text style: font, size, width factor, oblique angle; for a linetype: the pattern |
| `PAYLOAD_EXACT` | `bytes` = the raw stored bytes of the RackCad payload (the `Xrecord` under the key `RACKCAD_SELECTIVE` in the extension dictionary of the block definition) |
| `NOD_ENTRY` | `key`, `value` = canonical value |
| `ENUMERATION` | the ordered or sorted list of names or handles of a container; **handle lists are valid only within one session** and are excluded from cross-session determinism |

### 2.4 Exclusions

`ObjectId`; database-local handles (except inside `ENUMERATION` within one session); timestamps; volatile local identifiers; reactors and other host-local pointers; anonymous block table records (`*U*`, `*D*` and any other host-generated anonymous name) as definitions.

### 2.5 Unknown and unsupported

A proxy entity, a custom entity, a type not listed in section 2.3, a non-finite number, or a record the provider cannot read yields **`UNKNOWN`**, never a partial digest.

### 2.6 Cross-session determinism (requirements)

- The same content yields the same digest in **two separate sessions** (processes) of the same build; a 1-ulp change of one coordinate changes the digest; an unlisted type yields `UNKNOWN`; the `SIGNED_ZERO_CONTROL` shows the two layers differ exactly where meant.
- The provider is qualified as instrument I-12 (BA-04 scenarios `Q-I12` and `Q-SIGNED-ZERO`).
- Equal exact digests mean equal; unequal mean different; not computable means `UNKNOWN`.

### 2.7 Informative test vectors

Record type `TEST_COORD3` has the fields `x`, `y`, `z` (double); `TEST_NAME` has the field `name` (string). The streams and digests below follow section 2.1; a conformance test of I-12 must reproduce them. **These are informative examples of the encoding; they are not records of any run.**

| Id | Content | Stream (`\n` = byte 0x0A) | SHA-256 |
|---|---|---|---|
| V-1 | x = +1.0, y = +0.0, z = +0.0 | `FPSPEC-V1\|EXACT\|TEST_COORD3\|x=3ff0000000000000\ny=0000000000000000\nz=0000000000000000\n` | `ba4e9ed7478785ce06e8e0010bfbd14970b9e87cbc923f63108fa6009acdae5e` |
| V-2 | x = +1.0, y = -0.0, z = +0.0 (signed zero) | `FPSPEC-V1\|EXACT\|TEST_COORD3\|x=3ff0000000000000\ny=8000000000000000\nz=0000000000000000\n` | `bc2cf0352fc258115745f43a0d7127011c2d692231696af4a25b4bd244626c11` |
| V-3 | x = 1.0000000000000002 (1 ulp above 1.0), y = +0.0, z = +0.0 | `FPSPEC-V1\|EXACT\|TEST_COORD3\|x=3ff0000000000001\ny=0000000000000000\nz=0000000000000000\n` | `23ec89504f6f855fda794064fae9df813679bb4de17a347f9d36dce2d607c42d` |
| V-4 | string `Rack Ñ 1` (UTF-8, exact case, no normalization) | `FPSPEC-V1\|EXACT\|TEST_NAME\|name=S9:Rack Ñ 1\n` | `4176c368798514671c42ddddc5a62bee3ea38a2b086ea26b2103ea60da7e45f8` |
| V-5 | string `rack Ñ 1` (case differs) | `FPSPEC-V1\|EXACT\|TEST_NAME\|name=S9:rack Ñ 1\n` | `6046264660338e1c041e83bba001637440476649ce6d2a9593168c9a7c05e1c4` |

Observations that the vectors demonstrate: V-1 and V-2 differ (signed zero), V-1 and V-3 differ (1 ulp), V-4 and V-5 differ (case), and a NaN coordinate has **no** digest.

## 3. Layer 2: `SEMANTIC_COMPARISON`

### 3.1 Comparator responsibilities

The comparator parses the persisted record with the **authoritative reader** into its typed form and compares it with the **expected typed form** computed by the oracle. It is independent of the writer and of the reflection `mu_k`.

| Compared value | Rule | Slot |
|---|---|---|
| coordinates and transform components (position, scale factors, normal) | equal within the declared tolerance; `-0.0` equals `+0.0` | `TOL_LENGTH`, `TOL_SCALE`, `TOL_NORMAL` = `TO_BE_FIXED_BY_SELECTIVE_KIND_BASELINE` |
| angles (rotation) | equal within the declared tolerance after the declared normalization of the angle | `TOL_ANGLE`, `NORM_ANGLE` = `TO_BE_FIXED_BY_SELECTIVE_KIND_BASELINE` |
| text | string exact; height, rotation and position within tolerance; alignment modes by stored name | `TOL_TEXT_HEIGHT` = `TO_BE_FIXED_BY_SELECTIVE_KIND_BASELINE` |
| dimension | measurement points within tolerance; style name exact (stored case); overrides as name/value pairs | `TOL_DIM` = `TO_BE_FIXED_BY_SELECTIVE_KIND_BASELINE` |
| dynamic property values | numeric within tolerance; strings exact | `TOL_DYNAMIC` = `TO_BE_FIXED_BY_SELECTIVE_KIND_BASELINE` |
| sibling sets | unordered | none |
| entity sequences whose order the plan determines (draw order) | order-sensitive | none |
| names of layers, styles, blocks | exact stored case for the value; existence and collision by host symbol-table semantics | none |
| authored payload and envelope | typed members, `ExtensionData` and unknown members (section 3.2) | none |
| an unrecognized schema version, an unsupported type, an unparsable record | `UNKNOWN` | none |

**No numeric tolerance or normalization is fixed in this artifact.** Every slot is `TO_BE_FIXED_BY_SELECTIVE_KIND_BASELINE` (BA-08). V2.2 PA-9 gate: the values must be agreed before `BASELINE_READY`, sealed before the first governing scenario that uses them, **not derived from the outputs of governing runs**, and any later change produces a new baseline.

### 3.2 `ExtensionData`, authored and unknown members (V2.2 PA-8)

The typed form compared for the authored payload and envelope **includes**: `ExtensionData` as the authoritative reader exposes it; authored and forward-compatible members; unknown members preserved by contract (the I-11 rule).

1. Unknown and extension members are compared **exactly** (canonical serialization: sorted keys, exact numbers) between the expected form and the persisted form; the caller composes the envelope and the seam writes it verbatim, so the expected form contains those members.
2. **A writer that drops or alters an unknown member must not pass the semantic verification**: the comparison is `DIFFERENT` (product outcome O4 through S-2 or S-3).
3. A member that cannot be compared (a member kind the reader does not expose, or the declared I-11 limitation that `JsonExtensionData` is not recursive) yields the comparison state **`UNKNOWN_MEMBER_UNSUPPORTED`**, surfaced explicitly, giving `UNKNOWN` (the O5 path); it is **never ignored and never treated as absent**.
4. An unrecognized schema version remains `UNKNOWN`.

### 3.3 Use of the layers

| Use | Layer |
|---|---|
| expected versus persisted (S-1, S-2, S-3), comparator of SL-6 | `SEMANTIC_COMPARISON` |
| read-set re-observation E-08 (S-4), abort verification (S-5), `PRE_COMMAND_STATE_RECORD`, `PREPARE_RESIDUE_MANIFEST`, `REUSED_BOUND`, `LIBRARY_EQUIVALENCE_OBSERVATION` | `EXACT_STATE_FINGERPRINT` |

## 4. Canonical serialization of the manifest and of other artifacts

Other artifacts (the manifest, the custody log, the scenario catalog) are not fingerprints; their canonical serialization (sorted keys, LF, exact numbers) and hashing are defined by BA-07 and BA-11 and do not use `FPSPEC-V1`.

## 5. Open items that keep NB-3 open

| Id | Item |
|---|---|
| L-1 | **library entity-type census**: the library file is not versioned in the repository (V17 L-28). The list of entity types and property lists of section 2.3 for imported library blocks must be closed by enumerating the types present in the library blocks that a Selective plan requires, on the exact library file of the baseline (a tuple field). An unlisted type yields `UNKNOWN` (section 2.5) |
| L-2 | the concrete property lists for the candidate entity types of section 2.3 |
| L-3 | the numeric tolerances and normalizations of section 3.1 (BA-08) |
| L-4 | standalone hash of this artifact, and Coordinator and Architect agreement |

```text
NB-3 = ADVANCES_TO_BASELINE_ARTIFACT_PREPARATION ; OPEN until hash and agreement
FINGERPRINT_SPEC_VERSION = FPSPEC-V1-DRAFT (not sealed)
CT21D_AUTHORITY_BASELINE_READY = FALSE
```
