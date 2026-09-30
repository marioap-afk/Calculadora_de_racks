# I-52 — CT-21D Baseline Artifact BA-05: Fingerprint Specification FPSPEC-V1 (DRAFT V2)

> **BASELINE ARTIFACT BA-05 V2 — DRAFT, NOT SEALED. Design / documentation only. Nothing here is executed or authorized.**
>
> ```text
> ARTIFACT_ID               = CT21D-BASE-FINGERPRINT-SPEC
> FINGERPRINT_SPEC_VERSION  = FPSPEC-V1 (a tuple field; a change is a change of the baseline)
> ARTIFACT_VERSION          = 2-DRAFT   (supersedes 1-DRAFT, blob 69a548ecd0ec1d0da4d781ad9df8d8bc4200282d, which stays as history)
> ARTIFACT FILES            = this document + docs/automation/evidence/I-52-ct21d-fpspec-v1-vectors.gen.py (reference encoder)
>                             + docs/automation/evidence/I-52-ct21d-fpspec-v1-vectors.json (NORMATIVE vectors)
> ARTIFACT_STATUS           = DRAFT (phase 2)     CANDIDATE_HASH = UNSET     SEAL_HASH = UNSET
> AUTHORITY_CONTRACT        = CT-21D V2.1 28 + V2.2 PA-8, PA-9 + V2.3 + V2.4
> PHASE-1 RULING APPLIED    = normative vectors (extended); rule for types outside the census; census procedure and schema; tolerance sources (V17 4.3)
> BLOCKER                   = NB-3 (ADVANCES_TO_BASELINE_ARTIFACT_PREPARATION; OPEN: library census requires host; TOL_SCALE has no authority)
> CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
> ```

## 1. Two layers with different jobs

| Layer | Name | Job | Decides equivalence? |
|---|---|---|---|
| 1 | `EXACT_STATE_FINGERPRINT` | preserved-state comparison: `PRE_COMMAND_STATE_RECORD` against the state after an abort, pre-existing definitions, `REUSED_BOUND`, manifest additions, `LIBRARY_EQUIVALENCE_OBSERVATION`, cross-session determinism | **no**: equal digests = equal state; unequal = different |
| 2 | `SEMANTIC_COMPARISON` | expected versus persisted product semantics (S-1, S-2, S-3, the comparator of SL-6) | **yes**, by a typed domain comparator, **never** by a digest |

A signed-zero or other bit-level difference between domain-equivalent values is not a difference of layer 2 and must not produce O4; it may be recorded as `BIT_LEVEL_DIFFERENCE_DOMAIN_EQUIVALENT`.

## 2. Layer 1: `EXACT_STATE_FINGERPRINT`

### 2.1 Canonical byte stream (normative)

`stream = "FPSPEC-V1|EXACT|" + <record type> + "|" + fields`; for each field, in the schema order of section 2.3: `<name>` `=` `<value>` LF. The digest is SHA-256 of the stream, lowercase hexadecimal. The reference encoder is `I-52-ct21d-fpspec-v1-vectors.gen.py` (function `enc_value`); an implementation of instrument I-12 is conformant **only** if it reproduces every vector of section 2.7.

| Value kind | Encoding |
|---|---|
| integer | decimal ASCII, `-` for negatives, `0` for zero, no leading zeros |
| boolean | `1` or `0` |
| double | 16 lowercase hexadecimal digits of the IEEE-754 binary64 bit pattern, big-endian. `-0.0` and `+0.0` **differ**. NaN and infinities are **unsupported**: the record is `UNKNOWN`, never a digest |
| string, enum | `S` + decimal count of UTF-8 bytes + `:` + the UTF-8 bytes of the **exact stored text** (no normalization, no case folding; an enum by its stored name) |
| raw bytes | `B` + decimal byte count + `:` + the bytes |
| absent / null | `~` |
| ordered sequence | `[` + decimal element count + LF + each element encoded and followed by LF + `]` |
| unordered set | the elements sorted by the ordinal order of their stored UTF-8 name (or canonical key), then encoded as an ordered sequence |
| nested record inline | `{` + record type + LF + its fields + `}` |
| nested definition by reference | `@` + the name encoded as a string + `#` + its 64-hex digest |

### 2.2 Ordering (normative)

| Container | Order |
|---|---|
| ordered entity containers (entity sequence of a block table record = draw order) | host iteration / drawing order; the order is state |
| unordered containers (symbol-table name sets, dictionary entries, sibling sets) | stored-name order (ordinal) |
| property keys inside a record | sorted, ordinal |

### 2.3 Record types (schema, version 1)

| Record type | Fields (stream order) |
|---|---|
| `DEFINITION_DUMP` | `name` (stored case), `originX`, `originY`, `originZ`, `explodable`, `scaling`, `units`, `annotative`, `entities` (ordered sequence of `ENTITY`; nested definitions appear as `BLOCK_REFERENCE` entities that name them; the nested definition digest is carried with a reference encoding) |
| `ENTITY` | `dxfName`, `layer`, `linetype`, `colorSource`, `colorValue`, `lineweight`, `transparency`, `linetypeScale`, `visible`, then the fields of the concrete type |
| `BLOCK_REFERENCE` (concrete) | `blockName` (stored case), `positionX/Y/Z`, `scaleX/Y/Z`, `rotation`, `normalX/Y/Z`, `dynamicProperties` (set of (`name`, `value`) sorted by name); anonymous `*U` names are excluded: a dynamic reference is identified by the parent dynamic block name and its property values |
| `TEXT` (concrete, single-line text) | `string`, `height`, `rotation`, `styleName`, `horizontalMode`, `verticalMode`, `positionX/Y/Z`, `alignmentX/Y/Z` |
| `ROTATED_DIMENSION` (concrete) | `xLine1X/Y/Z`, `xLine2X/Y/Z`, `dimLineX/Y/Z`, `rotation`, `dimStyleName`, `overrides` (set of (`name`, `value`)), `textOverride`; the host-derived anonymous `*D` block is excluded |
| other concrete types found in the **library census** (section 5) | property lists are **fixed by the census**, one new version of this specification when the list is added; until then such a type is `UNKNOWN` |
| `SYMBOL_RECORD` | `table`, `name` (stored case), then the record properties (layer: `color`, `linetype`, `lineweight`, `on`, `frozen`, `locked`, `plot`, `transparency`; text style: font, size, width factor, oblique angle; dimension style: the full variable set; linetype: the pattern) |
| `PAYLOAD_EXACT` | `bytes`: the raw stored bytes of the RackCad payload (the `Xrecord` under `RACKCAD_SELECTIVE` in the extension dictionary of the block definition), concatenated chunk by chunk |
| `NOD_ENTRY` | `key`, `value` (canonical) |
| `ENUMERATION` | ordered or sorted list of names or handles of a container; handle lists are valid only within one session |

### 2.4 Exclusions

`ObjectId`; database-local handles (except inside `ENUMERATION` within one session); timestamps; volatile identifiers; reactors and host-local pointers; anonymous block table records (`*U`, `*D`, any host-generated anonymous name) as definitions.

### 2.5 Unknown and unsupported (rule for types outside the census)

A proxy entity, a custom entity, a type whose property list is not in section 2.3 (including every type found by the census that has not been added by a new version), a non-finite number, or an unreadable record yields **`UNKNOWN`**, never a partial digest. Consequences:

- a member of the **static dependency closure** with such a type makes the closure `UNKNOWN` and **refuses the run (O1)** (V2.1 6);
- a **pre-existing homologue** of the user's drawing with such a type (in the record or a `REUSED_BOUND` member) makes the record `UNKNOWN` and refuses the run (O1); this is an **availability cost** disclosed in Owner Act 2;
- adding a type is a **new version** of this specification (and a change of the baseline).

### 2.6 Cross-session determinism

The same content yields the same digest in two separate sessions of the same build; a 1-ulp change changes the digest; the provider is qualified as I-12 (scenarios `Q-I12`, `Q-SIGNED-ZERO`, `Q-FPSPEC-VECTORS`).

### 2.7 NORMATIVE exact vectors

The streams show LF as `\n`. These vectors become normative when this artifact is sealed; the JSON file is authoritative for the exact bytes.

| Id | Purpose | Stream | SHA-256 or result |
|---|---|---|---|
| EV-01 | baseline coordinate (+0.0 components) | `FPSPEC-V1\|EXACT\|TEST_COORD3\|x=3ff0000000000000\ny=0000000000000000\nz=0000000000000000\n` | `ba4e9ed7478785ce06e8e0010bfbd14970b9e87cbc923f63108fa6009acdae5e` |
| EV-02 | SIGNED ZERO: y = -0.0 must differ from EV-01 | `FPSPEC-V1\|EXACT\|TEST_COORD3\|x=3ff0000000000000\ny=8000000000000000\nz=0000000000000000\n` | `bc2cf0352fc258115745f43a0d7127011c2d692231696af4a25b4bd244626c11` |
| EV-03 | 1 ulp above 1.0 must differ from EV-01 | `FPSPEC-V1\|EXACT\|TEST_COORD3\|x=3ff0000000000001\ny=0000000000000000\nz=0000000000000000\n` | `23ec89504f6f855fda794064fae9df813679bb4de17a347f9d36dce2d607c42d` |
| EV-04 | SUBNORMAL: smallest positive subnormal | `FPSPEC-V1\|EXACT\|TEST_COORD3\|x=0000000000000001\ny=0000000000000000\nz=0000000000000000\n` | `e2a2a8b5b8faf7e5c36bedd923cf2a93af3a298c69abad649c7a1a7f8f7d09b0` |
| EV-05 | MAXIMUM FINITE double | `FPSPEC-V1\|EXACT\|TEST_COORD3\|x=7fefffffffffffff\ny=0000000000000000\nz=0000000000000000\n` | `8443fcaab8e6744212cc983a39231e856c418cdc91e01695f9d4d0439187dffd` |
| EV-06 | NaN is unsupported: UNKNOWN, no digest | (no stream) | **UNKNOWN** (non-finite double) |
| EV-07 | infinity is unsupported: UNKNOWN, no digest | (no stream) | **UNKNOWN** (non-finite double) |
| EV-08 | string, exact case | `FPSPEC-V1\|EXACT\|TEST_NAME\|name=S8:Rack N 1\n` | `b625be899405392b55556bc8bff40e70a463190fbaf52e28460df64a91d9627a` |
| EV-09 | string, case differs from EV-08: must differ | `FPSPEC-V1\|EXACT\|TEST_NAME\|name=S8:rack N 1\n` | `80f92df041b0eddb78d33c471bd4b1fc0a815e2eb30dde3c0a4c47844945870f` |
| EV-10 | EMPTY STRING | `FPSPEC-V1\|EXACT\|TEST_NAME\|name=S0:\n` | `4ea81bbc8c2e91b3d939861286eb0b5713bc2310917bb6e33361f09a8b885924` |
| EV-11 | MULTIBYTE string (UTF-8, no normalization) | `FPSPEC-V1\|EXACT\|TEST_NAME\|name=S18:Ñandú 日本 €\n` | `1ae4a2037f5489626a3275ecaf007dd75373f89af1489347d58421404ab50ef3` |
| EV-12 | ABSENT value | `FPSPEC-V1\|EXACT\|TEST_OPTIONAL\|name=S4:Rack\nlayer=~\n` | `45a510107508c65801adbfc9e5751725b90b4a4e44e066d028a0dabc97f6018c` |
| EV-13 | ORDERED SEQUENCE (draw order is state) | `FPSPEC-V1\|EXACT\|TEST_SEQ\|items=[2\nS1:b\nS1:a\n]\n` | `d7504a38b410755341ed7aa6b0c6950b3ecdb42bb2535d2908a6d846b9e51063` |
| EV-14 | ordered sequence in the other order: must differ from EV-13 | `FPSPEC-V1\|EXACT\|TEST_SEQ\|items=[2\nS1:a\nS1:b\n]\n` | `01d971c4f2b3a0dd7d370bcf827d996985ae0f6707050a5b9407b687bf31f1e0` |
| EV-15 | UNORDERED container given as b,a: sorted by stored name | `FPSPEC-V1\|EXACT\|TEST_SET\|names=[2\nS1:a\nS1:b\n]\n` | `ab9751f8933fd5a2802d0309781c80b34161586758da7854cc1f47dba9a7c9de` |
| EV-16 | unordered container given as a,b: must EQUAL EV-15 | `FPSPEC-V1\|EXACT\|TEST_SET\|names=[2\nS1:a\nS1:b\n]\n` | `ab9751f8933fd5a2802d0309781c80b34161586758da7854cc1f47dba9a7c9de` |
| EV-17 | NESTED record inline | `FPSPEC-V1\|EXACT\|TEST_OUTER\|label=S5:outer\ninner={TEST_COORD3\nx=3ff0000000000000\ny=4000000000000000\nz=4008000000000000\n}\n` | `920c68a4e5b093b413fe47ed18b1e8f2e92a43171f327b846ea71725813463ea` |
| EV-18 | NESTED definition referenced by name and digest | `FPSPEC-V1\|EXACT\|TEST_REFHOLDER\|ref=@S18:SEL_FRONTAL_Post_0#ba4e9ed7478785ce06e8e0010bfbd14970b9e87cbc923f63108fa6009acdae5e\n` | `b3bd691805fcf4e27a0db961be0da70e1fd5d90842dd484513eb4d13e6183164` |
| EV-19 | PAYLOAD_EXACT raw bytes, no unknown member | `FPSPEC-V1\|EXACT\|PAYLOAD_EXACT\|bytes=B53:{"Id":"g-1","Kind":"selective","SchemaVersion":"1.0"}\n` | `94c92e2d6f8ca77ce4825dc71b4f49084374bee9de4b4dfb9791a6a13f74c9be` |
| EV-20 | PAYLOAD_EXACT with an UNKNOWN / FORWARD-COMPATIBLE member (ExtensionData): must differ from EV-19 | `FPSPEC-V1\|EXACT\|PAYLOAD_EXACT\|bytes=B70:{"Id":"g-1","Kind":"selective","SchemaVersion":"1.0","futureMember":1}\n` | `c1e2600cd701c22ecdfd26f3e2db247083bf95a6f00794b7174fd07535b012c3` |

Relations that the vectors pin: EV-01 != EV-02 (signed zero); EV-01 != EV-03 (1 ulp); EV-08 != EV-09 (case); EV-13 != EV-14 (sequence order is state); EV-15 == EV-16 (unordered set sorted); EV-19 != EV-20 (an unknown / forward-compatible member changes the exact payload digest); EV-06 and EV-07 have no digest.

## 3. Layer 2: `SEMANTIC_COMPARISON`

### 3.1 Comparator responsibilities

The comparator parses the persisted record with the **authoritative reader** into its typed form and compares it with the expected typed form computed by the oracle (BA-08 V2, `ORACLE_SPEC`). It is independent of the writer and of `mu_k`.

| Compared value | Rule | Slot and value |
|---|---|---|
| positions, text heights, dimension points, lengths | equal within the tolerance; `-0.0` equals `+0.0` | `TOL_LENGTH` = `GeometryTolerance.Length` = **1e-9 in** (`src/RackCad.Application/Geometry/Vector2D.cs:93`; V17 4.3 "Longitud") |
| rotations, normals | equal within the tolerance (normal: angle between the vectors) | `TOL_ANGLE` = `TOL_NORMAL` = `GeometryTolerance.Angle` = **1e-9 rad** (`Vector2D.cs:100`; V17 4.3 "Angulo: rotaciones, Normal") |
| values computed by a normalization (for example a reflected quantity recomputed by the plan) | equal within the tolerance, where the kind baseline marks the quantity as normalized | `TOL_CONTINUITY` = `GeometryTolerance.Continuity` = **1e-7 in** (`Vector2D.cs:97`; V17 4.3 "Continuidad") |
| dynamic-block property values that are lengths | equal within `TOL_LENGTH` | `TOL_DYNAMIC` = `TOL_LENGTH` (where the property is a length in inches) |
| **scale factors** | equal within `TOL_SCALE` | **`TOL_SCALE` = NO AUTHORITY** (V17 4.3: "Factor de escala: no existe"): a separate decision, test and record is required; it may use non-governing evidence; it is never derived from governing outputs; **no value is invented here** |
| angle normalization before comparison | the normalization rule of the kind baseline | `NORM_ANGLE` = fixed by BA-08 V2 from the code facts (no numeric value) |
| texts, names, enum values | exact (stored case for the value) | none |
| sibling sets | unordered | none |
| sequences whose order the plan determines | order-sensitive | none |
| authored payload and envelope | typed members, `ExtensionData` and unknown members (section 3.2) | none |
| unrecognized schema version, unsupported type, unparsable record | `UNKNOWN` | none |

Gate (V2.2 PA-9): the values are part of the Selective kind authority baseline; agreed before `BASELINE_READY`; sealed before the first governing scenario that uses them; never derived from governing outputs; any change is a new baseline.

### 3.2 `ExtensionData`, authored and unknown members (V2.2 PA-8)

The typed form compared for the authored payload and envelope includes `ExtensionData`, the authored and forward-compatible members, and the unknown members preserved by contract (I-11). Unknown and extension members are compared exactly after canonical serialization. A writer that **drops or alters** an unknown member **does not pass** (DIFFERENT; O4 through S-2 or S-3). A member that cannot be compared yields **`UNKNOWN_MEMBER_UNSUPPORTED`**, surfaced explicitly, giving `UNKNOWN` (O5 path). An unrecognized schema version is `UNKNOWN`.

### 3.3 NORMATIVE semantic comparison vectors

| Id | Expected | Persisted | Result | Rule |
|---|---|---|---|---|
| SV-01 | {"Id":"g-1","ExtensionData":{"futureKey":1}} | {"ExtensionData":{"futureKey":1},"Id":"g-1"} | **EQUAL** | keys compared after canonical sorting; extension member present and equal |
| SV-02 | {"Id":"g-1","ExtensionData":{"futureKey":1}} | {"Id":"g-1"} | **DIFFERENT** | a writer that DROPS an unknown member must not pass (O4 through S-2/S-3) |
| SV-03 | {"Id":"g-1","ExtensionData":{"futureKey":1}} | {"Id":"g-1","ExtensionData":{"futureKey":2}} | **DIFFERENT** | a writer that ALTERS an unknown member must not pass |
| SV-04 | unknown member nested inside a known sub-object that has no ExtensionData (JsonExtensionData is not recursive, I-11) | any | **UNKNOWN_MEMBER_UNSUPPORTED** | surfaced explicitly, gives UNKNOWN (O5 path); never ignored, never treated as absent |
| SV-05 | coordinate y = +0.0 | coordinate y = -0.0 | **EQUAL** | signed zero is domain-equivalent; secondary finding BIT_LEVEL_DIFFERENCE_DOMAIN_EQUIVALENT; the EXACT digests differ (EV-01 / EV-02) |
| SV-06 | length 10.0 in | length 10.0000000005 in | **EQUAL** | within TOL_LENGTH = GeometryTolerance.Length (1e-9 in) |
| SV-07 | length 10.0 in | length 10.000000002 in | **DIFFERENT** | outside TOL_LENGTH |
| SV-08 | envelope SchemaVersion "1.0" | envelope SchemaVersion "9.0" (unrecognized major) | **UNKNOWN** | an unrecognized schema version is UNKNOWN |
| SV-09 | entity of a type outside the supported list (proxy/custom) | same | **UNKNOWN** | unsupported type is UNKNOWN; in the closure it refuses the run (O1) |

### 3.4 Use of the layers

| Use | Layer |
|---|---|
| S-1, S-2, S-3, the comparator of SL-6 | `SEMANTIC_COMPARISON` |
| E-08 (S-4), S-5, `PRE_COMMAND_STATE_RECORD`, `PREPARE_RESIDUE_MANIFEST`, `REUSED_BOUND`, `LIBRARY_EQUIVALENCE_OBSERVATION` | `EXACT_STATE_FINGERPRINT` |

## 4. Other artifacts

The manifest, the custody log and the catalog are not fingerprints; their canonical serialization and hashing are defined by BA-07 V2 and BA-11 V2.

## 5. Library entity-type census (procedure and schema; NOT performed)

The census is **required before this artifact is sealed**. It reads the library drawing, which requires host work (**not authorized**); it is **not** performed in this gate.

**Procedure (for a future host-authorized gate).** On the private copy of the exact library file of the baseline (a tuple field), read-only: (1) enumerate every block definition that the static required-block set of BA-08 V2 section 7 can require, and the closure of their nested definitions; (2) for each, record the entity `dxfName` values with counts, the nested block names, the symbol records referenced, and whether any proxy or custom entity is present; (3) compute the type universe; (4) every type outside section 2.3 is listed as `UNSUPPORTED` until a new version of this specification adds its property list.

**Schema of the census record.**

| Field | Content |
|---|---|
| `libraryFileSha256`, `libraryPath`, `censusUtc`, `instrumentBuild` | identity of the census |
| `blocks[]` | `blockName`, `requiredBy` (BA-08 V2 roles), `entityTypes[]` (`dxfName`, `count`), `nestedBlocks[]`, `symbolRecords` (`layers[]`, `linetypes[]`, `textStyles[]`, `dimStyles[]`), `containsProxyOrCustom` |
| `typeUniverse[]` | the distinct `dxfName` values |
| `unsupportedTypes[]` | the types outside section 2.3 |

## 6. What keeps NB-3 open

| Id | Item | Gate |
|---|---|---|
| L-1 | the library census (section 5) and the property lists of the census types | host (not authorized) |
| L-2 | `TOL_SCALE` decision, test and record | separate decision (no authority today) |
| L-3 | `NORM_ANGLE` fixed in BA-08 V2 | baseline review |
| L-4 | candidate ruling, ratifications and seal of this artifact (document + vectors + reference encoder) | baseline review |

```text
NB-3 = ADVANCES_TO_BASELINE_ARTIFACT_PREPARATION ; OPEN (census, TOL_SCALE)
FPSPEC-V1 vectors = 20 exact + 9 semantic (normative when sealed)
CT21D_AUTHORITY_BASELINE_READY = FALSE
```
