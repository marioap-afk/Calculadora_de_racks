# I-52 — CT-21D Baseline Artifact BA-05: Fingerprint Specification FPSPEC-V1 (DRAFT V4)

> **BASELINE ARTIFACT BA-05 V4 — DRAFT, NOT SEALED. Design / documentation only. Nothing here is executed or authorized.**
>
> ```text
> ARTIFACT_ID               = CT21D-BASE-FINGERPRINT-SPEC
> FINGERPRINT_SPEC_VERSION  = FPSPEC-V1 (a tuple field; unchanged by this draft version, see section 2.6)
> ARTIFACT_VERSION          = 4-DRAFT (supersedes 3-DRAFT, blob 9e4fa8eb9295a06c8db96e397fe31e9fb454ceaa, which stays as history with its V3 vector files)
> ARTIFACT FILES            = this document + docs/automation/evidence/I-52-ct21d-fpspec-v1-vectors-v4.gen.py (reference encoder, schema registry
>                             and normative tables, SHA-256 56e28e7bd57fa405f97c476ad4ad65bc34b22c59edac0596f4863f293422656f) + docs/automation/evidence/I-52-ct21d-fpspec-v1-vectors-v4.json
>                             (NORMATIVE vectors, SHA-256 41a7c89ea8a24bc5417543dc5a12349a456879f75ecd6c6954b69220a28c4042); both SHA-256 over the LF-normalized bytes
> STATUS AUTHORITY          = the BA-11 registry entry of this artifact is the only authority for its status and hashes;
>                             this header is frozen, non-authoritative text of the candidate bytes (BA-11 V4 section 2)
> AUTHORITY_CONTRACT        = CT-21D V2.1 + V2.2 + V2.3 + V2.4 (draft revision 3) + V2.5 (draft revision 1); V2.4 and V2.5 pending
>                             Coordinator textual verification (clauses used: V2.1 6, 11.1, 11.3, 28; V2.2 PA-8, PA-9)
> DELTA RULING APPLIED      = decisions section 214 (AR3-17, AR3-18, AR3-19, AR3-20, AR3-21, AR3-30; AR3-11 and AR3-25 where they reach this
>                             artifact); section 211 (AR2-32..AR2-36) still applies where section 214 does not change it
> PHASE-2 RULING (V3)       = BA05-F01..F11 (AR2-32..AR2-36), kept; the remnants of F08 and F09 are closed in this version (section 7)
> DEPENDS_ON                = BA-01 only (BA-11 V4 section 2.1). No value of this artifact is defined in BA-08 (AR3-17): the tolerance slots are
>                             named here and valued only in the kind baseline; only the edge BA-08 -> BA-05 exists
> PREREQUISITES             = the library entity-type census (section 5; host work, not authorized): before the candidate, because its classes
>                             enter this draft (section 2.6); the Architect's answers to AQ-V4-08, AQ-V4-09 and AQ-V4-10, or their
>                             recorded deferral (before the candidate)
> BLOCKER                   = NB-3 (ADVANCES_TO_BASELINE_ARTIFACT_PREPARATION; OPEN: the library census requires host work)
> CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
> ```

## 1. Two layers with different jobs

| Layer | Name | Job | Decides equivalence? |
|---|---|---|---|
| 1 | `EXACT_STATE_FINGERPRINT` | preserved-state comparison: `PRE_COMMAND_STATE_RECORD` against the state after an abort, pre-existing definitions, `REUSED_BOUND`, manifest additions, `LIBRARY_EQUIVALENCE_OBSERVATION`, cross-session determinism | **no**: equal digests = **equal over the fingerprinted fields** (section 2.4 lists what is not fingerprinted); unequal = different |
| 2 | `SEMANTIC_COMPARISON` | expected versus persisted product semantics (S-1, S-2, S-3, the comparator of SL-6) | **yes**, by a typed domain comparator, **never** by a digest |

A signed-zero or other bit-level difference between domain-equivalent values is not a difference of layer 2 and must not produce O4; it may be recorded as `BIT_LEVEL_DIFFERENCE_DOMAIN_EQUIVALENT`.

## 2. Layer 1: `EXACT_STATE_FINGERPRINT`

### 2.1 Canonical byte stream (normative)

`stream = "FPSPEC-V1|EXACT|" + <record type> + "|" + fields`, and for each field **in ordinal (UTF-8 byte) order of the field name** (V2.1 28.1 "property keys inside a record are in sorted ordinal order"; AR2-32): `<name>` `=` `<value>` LF. The order in which a producer supplies the fields, and the order in which section 2.3 lists them, are irrelevant (vectors EV-21 = EV-22). The digest is SHA-256 of the stream, lowercase hexadecimal. The reference encoder is `I-52-ct21d-fpspec-v1-vectors-v4.gen.py`; an implementation of instrument I-12 is conformant **only** if, from the `input` of every vector of section 2.7, it reproduces every digest byte for byte, every `UNKNOWN` and every `REJECTED`.

Three results exist. **Digest.** **`UNKNOWN`**: the state cannot be fingerprinted (section 2.5); never a partial digest. **`REJECTED`**: the producer's input violates a field table or a conditional rule of section 2.3 (a missing or extra field, a conditional field present where it must be absent, a key or item not in its form); no digest and no `UNKNOWN`, and a conformant I-12 never emits such a record (vectors RJ-01..RJ-06).

| Value kind | Encoding |
|---|---|
| integer | decimal ASCII, `-` for negatives, `0` for zero, no leading zeros |
| boolean | `1` or `0` |
| double | 16 lowercase hexadecimal digits of the IEEE-754 binary64 bit pattern, big-endian. `-0.0` and `+0.0` **differ**. NaN and infinities are **unsupported**: the record is `UNKNOWN`, never a digest |
| string, enum | `S` + decimal count of UTF-8 bytes + `:` + the UTF-8 bytes of the **exact stored text** (no normalization, no case folding; an enum by its stored name). A string that is **not well-formed UTF-16** (a lone surrogate) is **`UNKNOWN`** (AR2-33; vector EV-25) |
| raw bytes | `B` + decimal byte count + `:` + the bytes |
| digest | the 64 lowercase hexadecimal digits |
| absent / null | `~` |
| ordered sequence | `[` + decimal element count + LF + each element encoded and followed by LF + `]` |
| unordered set of strings | the elements sorted by **ordinal order = Unicode scalar value order = UTF-8 byte order** (never UTF-16 code-unit order and never culture order; AR2-33; vectors EV-23, EV-24), then encoded as an ordered sequence; a duplicate member is `UNKNOWN` (vector EV-72) |
| unordered set of records | sorted by the declared key field of the record (UTF-8 byte order), then encoded as an ordered sequence; a duplicate key is `UNKNOWN` (vector EV-70) |
| nested record inline | `{` + record type + LF + its fields (ordinal order) + `}` |
| nested definition by reference | `@` + the name encoded as a string + `#` + its 64-hex digest. When the referenced definition is `UNKNOWN`, the referring record is `UNKNOWN` (section 2.5; AR3-18; vectors EV-67, EV-68) |
| typed value (`TYPED_VALUE`) | an inline record with `kind` (one of `ABSENT`, `BOOL`, `INT`, `DOUBLE`, `STRING`, `NAME`, `BYTES`, `POINT3D`, `COLOR`) and `value` encoded by that kind; used for maps (pairs) and polymorphic values |

#### 2.1.1 Maps and pairs

A map (the dynamic properties of a reference, the variables of a dimension style) is an **unordered set of records** keyed by the name, each record carrying `name` and a `TYPED_VALUE`. There is no other pair encoding. The per-entity overrides of a dimension are **not** a map: they are the stored `RB` sequence of section 2.3.2.

#### 2.1.2 ResultBuffer typed values (`RB`)

A stored `ResultBuffer` (the data of an `Xrecord`, and the `DSTYLE` section of section 2.3.2) is encoded as an ordered sequence of `RB` records `{code, value}` in host order; **chunk boundaries and non-text entries are preserved** (vectors EV-26 != EV-27, EV-27 != EV-28). This lossless `(code, value)` sequence is the Architect's reading of the contract's "raw stored bytes" (V2.1 28.1; **AR3-21**). The value kind comes from the DXF group-code range, and the host value of the entry must be of the host type of its range:

| Codes | Kind | Host type of the value |
|---|---|---|
| 0-4 | string | String |
| 6-9 | string | String |
| 10-18 | point3d | Point3d |
| 19-59 | double | Double |
| 60-79 | int | Int16, Int32 or Int64 |
| 90-99 | int | Int16, Int32 or Int64 |
| 100-100 | string | String |
| 102-102 | string | String |
| 110-112 | point3d | Point3d |
| 113-149 | double | Double |
| 160-169 | int | Int16, Int32 or Int64 |
| 170-179 | int | Int16, Int32 or Int64 |
| 210-210 | point3d | Point3d |
| 211-239 | double | Double |
| 270-289 | int | Int16, Int32 or Int64 |
| 290-299 | bool | Boolean |
| 300-309 | string | String |
| 310-319 | bytes | byte[] |
| 370-389 | int | Int16, Int32 or Int64 |
| 400-409 | int | Int16, Int32 or Int64 |
| 410-419 | string | String |
| 420-429 | int | Int16, Int32 or Int64 |
| 430-439 | string | String |
| 440-459 | int | Int16, Int32 or Int64 |
| 460-469 | double | Double |
| 470-479 | string | String |
| 999-1003 | string | String |
| 1004-1004 | bytes | byte[] |
| 1006-1009 | string | String |
| 1010-1013 | point3d | Point3d |
| 1014-1059 | double | Double |
| 1060-1071 | int | Int16, Int32 or Int64 |

**Unsupported, therefore `UNKNOWN`:** handle and object-id codes (5, 105, 320-369, 390-399, 480-481, 1005) and every code outside the table; a host value whose type is not the one of its code range. In detail (AR3-21): code **5** (an entity handle), 105 and 1005 are handles, not text (vectors EV-76, EV-30); the **point-valued codes** 10-18, 110-112, 210 and 1010-1013 carry a host `Point3d` and are encoded as an inline `POINT3D` record (vector EV-77); a value whose host type is not the one of its range (for example a `Double` under code 70) is `UNKNOWN`, never coerced (vector EV-78). The host types of the ranges are confirmed on the exact build by the census and the qualification of I-12 (section 5); a correction before the seal follows section 2.6.

#### 2.1.3 Host values as typed values

A value read from the host as an object (the value of a dynamic property, the value of a context variable) becomes a `TYPED_VALUE` by its host type:

| Host type of the value | `TYPED_VALUE` kind |
|---|---|
| null | `ABSENT` |
| System.Boolean | `BOOL` |
| System.Int16, System.Int32, System.Int64 | `INT` |
| System.Double | `DOUBLE` |
| System.String | `STRING` |
| Point3d | `POINT3D` |
| any other type (for example Point2d, Vector3d, ObjectId, byte[]) | `UNKNOWN` |

The kinds `NAME`, `COLOR` and `BYTES` are used only where a table declares them (the variables of section 2.3.3); the host never supplies them through this table. Vectors EV-49, EV-69, EV-71, EV-80, EV-81.

### 2.2 Ordering (normative)

| Container | Order |
|---|---|
| ordered entity containers (the entity sequence of a block table record) | **the host iteration order** as reported (V2.1 28.1; V2.2 PA-9). It is **not necessarily the drawing order**: the drawing order of a `SortentsTable` is outside layer 1 (section 2.4; AR2-34) |
| unordered containers (symbol-table name sets, dictionary entries, sibling sets, maps, an `ENUMERATION` with `ordered = 0`) | stored-name or key order, ordinal (UTF-8 byte order); the **encoder sorts**, so the order in which a producer supplies them is irrelevant (vectors EV-15 = EV-16, EV-61 = EV-73) |
| fields of a record | ordinal order of the field name (section 2.1) |

### 2.3 Record types (schema version 1; field tables)

The field tables below are generated from the schema registry of the reference encoder (`SCHEMAS`), which is the normative source; every cell is identical to the registry (a literal `|` inside a cell is written `\|`; the check is mechanical, section 7). A record whose field set differs from its table is `REJECTED`. `ENTITY_*` records carry the common entity fields (`dxfName`, `layer`, `linetype`, `linetypeScale`, `lineweight`, `visible`, the colour fields and the transparency fields) **merged with** their concrete fields; the merged set is streamed in ordinal order. The record type of a host object is chosen by its **exact host class** (section 2.3.1); an object of any other class is **`UNKNOWN`** (section 2.5).

#### Production record types

**`DEFINITION_DUMP`**

| Field | Value kind | Content |
|---|---|---|
| `name` | string | block name (stored case) |
| `originX` | double | - |
| `originY` | double | - |
| `originZ` | double | - |
| `explodable` | boolean | - |
| `scaling` | enum (stored name as a string) | stored name of the host BlockScaling |
| `units` | enum (stored name as a string) | stored name of the host UnitsValue |
| `annotative` | enum (stored name as a string) | stored name of the host AnnotativeStates |
| `entities` | ordered sequence of inline `ENTITY_*` records | every entity of the record in HOST ITERATION ORDER, each an inline ENTITY_* record chosen by its exact host class (sections 2.2, 2.3.1) |

**`ENTITY_LINE`**

| Field | Value kind | Content |
|---|---|---|
| `dxfName` | string | DXF class name as the host reports it (recorded; the record type comes from the exact host class, section 2.3.1) |
| `layer` | string | layer name (stored case) |
| `linetype` | string | linetype name (stored case) |
| `linetypeScale` | double | - |
| `lineweight` | enum (stored name as a string) | stored name of the host LineWeight value |
| `visible` | boolean | - |
| `colorMethod` | enum (stored name as a string) | stored name of the host ColorMethod |
| `colorIndex` | integer | ACI index as the host reports it |
| `colorRgb` | integer | R*65536 + G*256 + B when colorMethod = ByColor, else absent |
| `colorBookName` | string | color book name, or absent |
| `colorName` | string | color name in the book, or absent |
| `transparencyMethod` | enum (stored name as a string) | stored name of the host TransparencyMethod |
| `transparencyAlpha` | integer | alpha 0-255 when the method is ByAlpha, else absent |
| `startX` | double | - |
| `startY` | double | - |
| `startZ` | double | - |
| `endX` | double | - |
| `endY` | double | - |
| `endZ` | double | - |
| `normalX` | double | - |
| `normalY` | double | - |
| `normalZ` | double | - |
| `thickness` | double | - |

**`ENTITY_ARC`**

| Field | Value kind | Content |
|---|---|---|
| `dxfName` | string | DXF class name as the host reports it (recorded; the record type comes from the exact host class, section 2.3.1) |
| `layer` | string | layer name (stored case) |
| `linetype` | string | linetype name (stored case) |
| `linetypeScale` | double | - |
| `lineweight` | enum (stored name as a string) | stored name of the host LineWeight value |
| `visible` | boolean | - |
| `colorMethod` | enum (stored name as a string) | stored name of the host ColorMethod |
| `colorIndex` | integer | ACI index as the host reports it |
| `colorRgb` | integer | R*65536 + G*256 + B when colorMethod = ByColor, else absent |
| `colorBookName` | string | color book name, or absent |
| `colorName` | string | color name in the book, or absent |
| `transparencyMethod` | enum (stored name as a string) | stored name of the host TransparencyMethod |
| `transparencyAlpha` | integer | alpha 0-255 when the method is ByAlpha, else absent |
| `centerX` | double | - |
| `centerY` | double | - |
| `centerZ` | double | - |
| `radius` | double | - |
| `startAngle` | double | - |
| `endAngle` | double | - |
| `normalX` | double | - |
| `normalY` | double | - |
| `normalZ` | double | - |
| `thickness` | double | - |

**`ENTITY_CIRCLE`**

| Field | Value kind | Content |
|---|---|---|
| `dxfName` | string | DXF class name as the host reports it (recorded; the record type comes from the exact host class, section 2.3.1) |
| `layer` | string | layer name (stored case) |
| `linetype` | string | linetype name (stored case) |
| `linetypeScale` | double | - |
| `lineweight` | enum (stored name as a string) | stored name of the host LineWeight value |
| `visible` | boolean | - |
| `colorMethod` | enum (stored name as a string) | stored name of the host ColorMethod |
| `colorIndex` | integer | ACI index as the host reports it |
| `colorRgb` | integer | R*65536 + G*256 + B when colorMethod = ByColor, else absent |
| `colorBookName` | string | color book name, or absent |
| `colorName` | string | color name in the book, or absent |
| `transparencyMethod` | enum (stored name as a string) | stored name of the host TransparencyMethod |
| `transparencyAlpha` | integer | alpha 0-255 when the method is ByAlpha, else absent |
| `centerX` | double | - |
| `centerY` | double | - |
| `centerZ` | double | - |
| `radius` | double | - |
| `normalX` | double | - |
| `normalY` | double | - |
| `normalZ` | double | - |
| `thickness` | double | - |

**`ENTITY_LWPOLYLINE`**

| Field | Value kind | Content |
|---|---|---|
| `dxfName` | string | DXF class name as the host reports it (recorded; the record type comes from the exact host class, section 2.3.1) |
| `layer` | string | layer name (stored case) |
| `linetype` | string | linetype name (stored case) |
| `linetypeScale` | double | - |
| `lineweight` | enum (stored name as a string) | stored name of the host LineWeight value |
| `visible` | boolean | - |
| `colorMethod` | enum (stored name as a string) | stored name of the host ColorMethod |
| `colorIndex` | integer | ACI index as the host reports it |
| `colorRgb` | integer | R*65536 + G*256 + B when colorMethod = ByColor, else absent |
| `colorBookName` | string | color book name, or absent |
| `colorName` | string | color name in the book, or absent |
| `transparencyMethod` | enum (stored name as a string) | stored name of the host TransparencyMethod |
| `transparencyAlpha` | integer | alpha 0-255 when the method is ByAlpha, else absent |
| `closed` | boolean | - |
| `plinegen` | boolean | - |
| `elevation` | double | - |
| `thickness` | double | - |
| `normalX` | double | - |
| `normalY` | double | - |
| `normalZ` | double | - |
| `vertices` | ordered sequence of inline `PLINE_VERTEX` records | vertices in host order |

**`ENTITY_TEXT`**

| Field | Value kind | Content |
|---|---|---|
| `dxfName` | string | DXF class name as the host reports it (recorded; the record type comes from the exact host class, section 2.3.1) |
| `layer` | string | layer name (stored case) |
| `linetype` | string | linetype name (stored case) |
| `linetypeScale` | double | - |
| `lineweight` | enum (stored name as a string) | stored name of the host LineWeight value |
| `visible` | boolean | - |
| `colorMethod` | enum (stored name as a string) | stored name of the host ColorMethod |
| `colorIndex` | integer | ACI index as the host reports it |
| `colorRgb` | integer | R*65536 + G*256 + B when colorMethod = ByColor, else absent |
| `colorBookName` | string | color book name, or absent |
| `colorName` | string | color name in the book, or absent |
| `transparencyMethod` | enum (stored name as a string) | stored name of the host TransparencyMethod |
| `transparencyAlpha` | integer | alpha 0-255 when the method is ByAlpha, else absent |
| `textString` | string | exact stored text |
| `height` | double | - |
| `rotation` | double | - |
| `widthFactor` | double | - |
| `oblique` | double | - |
| `styleName` | string | text style name |
| `horizontalMode` | enum (stored name as a string) | stored name of the host TextHorizontalMode |
| `verticalMode` | enum (stored name as a string) | stored name of the host TextVerticalMode |
| `mirroredInX` | boolean | - |
| `mirroredInY` | boolean | - |
| `thickness` | double | - |
| `positionX` | double | - |
| `positionY` | double | - |
| `positionZ` | double | - |
| `alignmentX` | double | - |
| `alignmentY` | double | - |
| `alignmentZ` | double | - |
| `normalX` | double | - |
| `normalY` | double | - |
| `normalZ` | double | - |

**`ENTITY_MTEXT`**

| Field | Value kind | Content |
|---|---|---|
| `dxfName` | string | DXF class name as the host reports it (recorded; the record type comes from the exact host class, section 2.3.1) |
| `layer` | string | layer name (stored case) |
| `linetype` | string | linetype name (stored case) |
| `linetypeScale` | double | - |
| `lineweight` | enum (stored name as a string) | stored name of the host LineWeight value |
| `visible` | boolean | - |
| `colorMethod` | enum (stored name as a string) | stored name of the host ColorMethod |
| `colorIndex` | integer | ACI index as the host reports it |
| `colorRgb` | integer | R*65536 + G*256 + B when colorMethod = ByColor, else absent |
| `colorBookName` | string | color book name, or absent |
| `colorName` | string | color name in the book, or absent |
| `transparencyMethod` | enum (stored name as a string) | stored name of the host TransparencyMethod |
| `transparencyAlpha` | integer | alpha 0-255 when the method is ByAlpha, else absent |
| `contents` | string | raw stored contents with format codes |
| `textHeight` | double | - |
| `width` | double | - |
| `rotation` | double | - |
| `attachment` | enum (stored name as a string) | stored name of the host AttachmentPoint |
| `flowDirection` | enum (stored name as a string) | stored name of the host FlowDirection |
| `styleName` | string | - |
| `lineSpacingFactor` | double | - |
| `lineSpacingStyle` | enum (stored name as a string) | stored name of the host LineSpacingStyle |
| `locationX` | double | - |
| `locationY` | double | - |
| `locationZ` | double | - |
| `directionX` | double | - |
| `directionY` | double | - |
| `directionZ` | double | - |
| `normalX` | double | - |
| `normalY` | double | - |
| `normalZ` | double | - |

**`ENTITY_INSERT`**

| Field | Value kind | Content |
|---|---|---|
| `dxfName` | string | DXF class name as the host reports it (recorded; the record type comes from the exact host class, section 2.3.1) |
| `layer` | string | layer name (stored case) |
| `linetype` | string | linetype name (stored case) |
| `linetypeScale` | double | - |
| `lineweight` | enum (stored name as a string) | stored name of the host LineWeight value |
| `visible` | boolean | - |
| `colorMethod` | enum (stored name as a string) | stored name of the host ColorMethod |
| `colorIndex` | integer | ACI index as the host reports it |
| `colorRgb` | integer | R*65536 + G*256 + B when colorMethod = ByColor, else absent |
| `colorBookName` | string | color book name, or absent |
| `colorName` | string | color name in the book, or absent |
| `transparencyMethod` | enum (stored name as a string) | stored name of the host TransparencyMethod |
| `transparencyAlpha` | integer | alpha 0-255 when the method is ByAlpha, else absent |
| `blockName` | string | the definition name; for a dynamic reference, the name of its dynamic block definition (never the anonymous *U name) |
| `blockDigest` | nested definition by reference | reference to the DEFINITION_DUMP digest of that definition (nested-digest field); UNKNOWN when that definition is UNKNOWN (section 2.5, AR3-18) |
| `positionX` | double | - |
| `positionY` | double | - |
| `positionZ` | double | - |
| `scaleX` | double | - |
| `scaleY` | double | - |
| `scaleZ` | double | - |
| `rotation` | double | - |
| `normalX` | double | - |
| `normalY` | double | - |
| `normalZ` | double | - |
| `dynamicProperties` | unordered set of inline `DYNPROP` records keyed by `name` (section 2.3.2) | the WRITABLE dynamic properties of the reference (section 2.3.2), sorted by name; empty for a static reference |
| `attributes` | ordered sequence of inline `ENTITY_ATTRIB` records | attribute references in host iteration order (exact host class AcDbAttribute, section 2.3.1) |

**`ENTITY_ATTRIB`**

| Field | Value kind | Content |
|---|---|---|
| `dxfName` | string | DXF class name as the host reports it (recorded; the record type comes from the exact host class, section 2.3.1) |
| `layer` | string | layer name (stored case) |
| `linetype` | string | linetype name (stored case) |
| `linetypeScale` | double | - |
| `lineweight` | enum (stored name as a string) | stored name of the host LineWeight value |
| `visible` | boolean | - |
| `colorMethod` | enum (stored name as a string) | stored name of the host ColorMethod |
| `colorIndex` | integer | ACI index as the host reports it |
| `colorRgb` | integer | R*65536 + G*256 + B when colorMethod = ByColor, else absent |
| `colorBookName` | string | color book name, or absent |
| `colorName` | string | color name in the book, or absent |
| `transparencyMethod` | enum (stored name as a string) | stored name of the host TransparencyMethod |
| `transparencyAlpha` | integer | alpha 0-255 when the method is ByAlpha, else absent |
| `tag` | string | - |
| `textString` | string | - |
| `height` | double | - |
| `rotation` | double | - |
| `widthFactor` | double | - |
| `oblique` | double | - |
| `styleName` | string | - |
| `horizontalMode` | enum (stored name as a string) | - |
| `verticalMode` | enum (stored name as a string) | - |
| `invisible` | boolean | - |
| `lockPosition` | boolean | - |
| `positionX` | double | - |
| `positionY` | double | - |
| `positionZ` | double | - |
| `alignmentX` | double | - |
| `alignmentY` | double | - |
| `alignmentZ` | double | - |
| `normalX` | double | - |
| `normalY` | double | - |
| `normalZ` | double | - |

**`ENTITY_ATTDEF`**

| Field | Value kind | Content |
|---|---|---|
| `dxfName` | string | DXF class name as the host reports it (recorded; the record type comes from the exact host class, section 2.3.1) |
| `layer` | string | layer name (stored case) |
| `linetype` | string | linetype name (stored case) |
| `linetypeScale` | double | - |
| `lineweight` | enum (stored name as a string) | stored name of the host LineWeight value |
| `visible` | boolean | - |
| `colorMethod` | enum (stored name as a string) | stored name of the host ColorMethod |
| `colorIndex` | integer | ACI index as the host reports it |
| `colorRgb` | integer | R*65536 + G*256 + B when colorMethod = ByColor, else absent |
| `colorBookName` | string | color book name, or absent |
| `colorName` | string | color name in the book, or absent |
| `transparencyMethod` | enum (stored name as a string) | stored name of the host TransparencyMethod |
| `transparencyAlpha` | integer | alpha 0-255 when the method is ByAlpha, else absent |
| `tag` | string | - |
| `prompt` | string | - |
| `textString` | string | default value |
| `height` | double | - |
| `rotation` | double | - |
| `widthFactor` | double | - |
| `oblique` | double | - |
| `styleName` | string | - |
| `horizontalMode` | enum (stored name as a string) | - |
| `verticalMode` | enum (stored name as a string) | - |
| `invisible` | boolean | - |
| `constant` | boolean | - |
| `verify` | boolean | - |
| `preset` | boolean | - |
| `lockPosition` | boolean | - |
| `positionX` | double | - |
| `positionY` | double | - |
| `positionZ` | double | - |
| `alignmentX` | double | - |
| `alignmentY` | double | - |
| `alignmentZ` | double | - |
| `normalX` | double | - |
| `normalY` | double | - |
| `normalZ` | double | - |

**`ENTITY_ROTATED_DIMENSION`**

| Field | Value kind | Content |
|---|---|---|
| `dxfName` | string | DXF class name as the host reports it (recorded; the record type comes from the exact host class, section 2.3.1) |
| `layer` | string | layer name (stored case) |
| `linetype` | string | linetype name (stored case) |
| `linetypeScale` | double | - |
| `lineweight` | enum (stored name as a string) | stored name of the host LineWeight value |
| `visible` | boolean | - |
| `colorMethod` | enum (stored name as a string) | stored name of the host ColorMethod |
| `colorIndex` | integer | ACI index as the host reports it |
| `colorRgb` | integer | R*65536 + G*256 + B when colorMethod = ByColor, else absent |
| `colorBookName` | string | color book name, or absent |
| `colorName` | string | color name in the book, or absent |
| `transparencyMethod` | enum (stored name as a string) | stored name of the host TransparencyMethod |
| `transparencyAlpha` | integer | alpha 0-255 when the method is ByAlpha, else absent |
| `xLine1PointX` | double | - |
| `xLine1PointY` | double | - |
| `xLine1PointZ` | double | - |
| `xLine2PointX` | double | - |
| `xLine2PointY` | double | - |
| `xLine2PointZ` | double | - |
| `dimLinePointX` | double | - |
| `dimLinePointY` | double | - |
| `dimLinePointZ` | double | - |
| `textPositionX` | double | - |
| `textPositionY` | double | - |
| `textPositionZ` | double | - |
| `rotation` | double | - |
| `oblique` | double | - |
| `dimensionStyleName` | string | - |
| `dimensionText` | string | the text override as stored (empty = measured) |
| `usingDefaultTextPosition` | boolean | - |
| `normalX` | double | - |
| `normalY` | double | - |
| `normalZ` | double | - |
| `overrides` | ordered sequence of `RB` records, or absent | per-entity dimension-variable overrides: the typed values of the DSTYLE section of the entity's ACAD extended data, in host order (section 2.3.2); empty when there is no DSTYLE section, never absent; the host-derived *D block is excluded |

**`ENTITY_ALIGNED_DIMENSION`**

| Field | Value kind | Content |
|---|---|---|
| `dxfName` | string | DXF class name as the host reports it (recorded; the record type comes from the exact host class, section 2.3.1) |
| `layer` | string | layer name (stored case) |
| `linetype` | string | linetype name (stored case) |
| `linetypeScale` | double | - |
| `lineweight` | enum (stored name as a string) | stored name of the host LineWeight value |
| `visible` | boolean | - |
| `colorMethod` | enum (stored name as a string) | stored name of the host ColorMethod |
| `colorIndex` | integer | ACI index as the host reports it |
| `colorRgb` | integer | R*65536 + G*256 + B when colorMethod = ByColor, else absent |
| `colorBookName` | string | color book name, or absent |
| `colorName` | string | color name in the book, or absent |
| `transparencyMethod` | enum (stored name as a string) | stored name of the host TransparencyMethod |
| `transparencyAlpha` | integer | alpha 0-255 when the method is ByAlpha, else absent |
| `xLine1PointX` | double | - |
| `xLine1PointY` | double | - |
| `xLine1PointZ` | double | - |
| `xLine2PointX` | double | - |
| `xLine2PointY` | double | - |
| `xLine2PointZ` | double | - |
| `dimLinePointX` | double | - |
| `dimLinePointY` | double | - |
| `dimLinePointZ` | double | - |
| `textPositionX` | double | - |
| `textPositionY` | double | - |
| `textPositionZ` | double | - |
| `oblique` | double | - |
| `dimensionStyleName` | string | - |
| `dimensionText` | string | - |
| `usingDefaultTextPosition` | boolean | - |
| `normalX` | double | - |
| `normalY` | double | - |
| `normalZ` | double | - |
| `overrides` | ordered sequence of `RB` records, or absent | as ENTITY_ROTATED_DIMENSION (section 2.3.2) |

**`SYMBOL_RECORD_LAYER`**

| Field | Value kind | Content |
|---|---|---|
| `table` | enum (stored name as a string) | LAYER |
| `name` | string | stored case |
| `colorMethod` | enum (stored name as a string) | stored name of the host ColorMethod |
| `colorIndex` | integer | ACI index as the host reports it |
| `colorRgb` | integer | R*65536 + G*256 + B when colorMethod = ByColor, else absent |
| `colorBookName` | string | color book name, or absent |
| `colorName` | string | color name in the book, or absent |
| `linetype` | string | linetype name |
| `lineweight` | enum (stored name as a string) | - |
| `isOff` | boolean | - |
| `isFrozen` | boolean | - |
| `isLocked` | boolean | - |
| `isPlottable` | boolean | - |
| `transparencyMethod` | enum (stored name as a string) | stored name of the host TransparencyMethod |
| `transparencyAlpha` | integer | alpha 0-255 when the method is ByAlpha, else absent |

**`SYMBOL_RECORD_LTYPE`**

| Field | Value kind | Content |
|---|---|---|
| `table` | enum (stored name as a string) | LTYPE |
| `name` | string | - |
| `asciiDescription` | string | - |
| `patternLength` | double | - |
| `isScaledToFit` | boolean | - |
| `dashes` | ordered sequence of inline `LTYPE_DASH` records | in host order |

**`SYMBOL_RECORD_STYLE`**

| Field | Value kind | Content |
|---|---|---|
| `table` | enum (stored name as a string) | STYLE |
| `name` | string | - |
| `fileName` | string | - |
| `bigFontFileName` | string | - |
| `textSize` | double | - |
| `xScale` | double | width factor |
| `obliquingAngle` | double | - |
| `isVertical` | boolean | - |
| `isShapeFile` | boolean | - |
| `flagBits` | integer | - |
| `fontTypeface` | string | - |
| `fontBold` | boolean | - |
| `fontItalic` | boolean | - |
| `fontCharacterSet` | integer | - |
| `fontPitchAndFamily` | integer | - |

**`SYMBOL_RECORD_DIMSTYLE`**

| Field | Value kind | Content |
|---|---|---|
| `table` | enum (stored name as a string) | DIMSTYLE |
| `name` | string | - |
| `dimvars` | unordered set of inline `DIMVAR_VALUE` records keyed by `name` | EVERY variable of the closed list of section 2.3.3, sorted by name |

**`SYMBOL_RECORD_APPID`**

| Field | Value kind | Content |
|---|---|---|
| `table` | enum (stored name as a string) | APPID |
| `name` | string | - |

**`PAYLOAD_EXACT`**

| Field | Value kind | Content |
|---|---|---|
| `owner` | string | the stored name of the definition whose extension dictionary holds the payload |
| `dictKey` | string | the extension-dictionary key (RACKCAD_SELECTIVE) |
| `data` | ordered sequence of `RB` records, or absent | the stored ResultBuffer of the Xrecord: every (code, value) in host order, chunk boundaries and non-text entries preserved; absent when there is no payload |

**`NOD_ENTRY`**

| Field | Value kind | Content |
|---|---|---|
| `path` | string | the dictionary keys from the named-object dictionary root, stored case, joined by "/" |
| `objectClass` | enum (stored name as a string) | XRECORD when the exact host class is AcDbXrecord, DICTIONARY when it is AcDbDictionary; any other class (a subclass included) is UNKNOWN |
| `data` | ordered sequence of `RB` records, or absent | XRECORD: its ResultBuffer (never absent); DICTIONARY: absent |
| `children` | unordered set of strings | DICTIONARY: the child keys (each child is its own NOD_ENTRY; never absent); XRECORD: absent |

**`ENUMERATION`**

| Field | Value kind | Content |
|---|---|---|
| `container` | string | role name of the enumerated container |
| `ordered` | boolean | - |
| `itemForm` | enum (stored name as a string) | NAME, HANDLE or HANDLE_CLASS (section 2.3.6) |
| `items` | ordered sequence of strings | the items in the item form; ordered = 1: host iteration order; ordered = 0: the encoder sorts them in ordinal (UTF-8) order and a duplicate item is UNKNOWN; handle forms are valid only within one session |

**`CONTEXT_VARIABLE`**

| Field | Value kind | Content |
|---|---|---|
| `name` | string | the variable name, upper case, as the kind baseline lists it |
| `value` | record `TYPED_VALUE` (from the host value, section 2.1.3) | the value the host reports, as a TYPED_VALUE (section 2.1.3) |

**`PRE_COMMAND_STATE_RECORD`**

| Field | Value kind | Content |
|---|---|---|
| `kind` | string | the rack kind (selective) |
| `members` | unordered set of inline `STATE_MEMBER` records keyed by `memberKey` | every read-set component, every closure member and every context input (V2.1 6, 11.1, 11.3; section 2.3.5) |

**`PREPARE_RESIDUE_MANIFEST`**

| Field | Value kind | Content |
|---|---|---|
| `cloningMode` | enum (stored name as a string) | the cloning mode the importer declared (stored name) |
| `importsCompleted` | integer | - |
| `additions` | unordered set of inline `MANIFEST_ADDITION` records keyed by `memberKey` | every record added by PREPARE-W with its exact digest |
| `reusedBound` | unordered set of inline `MANIFEST_REUSED` records keyed by `memberKey` | every pre-existing record the imported content binds to (REUSED_BOUND, not residue) |

#### Helper record types (only inside other records)

**`POINT3D`**

| Field | Value kind | Content |
|---|---|---|
| `x` | double | - |
| `y` | double | - |
| `z` | double | - |

**`COLOR`**

| Field | Value kind | Content |
|---|---|---|
| `colorMethod` | enum (stored name as a string) | stored name of the host ColorMethod |
| `colorIndex` | integer | ACI index as the host reports it |
| `colorRgb` | integer | R*65536 + G*256 + B when colorMethod = ByColor, else absent |
| `colorBookName` | string | color book name, or absent |
| `colorName` | string | color name in the book, or absent |

**`TYPED_VALUE`**

| Field | Value kind | Content |
|---|---|---|
| `kind` | enum (stored name as a string) | one of ABSENT, BOOL, INT, DOUBLE, STRING, NAME, BYTES, POINT3D, COLOR |
| `value` | by the kind of the enclosing record | encoded by the kind; absent for ABSENT |

**`RB`**

| Field | Value kind | Content |
|---|---|---|
| `code` | integer | the DXF group code of the typed value |
| `value` | by the kind of the enclosing record | encoded by the kind of the code range (section 2.1.2) |

**`DYNPROP`**

| Field | Value kind | Content |
|---|---|---|
| `name` | string | dynamic property name (stored) |
| `value` | record `TYPED_VALUE` | typed value from the host value (section 2.1.3) |

**`DIMVAR_VALUE`**

| Field | Value kind | Content |
|---|---|---|
| `name` | string | dimension variable name (closed list, section 2.3.3) |
| `value` | record `TYPED_VALUE` | typed value |

**`PLINE_VERTEX`**

| Field | Value kind | Content |
|---|---|---|
| `x` | double | - |
| `y` | double | - |
| `bulge` | double | - |
| `startWidth` | double | - |
| `endWidth` | double | - |

**`LTYPE_DASH`**

| Field | Value kind | Content |
|---|---|---|
| `length` | double | - |
| `shapeNumber` | integer | - |
| `shapeStyleName` | string | or absent |
| `shapeOffsetX` | double | - |
| `shapeOffsetY` | double | - |
| `shapeRotation` | double | - |
| `shapeScale` | double | - |
| `shapeIsUcsOriented` | boolean | - |
| `text` | string | or absent |

**`STATE_MEMBER`**

| Field | Value kind | Content |
|---|---|---|
| `memberKey` | string | category\|recordKind\|expectedName (the set key; "\|" is a literal character) |
| `category` | enum (stored name as a string) | READ_SET, CLOSURE or CONTEXT (section 2.3.5) |
| `recordKind` | string | one of the record kinds of section 2.3.5 |
| `expectedName` | string | the name queried with host symbol-table semantics, or the key of section 2.3.5 |
| `storedName` | string | the stored case of the pre-existing record when PRESENT, else absent |
| `state` | enum (stored name as a string) | PRESENT, ABSENT or UNKNOWN |
| `digest` | 64 lowercase hex digits (no prefix) | the exact digest of the member record (record type of section 2.3.5) when PRESENT, else absent |

**`MANIFEST_ADDITION`**

| Field | Value kind | Content |
|---|---|---|
| `memberKey` | string | as STATE_MEMBER |
| `recordKind` | string | as STATE_MEMBER (section 2.3.5) |
| `storedName` | string | - |
| `digest` | 64 lowercase hex digits (no prefix) | exact digest recorded after the import |

**`MANIFEST_REUSED`**

| Field | Value kind | Content |
|---|---|---|
| `memberKey` | string | as STATE_MEMBER |
| `recordKind` | string | as STATE_MEMBER (section 2.3.5) |
| `storedName` | string | - |
| `preCommandDigest` | 64 lowercase hex digits (no prefix) | the digest of the pre-command record |

#### Test-only record types (exempt from section 2.5; used only by the vectors)

**`TEST_COORD3`**

| Field | Value kind | Content |
|---|---|---|
| `x` | double | - |
| `y` | double | - |
| `z` | double | - |

**`TEST_NAME`**

| Field | Value kind | Content |
|---|---|---|
| `name` | string | - |

**`TEST_OPTIONAL`**

| Field | Value kind | Content |
|---|---|---|
| `name` | string | - |
| `layer` | string | - |

**`TEST_SEQ`**

| Field | Value kind | Content |
|---|---|---|
| `items` | ordered sequence of strings | - |

**`TEST_SET`**

| Field | Value kind | Content |
|---|---|---|
| `names` | unordered set of strings | - |

**`TEST_OUTER`**

| Field | Value kind | Content |
|---|---|---|
| `label` | string | - |
| `inner` | inline record `TEST_COORD3` | - |

**`TEST_REFHOLDER`**

| Field | Value kind | Content |
|---|---|---|
| `ref` | nested definition by reference | - |

**`TEST_ORDER`**

| Field | Value kind | Content |
|---|---|---|
| `zeta` | integer | - |
| `alpha` | integer | - |

#### 2.3.1 Host class to record type (closed map; BA05V3-N05)

The record type of a host entity is chosen by **exact equality of its host class name** (the `RXClass` name, for example `AcDbBlockReference`): never by a subclass test (`is`, `IsDerivedFrom`) and never by the DXF name. The `dxfName` field still records the DXF name the host reports.

| Host class (`RXClass` name) | Managed class (informative) | Record type | Admitted as |
|---|---|---|---|
| `AcDbLine` | Line | `ENTITY_LINE` | an entity of a block table record (`DEFINITION_DUMP.entities`) |
| `AcDbArc` | Arc | `ENTITY_ARC` | an entity of a block table record (`DEFINITION_DUMP.entities`) |
| `AcDbCircle` | Circle | `ENTITY_CIRCLE` | an entity of a block table record (`DEFINITION_DUMP.entities`) |
| `AcDbPolyline` | Polyline (DXF name LWPOLYLINE) | `ENTITY_LWPOLYLINE` | an entity of a block table record (`DEFINITION_DUMP.entities`) |
| `AcDbText` | DBText | `ENTITY_TEXT` | an entity of a block table record (`DEFINITION_DUMP.entities`) |
| `AcDbMText` | MText | `ENTITY_MTEXT` | an entity of a block table record (`DEFINITION_DUMP.entities`) |
| `AcDbBlockReference` | BlockReference | `ENTITY_INSERT` | an entity of a block table record (`DEFINITION_DUMP.entities`) |
| `AcDbAttributeDefinition` | AttributeDefinition | `ENTITY_ATTDEF` | an entity of a block table record (`DEFINITION_DUMP.entities`) |
| `AcDbRotatedDimension` | RotatedDimension | `ENTITY_ROTATED_DIMENSION` | an entity of a block table record (`DEFINITION_DUMP.entities`) |
| `AcDbAlignedDimension` | AlignedDimension | `ENTITY_ALIGNED_DIMENSION` | an entity of a block table record (`DEFINITION_DUMP.entities`) |
| `AcDbAttribute` | AttributeReference | `ENTITY_ATTRIB` | an attribute of a reference (`ENTITY_INSERT.attributes`) only |

**Every other class is `UNKNOWN`**, a subclass of a listed class included: for example `AcDbMInsertBlock` (DXF name `INSERT`) and `AcDbTable`, both subclasses of `AcDbBlockReference` (vector EV-65), and `AcDbProxyEntity` (vector EV-51). A subclass that has its own row maps by its own row: `AcDbAttributeDefinition` (a subclass of `AcDbText`) is `ENTITY_ATTDEF`, never `ENTITY_TEXT` (vector EV-66). The classes RackCad itself draws (evidence `sa2`: `BlockReference`, `DBText`, `RotatedDimension`, `Polyline`, `Circle`; AR2-36) are all in the map. For objects of the named-object dictionary the same rule applies: `AcDbXrecord` is `XRECORD`, `AcDbDictionary` is `DICTIONARY`, every other class (for example `AcDbDictionaryWithDefault`) is `UNKNOWN` (vector EV-75).

#### 2.3.2 Extraction of the map-valued and stored-override fields (BA05V3-N06)

**`ENTITY_INSERT.dynamicProperties`.** The members are the entries of the reference's dynamic-property collection whose **read-only flag is false** (the writable properties: the ones a writer can assign, which is what the product assigns, `LateralHeaderDrawer.cs:430-436` at `3375aadb`). Each becomes a `DYNPROP` with the stored property name and the typed value of its host value (section 2.1.3). Read-only entries are not fingerprinted (section 2.4), so two read-only entries with the same name do not matter (vector EV-49). Two **writable** entries with the same name make the record `UNKNOWN` (a duplicate key; vector EV-70): a disclosed availability cost, whose occurrence in the library the census records (section 5). A writable property whose host value has no typed-value kind (for example a `Point2d`) is `UNKNOWN` (vector EV-71). A static reference has an empty set.

**`overrides` of `ENTITY_ROTATED_DIMENSION` and `ENTITY_ALIGNED_DIMENSION`.** The source is the **stored** override set, not an effective value compared with the style; this is this draft's reading, pending AQ-V4-10 (the Architect's open question on the source of dimension overrides). The stored set is the typed values of the entity's extended data of the registered application `ACAD` that lie inside its `DSTYLE` section (after the string `DSTYLE` and the opening control string `{`, up to the matching closing control string `}`), in host order, encoded as the `RB` sequence of section 2.1.2 (so an override equal to the style value is still state). An entity without a `DSTYLE` section has an empty sequence (never absent, vector RJ-05); a malformed section, or a value of an unsupported code (for example a handle, code 1005), is `UNKNOWN` (vector EV-83; a disclosed availability cost). This is the host's stored form of per-entity dimension-variable overrides; the product sets such overrides when the style is not named (`LateralHeaderDrawer.cs:362-372` at `3375aadb`); the census and the qualification of I-12 confirm the storage on the exact build (section 5), and a different storage changes this table before the seal (section 2.6). The `DSTYLE` section is carved out of the extended-data boundary of section 2.4. Vectors EV-52, EV-53.

#### 2.3.3 Closed list of dimension variables of `SYMBOL_RECORD_DIMSTYLE`

`DIMADEC` (INT), `DIMALT` (BOOL), `DIMALTD` (INT), `DIMALTF` (DOUBLE), `DIMALTMZF` (DOUBLE), `DIMALTMZS` (STRING), `DIMALTRND` (DOUBLE), `DIMALTTD` (INT), `DIMALTTZ` (INT), `DIMALTU` (INT), `DIMALTZ` (INT), `DIMAPOST` (STRING), `DIMARCSYM` (INT), `DIMASZ` (DOUBLE), `DIMATFIT` (INT), `DIMAUNIT` (INT), `DIMAZIN` (INT), `DIMBLK` (NAME), `DIMBLK1` (NAME), `DIMBLK2` (NAME), `DIMCEN` (DOUBLE), `DIMCLRD` (COLOR), `DIMCLRE` (COLOR), `DIMCLRT` (COLOR), `DIMDEC` (INT), `DIMDLE` (DOUBLE), `DIMDLI` (DOUBLE), `DIMDSEP` (INT), `DIMEXE` (DOUBLE), `DIMEXO` (DOUBLE), `DIMFRAC` (INT), `DIMFXL` (DOUBLE), `DIMFXLON` (BOOL), `DIMGAP` (DOUBLE), `DIMJOGANG` (DOUBLE), `DIMJUST` (INT), `DIMLDRBLK` (NAME), `DIMLFAC` (DOUBLE), `DIMLIM` (BOOL), `DIMLTEX1` (NAME), `DIMLTEX2` (NAME), `DIMLTYPE` (NAME), `DIMLUNIT` (INT), `DIMLWD` (INT), `DIMLWE` (INT), `DIMMZF` (DOUBLE), `DIMMZS` (STRING), `DIMPOST` (STRING), `DIMRND` (DOUBLE), `DIMSAH` (BOOL), `DIMSCALE` (DOUBLE), `DIMSD1` (BOOL), `DIMSD2` (BOOL), `DIMSE1` (BOOL), `DIMSE2` (BOOL), `DIMSOXD` (BOOL), `DIMTAD` (INT), `DIMTDEC` (INT), `DIMTFAC` (DOUBLE), `DIMTFILL` (INT), `DIMTFILLCLR` (COLOR), `DIMTIH` (BOOL), `DIMTIX` (BOOL), `DIMTM` (DOUBLE), `DIMTMOVE` (INT), `DIMTOFL` (BOOL), `DIMTOH` (BOOL), `DIMTOL` (BOOL), `DIMTOLJ` (INT), `DIMTP` (DOUBLE), `DIMTSZ` (DOUBLE), `DIMTVP` (DOUBLE), `DIMTXSTY` (NAME), `DIMTXT` (DOUBLE), `DIMTXTDIRECTION` (BOOL), `DIMTZIN` (INT), `DIMUPT` (BOOL), `DIMZIN` (INT). A variable the host cannot read is `UNKNOWN` (the record has no digest). `NAME` values are the stored names of the referenced records (blocks, linetypes, text styles).

#### 2.3.4 Record-level `ABSENT` and `UNKNOWN` members

In a `PRE_COMMAND_STATE_RECORD` a member whose record does not exist has `state = ABSENT` and no digest (vector EV-62); a member whose record cannot be read has `state = UNKNOWN`, and then **the whole record is `UNKNOWN`** and PREPARE-R refuses the run (O1, V2.1 6; vector EV-63). A field value that is absent is `~` (vector EV-12). The contract's "raw stored bytes" of the payload (V2.1 28.1) is realized as the lossless `RB` sequence of section 2.1.2, because the host stores an `Xrecord` as a typed `ResultBuffer`, not as bytes (BA05-F04; accepted as the Architect's reading, AR3-21).

#### 2.3.5 Categories and record kinds of `STATE_MEMBER` (BA05V3-N07)

| Category | Meaning |
|---|---|
| `READ_SET` | a component of the abort read-set (V2.1 11.1) as the kind baseline specifies it |
| `CLOSURE` | a member of the static dependency closure: the pre-existing homologue or `ABSENT` (V2.1 11.3) |
| `CONTEXT` | a plan-time input the kind baseline captures in the record for its oracle (for Selective: the consumed context variables and the presentation of the source top-level reference, AR3-11); whether ABORT-VERIFY or E-08 compares a `CONTEXT` member is fixed by the kind baseline (BA-08 V4 section 6 proposes an answer, pending AQ-V4-08) |

| `recordKind` | Record type of `digest` | `expectedName` |
|---|---|---|
| `BLOCK` | `DEFINITION_DUMP` | the block name |
| `LAYER` | `SYMBOL_RECORD_LAYER` | the layer name |
| `LTYPE` | `SYMBOL_RECORD_LTYPE` | the linetype name |
| `STYLE` | `SYMBOL_RECORD_STYLE` | the text style name |
| `DIMSTYLE` | `SYMBOL_RECORD_DIMSTYLE` | the dimension style name |
| `APPID` | `SYMBOL_RECORD_APPID` | the registered application name |
| `PAYLOAD` | `PAYLOAD_EXACT` | the name of the definition that owns the payload |
| `NOD_ENTRY` | `NOD_ENTRY` | the NOD path |
| `ENUMERATION` | `ENUMERATION` | the container role name |
| `CONTEXT_VARIABLE` | `CONTEXT_VARIABLE` | the variable name |
| `REFERENCE` | `ENTITY_INSERT` | the handle of the reference (session-local, item form HANDLE) |

`memberKey` = `category|recordKind|expectedName` (a literal `|`); another form is `REJECTED` (vector RJ-03). **Bindings and addresses** (V2.1 11.1: "per-view `RackViewAddress` and grouping") have **no record kind of their own** (the V3 kind `BINDING` is removed): the persisted address (`Kind`, `View`, `Section`) and the grouping identity (`Id`) are members of the envelope that the product writes on each view definition (`RackEmbedDocument.cs:38-50` and `RackDefinitionCreator.cs:197-204` at `3375aadb`), so they are inside the exact `RB` sequence of that definition's `PAYLOAD` member, and any change to them changes its digest. The decoded address is a function of those bytes (the total codec `RackViewCodec`) and is not a separate layer-1 record. **The class of a reference** in a reference set is carried by the `ENUMERATION` item form `HANDLE_CLASS` (section 2.3.6; vector EV-74). A `REFERENCE` member is the exact `ENTITY_INSERT` record of one reference (vector EV-82): it carries the layer, colour, linetype, linetype scale, lineweight, transparency and visibility of the reference, but **not its plot style name**, which is outside layer 1 (section 2.4); a kind baseline that needs the plot style name (V17 ST-13 under a named plot-style table) declares that gap or asks for a table change before the seal (section 2.6).

#### 2.3.6 `ENUMERATION` item forms (BA05V3-N07, N08)

| `itemForm` | Item | Pattern |
|---|---|---|
| `NAME` | the stored name | - |
| `HANDLE` | the handle in lowercase hexadecimal without leading zeros | `^[1-9a-f][0-9a-f]{0,15}$` |
| `HANDLE_CLASS` | the handle as in HANDLE, a colon, and the exact host class name (RXClass name) of the object | `^[1-9a-f][0-9a-f]{0,15}:[A-Za-z0-9_]+$` |

With `ordered = 0` the **encoder** sorts the items in ordinal (UTF-8) order (handles therefore sort as strings, not as numbers) and a duplicate item is `UNKNOWN`; with `ordered = 1` the items are in host iteration order. An item not in its form is `REJECTED` (vector RJ-06). Handle forms are valid only within one session and are excluded from cross-session determinism (V2.1 28.1). Vectors EV-61, EV-73, EV-74.

### 2.4 Exclusions and coverage boundary (BA05-F11)

**Excluded:** `ObjectId`; database-local handles (except inside `ENUMERATION`, `REFERENCE` keys and `memberKey` within one session); timestamps; volatile identifiers; reactors and host-local pointers; anonymous block table records (`*U`, `*D`, any host-generated anonymous name) as definitions.

**Not fingerprinted (the coverage boundary of "equal over the fingerprinted fields"):** extension-dictionary entries of a definition other than the RackCad payload (including the `ACAD_SORTENTS` drawing-order table and dynamic-block data); extended data and extension dictionaries of entities, **except** the `DSTYLE` section of the `ACAD` extended data of a dimension (field `overrides`, section 2.3.2); the read-only dynamic properties of a reference (section 2.3.2); entity properties not listed in the tables (for example the plot style name, material and shadow properties, hyperlinks; for the plot style name BA-08 V4 declares the gap, pending AQ-V4-09); the metadata of an `Xrecord` beyond its data (merge style, ownership flags); symbol-record properties not listed (for example layer descriptions and viewport overrides). A change confined to these is not detected by layer 1.

### 2.5 Unknown and unsupported (rule for classes outside the tables)

A proxy entity, a custom entity, an object whose exact class is not in section 2.3.1, a non-finite number, an ill-formed string, an unsupported `RB` code or host type, a host value without a typed-value kind, a duplicate set member or key, or an unreadable record yields **`UNKNOWN`**, never a partial digest. Consequences:

- a member of the **static dependency closure** with such content makes the closure `UNKNOWN` and **refuses the run (O1)** (V2.1 6);
- a **pre-existing homologue** of the user's drawing with such content makes the record `UNKNOWN` and refuses the run (O1): an **availability cost** disclosed in Owner Act 2;
- **`UNKNOWN` propagates through `blockDigest` (AR3-18).** A definition that references, at any depth, a definition that is `UNKNOWN` is itself `UNKNOWN` (vectors EV-67, EV-68). So a RackCad view definition, or a pre-existing homologue, that references a library block holding a class without a table is `UNKNOWN`, and the run is refused (O1). Because the digest of a RackCad definition chains through the digests of the library blocks it references, the **library census (section 5) is a precondition of every such digest**;
- **two sources of the class list (AR2-36):** the **library census** (section 5) and the **static derivation of the classes RackCad draws** in every kind whose definitions can be in the read-set (evidence `sa2`: `BlockReference`, `DBText`, `RotatedDimension`, `Polyline` and `Circle`; Selective, Dynamic and Push Back draw the first three, Cantilever and RACKSECCION the last two). Every product-drawn class is in section 2.3.1, so a RackCad-owned definition of any kind is not refused **for its product-drawn content**; it is still `UNKNOWN` when a library block it references is (previous bullet);
- a payload whose chunking split a surrogate pair (the product chunks at 255 UTF-16 units) is `UNKNOWN` (vector EV-31): a disclosed availability cost. The same holds for two writable dynamic properties with the same name (EV-70) and for a handle value in a `DSTYLE` section (EV-83).

### 2.6 Versioning (BA05-F02, BA05V3-N09)

| When | Version effect |
|---|---|
| **before the seal**: a type table, a class-map row, a code range or a vector is added, changed or regenerated (for example after the census) | a new `ARTIFACT_VERSION` of BA-05; `FINGERPRINT_SPEC_VERSION` stays **`FPSPEC-V1`** and the domain separator stays; vectors **may be added or regenerated** (a changed table changes its own vectors and every vector that nests them through `blockDigest` or member digests). V3 regenerated EV-12, EV-17, EV-19 and EV-20 against V2; this version regenerates EV-52, EV-53 and EV-60 to EV-64 and adds EV-65 to EV-83 and RJ-01 to RJ-06 (section 7) |
| **at the seal** | the vectors are **frozen** with the sealed bytes |
| **after the seal** | any change is a new `FINGERPRINT_SPEC_VERSION` (`FPSPEC-V2`, new domain separator) and a **new baseline** |

### 2.7 NORMATIVE exact vectors

The JSON file is **authoritative for the exact bytes** through the field **`streamHex`** of each vector; `streamDisplay` is informative only (backslash doubled, LF shown as `\n`) (BA05-F07). Every vector carries its producer-side **`input`** (conventions in the JSON field `encoding.inputConventions`: a double as `{"binary64": <16 hex>}`, a host point as `{"point3d": [...]}` or `{"point2d": [...]}`, an ill-formed string as its UTF-16 code units, an `RB` entry as `[code, value]`, an entity as `[host class, fields]`, a nested reference as `[name, digest or "UNKNOWN"]`), so a conformance harness feeds the same input to I-12. These vectors become normative when this artifact is sealed. 58 digests and 17 `UNKNOWN` results, 75 vectors in all, plus 6 `REJECTED` inputs; at least one digest per production record type (the encoder asserts it).

| Id | Purpose | Record type | SHA-256 or result |
|---|---|---|---|
| EV-01 | baseline coordinate (+0.0 components) | `TEST_COORD3` | `ba4e9ed7478785ce06e8e0010bfbd14970b9e87cbc923f63108fa6009acdae5e` |
| EV-02 | SIGNED ZERO: y = -0.0 must differ from EV-01 | `TEST_COORD3` | `bc2cf0352fc258115745f43a0d7127011c2d692231696af4a25b4bd244626c11` |
| EV-03 | 1 ulp above 1.0 must differ from EV-01 | `TEST_COORD3` | `23ec89504f6f855fda794064fae9df813679bb4de17a347f9d36dce2d607c42d` |
| EV-04 | SUBNORMAL: smallest positive subnormal | `TEST_COORD3` | `e2a2a8b5b8faf7e5c36bedd923cf2a93af3a298c69abad649c7a1a7f8f7d09b0` |
| EV-05 | MAXIMUM FINITE double | `TEST_COORD3` | `8443fcaab8e6744212cc983a39231e856c418cdc91e01695f9d4d0439187dffd` |
| EV-06 | NaN is unsupported: UNKNOWN, no digest | `TEST_COORD3` | **UNKNOWN** (non-finite double) |
| EV-07 | infinity is unsupported: UNKNOWN, no digest | `TEST_COORD3` | **UNKNOWN** (non-finite double) |
| EV-08 | string, exact case | `TEST_NAME` | `b625be899405392b55556bc8bff40e70a463190fbaf52e28460df64a91d9627a` |
| EV-09 | string, case differs from EV-08: must differ | `TEST_NAME` | `80f92df041b0eddb78d33c471bd4b1fc0a815e2eb30dde3c0a4c47844945870f` |
| EV-10 | EMPTY STRING | `TEST_NAME` | `4ea81bbc8c2e91b3d939861286eb0b5713bc2310917bb6e33361f09a8b885924` |
| EV-11 | MULTIBYTE string (UTF-8, no normalization) | `TEST_NAME` | `1ae4a2037f5489626a3275ecaf007dd75373f89af1489347d58421404ab50ef3` |
| EV-12 | ABSENT field value (field layer streams before name: ordinal field order) | `TEST_OPTIONAL` | `36c09d5bb858fe8ffa97a0f28e0cbf806959897643e5e94db0ebbeb091f89e98` |
| EV-13 | ORDERED SEQUENCE (host order is state) | `TEST_SEQ` | `d7504a38b410755341ed7aa6b0c6950b3ecdb42bb2535d2908a6d846b9e51063` |
| EV-14 | ordered sequence in the other order: must differ from EV-13 | `TEST_SEQ` | `01d971c4f2b3a0dd7d370bcf827d996985ae0f6707050a5b9407b687bf31f1e0` |
| EV-15 | UNORDERED container given as b,a: sorted by stored name | `TEST_SET` | `ab9751f8933fd5a2802d0309781c80b34161586758da7854cc1f47dba9a7c9de` |
| EV-16 | unordered container given as a,b: must EQUAL EV-15 | `TEST_SET` | `ab9751f8933fd5a2802d0309781c80b34161586758da7854cc1f47dba9a7c9de` |
| EV-17 | NESTED record inline (field inner streams before label) | `TEST_OUTER` | `8c120f364bfe895b1a2dee9a02c8832deadd787d72ca20c205e7329a9e331681` |
| EV-18 | NESTED definition referenced by name and digest | `TEST_REFHOLDER` | `b3bd691805fcf4e27a0db961be0da70e1fd5d90842dd484513eb4d13e6183164` |
| EV-19 | PAYLOAD_EXACT, one text chunk (code 1), no unknown member | `PAYLOAD_EXACT` | `aff990496d5edb07ff330c3c01dcbe7e3c3c1ac13d2cd49082fc94a394f3cc81` |
| EV-20 | PAYLOAD_EXACT with an UNKNOWN / FORWARD-COMPATIBLE member: must differ from EV-19 | `PAYLOAD_EXACT` | `90d3fefba07d7338d7e2d86d01854b0365d599a9b9808390f3a143d897789219` |
| EV-21 | FIELD ORDER: fields supplied as (zeta, alpha) stream as alpha, zeta | `TEST_ORDER` | `f1919b6fc7a6e32bcd83675072bbcb528dbe53117575fcdf6efc60d8781530b0` |
| EV-22 | fields supplied as (alpha, zeta): must EQUAL EV-21 (ordinal field order, not supply or schema order) | `TEST_ORDER` | `f1919b6fc7a6e32bcd83675072bbcb528dbe53117575fcdf6efc60d8781530b0` |
| EV-23 | CASE-MIXED unordered set b,B,a,A: sorted A,B,a,b (UTF-8 byte order) | `TEST_SET` | `af7a2f0c29798c8eee91fb84456f0c54a8474d866c4af05bd3dd48be939c458d` |
| EV-24 | U+1F600 and U+FF21: UTF-8 order puts U+FF21 first (UTF-16 code-unit order would put U+1F600 first) | `TEST_SET` | `25d74443039a84afe56998bd47edcb7e904fafd8d2e8fcdd7a365b95e500faaf` |
| EV-25 | LONE SURROGATE (ill-formed UTF-16): UNKNOWN, no digest | `TEST_NAME` | **UNKNOWN** (string is not well-formed UTF-16 (lone surrogate)) |
| EV-26 | PAYLOAD_EXACT, same text split in two chunks ("ab" + "c"): must differ from one chunk | `PAYLOAD_EXACT` | `b82a8df1b878539b8a9d564e2c9f71eef1e125e59259c9c06a97151e4b602ae1` |
| EV-27 | PAYLOAD_EXACT, one chunk "abc" | `PAYLOAD_EXACT` | `a4c852883142ee0f451aa8503ba844d9f769c2ba16512c76a91a3cad531d9619` |
| EV-28 | PAYLOAD_EXACT, same text with type code 300 instead of 1: must differ from EV-27 | `PAYLOAD_EXACT` | `21c152a7d737cbbba15990a1d7b0fbb17ba7fa815e5e99fb1ecd80ebdce48e9e` |
| EV-29 | PAYLOAD_EXACT, no payload: data absent | `PAYLOAD_EXACT` | `9cf5bb3248b95c6a010f1a771357a721bfd5d3128ece419eefd17cc14a22553f` |
| EV-30 | PAYLOAD_EXACT with a handle-valued entry (code 1005): UNKNOWN | `PAYLOAD_EXACT` | **UNKNOWN** (RB code 1005 (extended-data handle) is a volatile identifier) |
| EV-31 | PAYLOAD_EXACT whose chunking split a surrogate pair (RackBlockData chunks at 255 UTF-16 units): UNKNOWN | `PAYLOAD_EXACT` | **UNKNOWN** (string is not well-formed UTF-16 (lone surrogate)) |
| EV-40 | ENTITY_LINE | `ENTITY_LINE` | `21a7b93317b70859a49c4eaf66f83a0cd061239918bbcd952e1e239091b6a5b3` |
| EV-41 | ENTITY_ARC | `ENTITY_ARC` | `5c737004ef762b8c0ffee395ae0d32881437b7577fed69b0af731233765087f2` |
| EV-42 | ENTITY_CIRCLE | `ENTITY_CIRCLE` | `ab35af53476d555e1feb4ef8e3cbe508bcdd21bf5d12f564c1be8a3ebb355021` |
| EV-43 | ENTITY_LWPOLYLINE | `ENTITY_LWPOLYLINE` | `1b0c147265dc5ec5b102fe76378abac46d88010da8037a9bc503ebbb6d98451c` |
| EV-44 | ENTITY_TEXT | `ENTITY_TEXT` | `66bdad8149e2fe7e46ee4d1ad1eec4d7e4ae5d3d6415aa084bdee58ecc2a734b` |
| EV-45 | ENTITY_MTEXT | `ENTITY_MTEXT` | `583d7e4b35b3b8d31f23818ae98be18e2cd3c7956b27723aae0aa9a4f76bc851` |
| EV-46 | ENTITY_ATTRIB | `ENTITY_ATTRIB` | `244c80e7951987f8b967184d4de62966eebbdf7a8c79f76a1588c395c366c188` |
| EV-47 | ENTITY_ATTDEF | `ENTITY_ATTDEF` | `1f41aa134ca5eb198bb3b74e26ac1b8dedbcd6cacba04e4b996e5851fcdfe258` |
| EV-48 | DEFINITION_DUMP (one LINE, host class AcDbLine) | `DEFINITION_DUMP` | `b31c3550049136622fbccf010d5c90d51a981df57a4cac48b2757606dc67b9f4` |
| EV-49 | ENTITY_INSERT with nested-digest field and dynamic properties: only the WRITABLE ones (FRENTE before LONGITUD); two read-only Origin entries with the same name are not fingerprinted and do not make it UNKNOWN | `ENTITY_INSERT` | `fc0f8fdef5c610e9cf88e68d5d5ec6362bb0f5000df9975c7aee12ce7c276211` |
| EV-50 | DEFINITION_DUMP holding an INSERT (nested definition by reference) and a TEXT, in host iteration order | `DEFINITION_DUMP` | `f51d99b36401200e585b2e421ea6a3629b3de59e0e8879dc200882bf1f607d0c` |
| EV-51 | DEFINITION_DUMP holding an entity of a class outside the map (a proxy, AcDbProxyEntity): UNKNOWN | `DEFINITION_DUMP` | **UNKNOWN** (host class AcDbProxyEntity has no record type (exact class, section 2.3.1)) |
| EV-52 | ENTITY_ROTATED_DIMENSION; overrides = the DSTYLE pairs as stored, in host order (illustrative pairs) | `ENTITY_ROTATED_DIMENSION` | `b528f4898adcf10366355b29d88597c0928f04a4408688aed802f9da9ffc2eeb` |
| EV-53 | ENTITY_ALIGNED_DIMENSION | `ENTITY_ALIGNED_DIMENSION` | `e426f3c586f8351f6d8f3b8fd752fdee72569d69ef218f23657c23e65b882da0` |
| EV-54 | SYMBOL_RECORD_LAYER | `SYMBOL_RECORD_LAYER` | `087023825ee388b5969f85f88be6b95da37e8fb00b7f0c6ba368dc89b91b2856` |
| EV-55 | SYMBOL_RECORD_LTYPE | `SYMBOL_RECORD_LTYPE` | `98c57e7db8a5192a35674b352c4d67946f23dc5064efebc8945f9cfb141f19ad` |
| EV-56 | SYMBOL_RECORD_STYLE | `SYMBOL_RECORD_STYLE` | `39e47384546fd9e832a1e322df84642b79dfdf851d529ad1a9e4ba87d04228b1` |
| EV-57 | SYMBOL_RECORD_DIMSTYLE with every variable of the closed list (illustrative values) | `SYMBOL_RECORD_DIMSTYLE` | `1423f9ee723868053442d1c0defd694cbabbfbfb9789ccb3e0161c7bddf4b848` |
| EV-58 | SYMBOL_RECORD_APPID | `SYMBOL_RECORD_APPID` | `cabe53b3432232e3fe0cf106002246d9c03602fe583e95b9cb401b7d39e3d1e2` |
| EV-59 | NOD_ENTRY, an XRECORD (RACKCAD_PROJECT, as the product writes it: text chunks of code 1) | `NOD_ENTRY` | `8c245e3263eb6231cde7faef67c3d131e85cba9e6c1d8cf2727bb9b1654f9710` |
| EV-60 | NOD_ENTRY, a DICTIONARY: the host dictionary ACAD_GROUP with two synthetic group keys (children sorted) | `NOD_ENTRY` | `22d585022d3b93308ca0184f169d9843cc014b676dfb40da795ad7d8d6003b5d` |
| EV-61 | ENUMERATION, unordered names given sorted | `ENUMERATION` | `5ec9eabd51f232b1e176b59ef5313b153724d67d548efcf81b93a97a1cbea28e` |
| EV-62 | PRE_COMMAND_STATE_RECORD (synthetic names): read-set definition and payload (EV-50, EV-19), an ABSENT annotation layer, an ABSENT closure block, the pre-existing layer 0 (EV-79), and two CONTEXT members (EV-80, EV-82) | `PRE_COMMAND_STATE_RECORD` | `69acdb3d56d16fbb8586fdfda799b56a8f8d94d47be9f030e4a1801bd4bc0fe0` |
| EV-63 | PRE_COMMAND_STATE_RECORD with an UNKNOWN member: UNKNOWN | `PRE_COMMAND_STATE_RECORD` | **UNKNOWN** (a member of the PRE_COMMAND_STATE_RECORD is UNKNOWN: no partial digest (refusal O1, V2.1 6)) |
| EV-64 | PREPARE_RESIDUE_MANIFEST (synthetic names): the import adds the closure block of EV-62 (EV-48) and binds to the pre-existing layer 0 (EV-79) | `PREPARE_RESIDUE_MANIFEST` | `94dd996f6eeec0fd28d1dcbeb6bf7d68bbd2d625e306504501a3b77fc8bfd8ea` |
| EV-65 | DEFINITION_DUMP holding an AcDbMInsertBlock (DXF name INSERT, a subclass of AcDbBlockReference): UNKNOWN by the exact-class map | `DEFINITION_DUMP` | **UNKNOWN** (host class AcDbMInsertBlock has no record type (exact class, section 2.3.1)) |
| EV-66 | DEFINITION_DUMP holding an AcDbAttributeDefinition (a subclass of AcDbText): ENTITY_ATTDEF by the exact-class map, never ENTITY_TEXT | `DEFINITION_DUMP` | `3e9dfbc44f4a803f2ef2efa4354a343b7c295793e84834bd42f2ef21ec81ea7c` |
| EV-67 | ENTITY_INSERT whose referenced definition is UNKNOWN: UNKNOWN (propagation through blockDigest, AR3-18) | `ENTITY_INSERT` | **UNKNOWN** (the referenced definition POSTE_PROXY is UNKNOWN: UNKNOWN propagates through the nested digest (AR3-18)) |
| EV-68 | DEFINITION_DUMP holding the reference of EV-67: UNKNOWN (propagates to every definition that references it, AR3-18) | `DEFINITION_DUMP` | **UNKNOWN** (the referenced definition POSTE_PROXY is UNKNOWN: UNKNOWN propagates through the nested digest (AR3-18)) |
| EV-69 | ENTITY_INSERT with writable dynamic properties of host types String, Int16 and Double (synthetic names): STRING, INT, DOUBLE (section 2.1.3) | `ENTITY_INSERT` | `a76fa8dc37a20784147bca0c33b5d6bbde160a76ad722459e5a014ef82b0b1a8` |
| EV-70 | ENTITY_INSERT with two WRITABLE dynamic properties of the same name: UNKNOWN (duplicate key) | `ENTITY_INSERT` | **UNKNOWN** (duplicate key in an unordered set of DYNPROP) |
| EV-71 | ENTITY_INSERT with a writable dynamic property whose host value is a Point2d: UNKNOWN (no typed-value kind) | `ENTITY_INSERT` | **UNKNOWN** (a host value of type Point2d has no typed-value kind (section 2.1.3)) |
| EV-72 | unordered set with a DUPLICATE member: UNKNOWN | `TEST_SET` | **UNKNOWN** (duplicate member in an unordered set) |
| EV-73 | ENUMERATION ordered = 0 given UNSORTED: the encoder sorts; must EQUAL EV-61 | `ENUMERATION` | `5ec9eabd51f232b1e176b59ef5313b153724d67d548efcf81b93a97a1cbea28e` |
| EV-74 | ENUMERATION of a reference set with class (item form HANDLE_CLASS; synthetic handles; sorted as strings) | `ENUMERATION` | `cb27e3ef9bdfeb2f21993f496e4d0f1c761b4c93ad829a4c103ad35cddfeab6b` |
| EV-75 | NOD_ENTRY of a class other than AcDbXrecord or AcDbDictionary (AcDbDictionaryWithDefault, a subclass): UNKNOWN | `NOD_ENTRY` | **UNKNOWN** (NOD object of class AcDbDictionaryWithDefault: only AcDbXrecord and AcDbDictionary (exact class) are supported) |
| EV-76 | NOD_ENTRY XRECORD with a code 5 entry (a handle): UNKNOWN (AR3-21) | `NOD_ENTRY` | **UNKNOWN** (RB code 5 (entity handle) is a volatile identifier) |
| EV-77 | NOD_ENTRY XRECORD with a point-valued code (10): an inline POINT3D (AR3-21) | `NOD_ENTRY` | `520ab963764be5776bc1c1cee97adb2cbbfcec080415c907101e374ff17c5f27` |
| EV-78 | NOD_ENTRY XRECORD whose code 70 entry holds a Double (not the integer of its range): UNKNOWN (fail closed) | `NOD_ENTRY` | **UNKNOWN** (RB code 70 holds a host value of type Double, not Int16, Int32 or Int64) |
| EV-79 | SYMBOL_RECORD_LAYER of layer 0 (illustrative properties; the closure layer bound by EV-62 and EV-64) | `SYMBOL_RECORD_LAYER` | `e3d72fcfd19b2d3790603002b70b7562ba58009115f76e3a82e258d355565dc1` |
| EV-80 | CONTEXT_VARIABLE CLAYER, host value a String | `CONTEXT_VARIABLE` | `a0e3612b6164eed4804bc1a4b4fdfdb71b3606aecfe1999c77f49aae0baf09ba` |
| EV-81 | CONTEXT_VARIABLE CELTSCALE, host value a Double | `CONTEXT_VARIABLE` | `d7b4f921bb2bd121486f917d386c546902625e5ab7a8a3dbf481f47966d70bbe` |
| EV-82 | ENTITY_INSERT of the source top-level reference (static; its definition is EV-50): the REFERENCE member of EV-62 | `ENTITY_INSERT` | `bd5de186fe639980ea58fc1e92552a40d10d5a15e2d05059fabd81b4d28656b4` |
| EV-83 | ENTITY_ROTATED_DIMENSION whose DSTYLE section holds a handle value (code 1005): UNKNOWN (a disclosed availability cost) | `ENTITY_ROTATED_DIMENSION` | **UNKNOWN** (RB code 1005 (extended-data handle) is a volatile identifier) |

**Rejected inputs (malformed producer records).**

| Id | Purpose | Record type | Result |
|---|---|---|---|
| RJ-01 | NOD_ENTRY XRECORD with children present | `NOD_ENTRY` | **REJECTED** (NOD_ENTRY XRECORD: data must be present and children absent) |
| RJ-02 | NOD_ENTRY DICTIONARY with data present | `NOD_ENTRY` | **REJECTED** (NOD_ENTRY DICTIONARY: data must be absent and children present) |
| RJ-03 | STATE_MEMBER whose memberKey is not category\|recordKind\|expectedName (written with "/") | `PRE_COMMAND_STATE_RECORD` | **REJECTED** (STATE_MEMBER: memberKey is not category\|recordKind\|expectedName) |
| RJ-04 | record with a missing field (TEST_OPTIONAL without layer) | `TEST_OPTIONAL` | **REJECTED** (TEST_OPTIONAL: fields ['layer', 'name'] expected, got ['name']) |
| RJ-05 | ENTITY_ROTATED_DIMENSION with overrides absent | `ENTITY_ROTATED_DIMENSION` | **REJECTED** (ENTITY_ROTATED_DIMENSION: overrides is never absent (empty when there is no DSTYLE section)) |
| RJ-06 | ENUMERATION HANDLE item with a leading zero | `ENUMERATION` | **REJECTED** (ENUMERATION: item '01f' is not in the form HANDLE) |

Relations that the vectors pin: EV-01 != EV-02 (signed zero); EV-01 != EV-03 (1 ulp); EV-08 != EV-09 (case); EV-13 != EV-14 (sequence order is state); EV-15 == EV-16, EV-21 == EV-22 and EV-61 == EV-73 (set order, field order and the encoder's sort of an unordered enumeration); EV-19 != EV-20 (an unknown member changes the payload digest); EV-26 != EV-27 != EV-28 (chunk boundaries and type codes are state); EV-49 keeps its V3 digest although its input now carries two read-only entries (they are not fingerprinted). Names in EV-60 to EV-64, EV-69, EV-74 and EV-82 are synthetic; the record types, roles and structures follow the product (N11): the RackCad NOD entries are `Xrecord`s (EV-59), the import adds library content and binds to pre-existing records (EV-64), and the annotation layer is added by the seam, not by the import.

## 3. Layer 2: `SEMANTIC_COMPARISON`

### 3.1 Comparator responsibilities and tolerance slots

The comparator parses the persisted record with the **authoritative reader** into its typed form and compares it with the expected typed form computed by the kind baseline's oracle (`ORACLE_SPEC`). It is independent of the writer and of `mu_k`. **This specification names the slots and states no value.** V2.2 PA-9 gives the tolerances and normalizations to the kind baseline: their values, the rule of `NORM_ANGLE` and their status live **only** there (for Selective, BA-08 V4 section 10; AR2-35, **AR3-17**). No value appears in this document or in its vector file (section 3.3). A comparison whose slot has no sealed value in the kind baseline cannot be declared.

| Compared value | Rule | Slot (value, rule and status: BA-08 V4 section 10) |
|---|---|---|
| positions, lengths, insertion points, polyline vertices and widths | predicate `ABS_DIFF_LE(TOL_LENGTH)` | `TOL_LENGTH` |
| text heights (`height`, `textHeight`) | predicate `ABS_DIFF_LE(TOL_TEXT_HEIGHT)` | `TOL_TEXT_HEIGHT` |
| dimension points (`xLine1Point`, `xLine2Point`, `dimLinePoint`, `textPosition`) | predicate `ABS_DIFF_LE(TOL_DIM)` | `TOL_DIM` |
| rotations and other angles | predicate `ANGLE_DIFF_LE(TOL_ANGLE)`, wrapped difference by the rule of `NORM_ANGLE` | `TOL_ANGLE`, `NORM_ANGLE` |
| normals | predicate `NORMAL_ANGLE_LE(TOL_NORMAL)` | `TOL_NORMAL` |
| dynamic-block property values that are lengths | `ABS_DIFF_LE(TOL_DYNAMIC)` | `TOL_DYNAMIC` |
| scale factors | `ABS_DIFF_LE(TOL_SCALE)`, absolute | `TOL_SCALE` |
| texts, names, enum values | `EXACT_STRING` | none |
| sibling sets | unordered | none |
| sequences whose order the plan determines | order-sensitive | none |
| authored payload and envelope | typed members, `ExtensionData` and unknown members (section 3.2), `CANONICAL_JSON_EQUAL` | none |
| unrecognized schema version, unsupported class, unparsable record | `UNKNOWN` | none |

### 3.1.1 Predicates (normative)

| Predicate | Definition |
|---|---|
| `ABS_DIFF_LE(TOL)` | \|a - b\| <= TOL, per scalar component (never a Euclidean distance; AR3-20), absolute (never relative); -0.0 equals +0.0; a non-finite value is UNKNOWN |
| `ANGLE_DIFF_LE(TOL_ANGLE)` | EQUAL when the wrapped difference of the two angles, computed by the rule of the slot NORM_ANGLE, is <= TOL_ANGLE; the rule of NORM_ANGLE is written only in the kind baseline (V2.2 PA-9), this specification names the slot |
| `NORMAL_ANGLE_LE(TOL_NORMAL)` | both vectors normalized (a zero-length vector is UNKNOWN); EQUAL when the angle between them <= TOL_NORMAL |
| `EXACT_STRING` | ordinal equality of the stored text |
| `CANONICAL_JSON_EQUAL` | both sides parsed from the wire text; objects compared as maps with ordinally sorted keys; arrays by position; strings exact; numbers by the exact JSON number token (AR3-19); whitespace not significant; a member present on either side that the authoritative reader does not expose is UNKNOWN_MEMBER_UNSUPPORTED |
| `SCHEMA_VERSION_RECOGNIZED` | an unrecognized schema version makes the comparison UNKNOWN |
| `TYPE_SUPPORTED` | an object whose exact host class is outside section 2.3.1 makes the comparison UNKNOWN |

### 3.2 `ExtensionData`, authored and unknown members (V2.2 PA-8)

The typed form compared for the authored payload and envelope includes `ExtensionData`, the authored and forward-compatible members, and the unknown members preserved by contract (I-11). Unknown and extension members are compared exactly after canonical serialization: object keys in ordinal order, **numbers by their exact JSON number token** (so `1` and `1.0` differ: vector SV-12). This token reading of PA-8 rule 1 "exact numbers" is recorded as the **Architect's interpretation of PA-8 (AR3-19)**. A writer that **drops or alters** an unknown member **does not pass** (DIFFERENT; O4 through S-2 or S-3). A member that cannot be compared yields **`UNKNOWN_MEMBER_UNSUPPORTED`**, surfaced explicitly, giving `UNKNOWN` (O5 path). An unrecognized schema version is `UNKNOWN`.

### 3.3 NORMATIVE semantic comparison vectors (BA05-F08, BA05V3-N01, N10)

Every input is concrete. **Wire** form = the exact persisted JSON text (the comparator parses it with the authoritative reader); **typed** form = a typed value (doubles as the 16-hex binary64 pattern); **host class** = the exact class name of section 2.3.1. SV-06, SV-07, SV-10 and SV-11 are written in **slot units** and carry no tolerance value (AR3-17): each persisted value is an expression in the slot, and its binary64 instance is generated by the `Q-FPSPEC-VECTORS` harness (section 3.3.1).

| Id | Record, field | Form | Expected | Persisted | Predicate | Result | Rule |
|---|---|---|---|---|---|---|---|
| SV-01 | `ENVELOPE` `(unknown member futureKey)` | wire (envelope JSON text) | `{"SchemaVersion":"1.0","Kind":"selective","Id":"g-1","futureKey":1}` | `{"futureKey": 1, "Id": "g-1", "Kind": "selective", "SchemaVersion": "1.0"}` | `CANONICAL_JSON_EQUAL` | **EQUAL** | key order and whitespace are not significant; the unknown member (read into ExtensionData) is present and equal |
| SV-02 | `ENVELOPE` `(unknown member futureKey)` | wire (envelope JSON text) | `{"SchemaVersion":"1.0","Kind":"selective","Id":"g-1","futureKey":1}` | `{"SchemaVersion":"1.0","Kind":"selective","Id":"g-1"}` | `CANONICAL_JSON_EQUAL` | **DIFFERENT** | a writer that DROPS an unknown member does not pass (O4 through S-2/S-3) |
| SV-03 | `ENVELOPE` `(unknown member futureKey)` | wire (envelope JSON text) | `{"SchemaVersion":"1.0","Kind":"selective","Id":"g-1","futureKey":1}` | `{"SchemaVersion":"1.0","Kind":"selective","Id":"g-1","futureKey":2}` | `CANONICAL_JSON_EQUAL` | **DIFFERENT** | a writer that ALTERS an unknown member does not pass |
| SV-04 | `DESIGN` `Bays[0].futureKey` | wire (design JSON text) | `{"SchemaVersion":"1.0","Bays":[{"Levels":[],"futureKey":1}]}` | `{"SchemaVersion":"1.0","Bays":[{"Levels":[],"futureKey":1}]}` | `CANONICAL_JSON_EQUAL` | **UNKNOWN_MEMBER_UNSUPPORTED** | a valid design (SelectiveBayDocument.Levels is a list); the unknown member sits in a bay, whose type declares no ExtensionData (JsonExtensionData is not recursive, I-11), so the reader drops it on both sides: never EQUAL; surfaced explicitly, gives UNKNOWN (O5 path) |
| SV-05 | `ENTITY_TEXT` `positionY` | typed (binary64) | `0000000000000000` | `8000000000000000` | `ABS_DIFF_LE(TOL_LENGTH)` | **EQUAL** | signed zero is domain-equivalent for any tolerance value; secondary finding BIT_LEVEL_DIFFERENCE_DOMAIN_EQUIVALENT; the exact digests differ (EV-01 / EV-02) |
| SV-06 | `ENTITY_LINE` `endX` | typed (binary64), slot units | `4024000000000000` | `expected + 0.5 * TOL_LENGTH` (slot units) | `ABS_DIFF_LE(TOL_LENGTH)` | **EQUAL** | a difference of half the slot is inside it (per component, absolute) |
| SV-07 | `ENTITY_LINE` `endX` | typed (binary64), slot units | `4024000000000000` | `expected + 2 * TOL_LENGTH` (slot units) | `ABS_DIFF_LE(TOL_LENGTH)` | **DIFFERENT** | a difference of twice the slot is outside it; the base value 10 separates the absolute reading from a relative one (a relative tolerance of 10 * TOL_LENGTH would give EQUAL) |
| SV-08 | `ENVELOPE` `SchemaVersion` | wire (envelope JSON text) | `{"SchemaVersion":"1.0","Kind":"selective","Id":"g-1"}` | `{"SchemaVersion":"9.0","Kind":"selective","Id":"g-1"}` | `SCHEMA_VERSION_RECOGNIZED` | **UNKNOWN** | an unrecognized schema version is UNKNOWN |
| SV-09 | `HOST_OBJECT` `(exact host class)` | host class (RXClass name) | `AcDbBlockReference` | `AcDbProxyEntity` | `TYPE_SUPPORTED` | **UNKNOWN** | the persisted object has a class outside section 2.3.1, so no typed form exists: the comparison is UNKNOWN (never EQUAL, never DIFFERENT); the O1 consequence for closure members belongs to section 2.5 |
| SV-10 | `ENTITY_INSERT` `rotation` | typed (binary64, radians), slot units | `0000000000000000` | `expected + 2*pi - 0.5 * TOL_ANGLE` (slot units) | `ANGLE_DIFF_LE(TOL_ANGLE)` | **EQUAL** | the two angles differ by 0.5 * TOL_ANGLE across the wrap at 2pi; EQUAL under any NORM_ANGLE rule that reduces modulo 2pi |
| SV-11 | `ENTITY_INSERT` `normal` | typed (binary64 vector), slot units | `0000000000000000 / 0000000000000000 / 3ff0000000000000` | `(tan(2 * TOL_NORMAL), 0, 1)` (slot units) | `NORMAL_ANGLE_LE(TOL_NORMAL)` | **DIFFERENT** | the angle between the normalized vectors is 2 * TOL_NORMAL; a zero-length vector is UNKNOWN |
| SV-12 | `ENVELOPE` `(unknown member futureKey)` | wire (envelope JSON text) | `{"SchemaVersion":"1.0","Kind":"selective","Id":"g-1","futureKey":1}` | `{"SchemaVersion":"1.0","Kind":"selective","Id":"g-1","futureKey":1.0}` | `CANONICAL_JSON_EQUAL` | **DIFFERENT** | PA-8 rule 1 "exact numbers" read as the exact JSON number token (Architect interpretation, AR3-19): 1 and 1.0 differ |
| SV-13 | `ENTITY_TEXT` `textString` | typed (string) | `Nivel 1` | `NIVEL 1` | `EXACT_STRING` | **DIFFERENT** | texts, names and enum values are compared exactly (stored case) |

#### 3.3.1 Instances of the slot-unit vectors (AR3-17)

SV-06, SV-07, SV-10 and SV-11 carry no tolerance value. The Q-FPSPEC-VECTORS harness instantiates them from the sealed slot values of the kind baseline: persisted = the binary64 nearest (ties to even) to the exact real value of slotExpression, component by component; it then checks, in exact arithmetic, that the realized difference is <= the slot for an EQUAL vector and > the slot for a DIFFERENT vector; an instance that fails the check is INVALID (reported, never PASS). The harness reads the slot values from the **sealed** kind baseline (for Selective, BA-08 section 10 of its sealed version), so the instance depends on that seal and never on this document; the base values are chosen so that the vectors discriminate: 10 for lengths (an absolute reading and a relative one give different results in SV-07), 0 and the wrap at 2pi for rotations, `+Z` for normals. `TOL_SCALE` is not used by any vector and is not a gate of this artifact (AR3-17). The equality results of SV-05 (signed zero) hold for any slot value.

### 3.4 Use of the layers

| Use | Layer |
|---|---|
| S-1, S-2, S-3, the comparator of SL-6 | `SEMANTIC_COMPARISON` |
| E-08 (S-4), S-5, `PRE_COMMAND_STATE_RECORD`, `PREPARE_RESIDUE_MANIFEST`, `REUSED_BOUND`, `LIBRARY_EQUIVALENCE_OBSERVATION` | `EXACT_STATE_FINGERPRINT` |

## 4. Other artifacts (BA05-F10)

`PRE_COMMAND_STATE_RECORD` and `PREPARE_RESIDUE_MANIFEST` are **record types of this specification** (section 2.3); their digests are the ones V2.1 6 and 7 log. The **loaded-code manifest** (E-10, E-11M, E-13), the custody log and the scenario catalog are not fingerprints: this specification does not define their serialization or hashing (the manifest and custody artifact defines its own canonical serialization, AR3-23; the registry defines the artifact hashes).

### 4.1 No full-database fingerprint: `SCOPED_RECORD_COMPARISON` (BA05V3-N12)

This specification defines **no fingerprint of a whole database**: layer 1 is a set of per-record digests with a declared coverage boundary (section 2.4), and any object of a class outside section 2.3.1 is `UNKNOWN` (section 2.5). A before-and-after comparison of database state (for example a dynamic-trace source that looks for net effects) is a **`SCOPED_RECORD_COMPARISON`**:

1. **Scope.** A list, written before the first observation, of the members it covers, each a `(recordKind, key)` of section 2.3.5, plus the `ENUMERATION` record of every container it covers (so that an added or removed record shows as a change of that enumeration).
2. **Observation.** At each observation every member is read as a `STATE_MEMBER` (`PRESENT` with its layer-1 digest, `ABSENT`, or `UNKNOWN`), as in a `PRE_COMMAND_STATE_RECORD`, except that an `UNKNOWN` member does not hide the others: it is reported as `UNKNOWN`.
3. **Result per member.** `EQUAL`, `DIFFERENT`, `ADDED` (`ABSENT` then `PRESENT`), `REMOVED` (`PRESENT` then `ABSENT`) or `UNKNOWN` (either side `UNKNOWN`).
4. **Coverage limit (declared).** Only the declared scope and only the fingerprinted fields are observed: a change outside the scope, or confined to what section 2.4 does not fingerprint, is **not seen**; a member of a class outside section 2.3.1 is `UNKNOWN`. An artifact that uses this comparison names its scope (for example the read-set records of the kind baseline) and repeats this limit.

## 5. Library entity-type census (procedure and schema; NOT performed)

The census is **required before the candidate of this artifact**: its classes are added to this draft as in section 2.6, which changes the bytes (a PREREQUISITE in the registry, BA-11 V4). It reads the library drawing, which requires host work (**not authorized**).

**Universe (AR3-17).** The census covers **every block table record of the library file** (named, anonymous and layout records alike): a superset of any plan's requirement set, defined without reference to any kind baseline. A kind baseline maps its own requirement universe onto the census (the edge runs from the kind baseline to this artifact, never the reverse).

**Procedure (for a future host-authorized gate).** On the private copy of the exact library file of the baseline (a tuple field), read-only:

1. enumerate every block table record of the library file;
2. for each record, record: the exact host class (`RXClass` name) and the DXF name of every entity, with counts; the nested block names; the symbol records referenced; whether any proxy or custom object is present; for every dimension entity, whether its `ACAD` extended data has a `DSTYLE` section and the `(code, host value type)` pairs in it (section 2.3.2); for every dynamic block definition, the dynamic properties a reference to it exposes, read through a reference created in a separate scratch database of the census (never in the library copy): name, read-only flag, host value type, the value set the host reports for the property (its allowed values, or none), and whether two properties share a name;
3. compute the class universe;
4. **add to this draft** (new `ARTIFACT_VERSION`, `FINGERPRINT_SPEC_VERSION` unchanged) a class-map row, a table and at least one vector for every class outside section 2.3.1 that occurs in a non-layout record; a class seen only in layout records is recorded and disclosed (it cannot be a member of a closure);
5. confirm, or correct before the candidate, the host facts this draft leaves to the census: the storage of dimension overrides (section 2.3.2) and the host value types of the `RB` ranges (section 2.1.2).

**Schema of the census record.**

| Field | Content |
|---|---|
| `libraryFileSha256`, `libraryPath`, `censusUtc`, `instrumentBuild`, `dynamicPropertyMethod` | identity of the census and how the dynamic properties were read |
| `blocks[]` | `blockName`, `isLayout`, `isAnonymous`, `isDynamic`, `entityClasses[]` (`rxClassName`, `dxfName`, `count`), `nestedBlocks[]`, `symbolRecords` (`layers[]`, `linetypes[]`, `textStyles[]`, `dimStyles[]`), `dimensionOverrides[]` (`hasDstyleSection`, `pairs[]` of `code` and `hostValueType`), `dynamicProperties[]` (`name`, `readOnly`, `hostValueType`, `allowedValues[]` or absent, `sharesNameWithAnother`), `containsProxyOrCustom` |
| `classUniverse[]` | the distinct `rxClassName` values, each with its `dxfName` values |
| `classesWithoutTable[]` | the classes outside section 2.3.1 at the time of the census, each marked `nonLayout` or `layoutOnly` |

## 6. What keeps NB-3 open

| Id | Item | Gate |
|---|---|---|
| L-1 | the library census (section 5) and the tables of its classes | host (not authorized); **before the candidate** |
| L-2 | **retired in V4.** `TOL_SCALE` is not a gate of BA-05 (AR3-17): it gates the kind baseline and the layer-2 comparisons that need it, and its value and status live only there | - |
| L-3 | candidate ruling, ratifications and seal of this artifact (document + vectors + reference encoder) | baseline review, after the Architect agrees the sealing workflow of BA-11 (AR3-25) |

```text
NB-3 = ADVANCES_TO_BASELINE_ARTIFACT_PREPARATION ; OPEN (library census)
FPSPEC-V1 vectors (V4 files) = 75 exact (58 digests, 17 UNKNOWN) + 6 REJECTED inputs + 13 semantic (4 in slot units; normative when sealed)
CT21D_AUTHORITY_BASELINE_READY = FALSE
```

## 7. Delta V4 (decisions section 214)

Section numbering of V3 is kept; new subsections are 2.1.3, 2.3.1, 2.3.2, 2.3.5, 2.3.6, 3.3.1 and 4.1, and section 7 is new (the text block of section 6 stays where it was). **Mechanical checks of this version** (scripts over the actual bytes, Python 3.13): every cell of the 42 field tables of section 2.3 equals the registry after unescaping `\|`; the reference encoder regenerates the JSON byte for byte; every V3 result is reproduced except the digests of EV-52, EV-53, EV-60, EV-61, EV-62 and EV-64, which changed on purpose (EV-63 stays `UNKNOWN` with a new input; the reasons of EV-30 and EV-51 are reworded); the table check parses every row of every field table, unescapes `\|` and compares the three cells with the registry entry (name, value-kind text, content); neither this document nor the JSON contains a tolerance value; the SHA-256 values of the header are computed over the LF-normalized bytes.

| Finding id | Decision | Fix (section / field) |
|---|---|---|
| BA05-F08 (closure remnant) | AR3-17, AR3-30 | FIXED: SV-04 is a valid design in wire form; every JSON vector is labelled wire; SV-09 has concrete classes; SV-06, SV-07, SV-10, SV-11 are in slot units (3.3, 3.3.1) |
| BA05-F09 (closure remnant) | AR3-17 | FIXED: no tolerance value in this document or the vector file; the `TOL_SCALE` row points to the kind baseline for value and status; L-2 retired; `DEPENDS_ON` = BA-01 (header, 3.1, 6) |
| BA05V3-N01 | AR3-17 | FIXED: SV-06 (`expected + 0.5 * TOL_LENGTH`, EQUAL), SV-07 (`+ 2 * TOL_LENGTH`, DIFFERENT), SV-10 (`2pi - 0.5 * TOL_ANGLE`, EQUAL), SV-11 (angle `2 * TOL_NORMAL`, DIFFERENT); binary64 instances by the Q-FPSPEC-VECTORS harness from the sealed kind-baseline values (3.3, 3.3.1; JSON `semanticSlotInstanceRule`); `TOL_SCALE` row = "value, rule and status: BA-08 V4 section 10" (3.1) |
| BA05V3-N02 | AR3-30, within AR3-17 | FIXED: 3.1 names BA-08's slots per value class (`TOL_TEXT_HEIGHT` for text heights, `TOL_DIM` for dimension points); `ANGLE_DIFF_LE` uses the rule of the slot `NORM_ANGLE`, written only in the kind baseline (3.1.1). The rule is referenced as a slot, like the tolerance values (AR2-35), so no BA-05 -> BA-08 edge arises (AR3-17) |
| BA05V3-N03 | AR3-30 | FIXED: a literal `\|` is escaped in every table cell (`memberKey` reads `category\|recordKind\|expectedName`); every field-table cell is checked mechanically against `SCHEMAS` (2.3, this section) |
| BA05V3-N04 | AR3-21 | FIXED: code 5 is `UNKNOWN`; the point codes 10-18, 110-112, 210, 1010-1013 are inline `POINT3D`; a host value of the wrong type is `UNKNOWN` (2.1.2); vectors EV-76, EV-77, EV-78 |
| BA05V3-N05 | AR3-30 | FIXED: closed map from the exact host class to the record type, every other class `UNKNOWN` (2.3.1); vectors EV-65 (`AcDbMInsertBlock`), EV-66 (`AcDbAttributeDefinition`), EV-51, EV-75 |
| BA05V3-N06 | AR3-30 | FIXED: `dynamicProperties` = the writable properties, duplicates `UNKNOWN` and disclosed (2.3.2); host-type table (2.1.3); `overrides` = the stored `DSTYLE` `RB` sequence, carved out of 2.4 (2.3.2, 2.4); census confirms (5); vectors EV-49, EV-52, EV-53, EV-69, EV-70, EV-71, EV-83 |
| BA05V3-N07 | AR3-30 | FIXED: `recordKind` -> record type table; `BINDING` removed because addresses and grouping are carried by the `PAYLOAD` members; reference class by the item form `HANDLE_CLASS` (2.3.5, 2.3.6); vectors EV-74, EV-82 |
| BA05V3-N08 | AR3-30 | FIXED: the encoder sorts an `ordered = 0` enumeration (EV-73 = EV-61), makes another NOD class `UNKNOWN` (EV-75), rejects violated conditional absences (RJ-01, RJ-02, RJ-05) and pins duplicates (EV-72, EV-70) (2.2, 2.3.1, 2.3.6) |
| BA05V3-N09 | AR3-30 | FIXED: before the seal vectors may be added or regenerated while `FPSPEC-V1` and the domain separator stay; frozen at the seal; any later change is `FPSPEC-V2` and a new baseline (2.6) |
| BA05V3-N10 | AR3-30 | FIXED: SV-04 uses a valid bay (`{"Levels":[],"futureKey":1}`); SV-01..SV-04, SV-08 and SV-12 are wire form; SV-09 names the classes `AcDbBlockReference` and `AcDbProxyEntity` (3.3) |
| BA05V3-N11 | AR3-30 | FIXED: EV-62 uses the `DEFINITION_DUMP` digest EV-50 for the read-set block and `PAYLOAD_EXACT` EV-19; EV-64 has the import add a closure block (EV-48) and bind to layer 0 (`SYMBOL_RECORD_LAYER` EV-79), not the annotation layer; EV-60 is the host dictionary `ACAD_GROUP`; synthetic names are marked (2.7) |
| BA05V3-N12 | AR3-30 (the alternative of the required fix) | FIXED on the BA-05 side: no full-database fingerprint exists; `SCOPED_RECORD_COMPARISON` with a declared scope and coverage limit (4.1). BA-06 V4 must cite it instead of "exact full-database fingerprint" (cross-artifact) |
| BA11V3-01 (BA-05 part) | AR3-17, AR3-25 | FIXED: vectors in slot units; census universe = every block table record of the library file, with no reference to BA-08 (5); `TOL_SCALE` not a BA-05 gate (6); `DEPENDS_ON` = BA-01 and the census as a PREREQUISITE before the candidate (header). The BA-11 rows themselves are BA-11 V4's |
| BA11-F3 (closure; BA-05 content) | AR3-17, AR3-25 | FIXED on the BA-05 side by the same changes: no BA-05 content comes from BA-08 |
| BA11V3-02, BA11V3-07 (mention BA-05) | AR3-23, AR3-25 | NOT_APPLICABLE to BA-05: the BA-07 -> BA-05 edge is removed in BA-07 (its own JCS serialization) and the DEPENDS_ON / PREREQUISITES split is BA-11's; this header already uses the split |
| (support) AR3-18 | AR3-18 | `UNKNOWN` propagates through `blockDigest`; census as a precondition (2.1, 2.5); vectors EV-67, EV-68 |
| (support) AR3-19, AR3-20 | AR3-19, AR3-20 | token reading of PA-8 recorded as the Architect's interpretation (3.2, SV-12); `ABS_DIFF_LE` per scalar component (3.1.1) |
| (support) AR3-11 items 2 and 3 | AR3-11 | the record can carry what the kind baseline captures for its oracle: category `CONTEXT`, record `CONTEXT_VARIABLE`, kind `REFERENCE` (2.3, 2.3.5); vectors EV-62, EV-80, EV-81, EV-82 |
| (support) D-13 of the BA-08 bundle | AR3-30 | the census records each dynamic property's read-only flag, host value type and value set (5), so the kind baseline can predict or declare a constrained value |
| AQ-V4-10 citation: source of dimension overrides (correction pass, minor) | AQ-V4-10 (not decided) | CITED: 2.3.2 marks the stored `DSTYLE` override set as the source of `overrides` as this draft's reading, pending AQ-V4-10 (the Architect's open question on the source of dimension overrides); no field, vector or digest changes |
| AQ-V4-08 citation: `CONTEXT` members in ABORT-VERIFY and E-08 (correction pass, minor) | AQ-V4-08 (not decided) | CITED: 2.3.5 row `CONTEXT`: the choice stays with the kind baseline; BA-08 V4 section 6 proposes an answer, pending AQ-V4-08; no category, record kind or vector changes |
| AQ-V4-09 citation: plot style name (correction pass, minor) | AQ-V4-09 (not decided) | CITED: 2.4, the plot-style-name exclusion: BA-08 V4 declares the gap, pending AQ-V4-09; the field tables and the vectors are unchanged |
| Header `PREREQUISITES` for AQ-V4-08, AQ-V4-09, AQ-V4-10 (correction pass, minor) | AQ-V4-08, AQ-V4-09, AQ-V4-10 (not decided) | ADDED: header `PREREQUISITES` lists the Architect's answers to AQ-V4-08, AQ-V4-09 and AQ-V4-10, or their recorded deferral (before the candidate); `DEPENDS_ON` unchanged |
