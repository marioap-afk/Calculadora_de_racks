# I-52 — CT-21D Baseline Artifact BA-05: Fingerprint Specification FPSPEC-V1 (DRAFT V5)

> **BASELINE ARTIFACT BA-05 V5 — DRAFT, NOT SEALED. Design / documentation only. Nothing here is executed or authorized.**
>
> ```text
> ARTIFACT_ID               = CT21D-BASE-FINGERPRINT-SPEC
> FINGERPRINT_SPEC_VERSION  = FPSPEC-V1 (a tuple field; unchanged by this draft version, see section 2.6)
> ARTIFACT_VERSION          = 5-DRAFT (supersedes 4-DRAFT, blob 5d6e4d0de5e4801af796ba5969b70f3457fb21be)
> ARTIFACT FILES            = this document + docs/automation/evidence/I-52-ct21d-fpspec-v1-vectors-v5.gen.py (reference encoder, schema registry
>                             and normative tables, SHA-256 08cd41d1ba50bc8d8bad2550b6be6309513438839c323a18bd1f4bfd7d7bf949) + docs/automation/evidence/I-52-ct21d-fpspec-v1-vectors-v5.json
>                             (NORMATIVE vectors, SHA-256 87473b0c008370c863f0b4c136939f5d05a84e4bf4913fca85b5f1f7348eb249); both SHA-256 over the LF-normalized bytes;
>                             the V4 document and the V4 vector files stay byte-identical as history
> STATUS AUTHORITY          = the BA-11 registry entry of this artifact is the only authority for its status and hashes;
>                             this header is frozen, non-authoritative text of the candidate bytes (the sealing workflow of the BA-11 registry)
> AUTHORITY_CONTRACT        = CT-21D V2.1 + V2.2 + V2.3 + V2.4 (draft revision 3, textually verified) + V2.5 (draft revision 2); V2.5 pending
>                             Coordinator textual verification (clauses used: V2.1 6, 7.1, 11, 11.3, 28; V2.2 PA-8, PA-9)
> DELTA RULING APPLIED      = decisions section 217 (AR4-08, AR4-09, AR4-10, AR4-11, AR4-17, AR4-20); section 214 (AR3-11, AR3-17..AR3-21,
>                             AR3-25) and section 211 (AR2-32..AR2-36) still apply where section 217 does not change them
> DEPENDS_ON                = BA-01 only (the BA-11 registry). No value of this artifact is defined in BA-08 (AR3-17): the tolerance slots are
>                             named here and valued only in the kind baseline; only the edge BA-08 -> BA-05 exists
> PREREQUISITES             = before the candidate, because they can change the bytes (section 2.6); host work, not authorized: the library census
>                             (section 5) and the confirmations of section 5 step 5 by the census and the I-12 qualification on the exact build
>                             (AR4-10: storage location, host order and equal-to-style behaviour of the stored dimension overrides; the host value
>                             types of the RB ranges; the host types of the context variables read by name). AQ-V4-08, AQ-V4-09 and AQ-V4-10 are
>                             ruled by AR4-08, AR4-09 and AR4-10 (section 217) and are no longer prerequisites
> BLOCKER                   = NB-3 (ADVANCES_TO_BASELINE_ARTIFACT_PREPARATION; OPEN: the census and the qualification confirmations require host work)
> HISTORY                   = phase-2 rulings BA05-F01..F11 (AR2-32..AR2-36) and the V4 delta (section 214) are kept; the V4 delta table stays in the V4 file
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

`stream = "FPSPEC-V1|EXACT|" + <record type> + "|" + fields`, and for each field **in ordinal (UTF-8 byte) order of the field name** (V2.1 28.1 "property keys inside a record are in sorted ordinal order"; AR2-32): `<name>` `=` `<value>` LF. The order in which a producer supplies the fields, and the order in which section 2.3 lists them, are irrelevant (vectors EV-21 = EV-22). The digest is SHA-256 of the stream, lowercase hexadecimal. The reference encoder is `I-52-ct21d-fpspec-v1-vectors-v5.gen.py`; an implementation of instrument I-12 is conformant **only** if, from the `input` of every vector of section 2.7, it reproduces every digest byte for byte, every `UNKNOWN` and every `REJECTED`.

Three results exist. **Digest.** **`UNKNOWN`**: the state cannot be fingerprinted (section 2.5); never a partial digest. **`REJECTED`**: the producer's input violates a field table or a rule of section 2.3 (a missing or extra field; a conditional field present where it must be absent, or absent where it must be present; a value outside a closed list; a key or item not in its form); no digest and no `UNKNOWN`, and a conformant I-12 never emits such a record (vectors RJ-01..RJ-25). **Precedence (normative):** `REJECTED` takes precedence over `UNKNOWN`. A record that violates a rule is `REJECTED` even when it also holds a value that cannot be fingerprinted, wherever that value lies in the record; the reference encoder checks every field and element before it reports `UNKNOWN` (vector RJ-25).

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
| typed value (`TYPED_VALUE`) | an inline record with `kind` (one of `ABSENT`, `BOOL`, `INT`, `DOUBLE`, `STRING`, `NAME`, `BYTES`, `POINT3D`, `COLOR`) and `value` encoded by that kind; the value is absent exactly when the kind is `ABSENT` (else `REJECTED`; vectors RJ-18, RJ-19); used for maps (pairs) and polymorphic values |

#### 2.1.1 Maps and pairs

A map (the dynamic properties of a reference, the variables of a dimension style) is an **unordered set of records** keyed by the name, each record carrying `name` and a `TYPED_VALUE`. There is no other pair encoding. The per-entity overrides of a dimension are **not** a map at layer 1: they are the stored `RB` sequence of section 2.3.2 (layer 2 compares them as a map, section 3.1).

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

**Unsupported, therefore `UNKNOWN`:** handle and object-id codes (5, 105, 320-369, 390-399, 480-481, 1005) and every code outside the table; a host value whose type is not the one of its code range. In detail (AR3-21): code **5** (an entity handle), 105 and 1005 are handles, not text (vectors EV-76, EV-30); the **point-valued codes** 10-18, 110-112, 210 and 1010-1013 carry a host `Point3d` and are encoded as an inline `POINT3D` record (vector EV-77); a value whose host type is not the one of its range (for example a `Double` under code 70) is `UNKNOWN`, never coerced (vector EV-78). The host types of the ranges are confirmed on the exact build by the census and the qualification of I-12 before the candidate (section 5 step 5); a correction follows section 2.6.

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

The kinds `NAME`, `COLOR` and `BYTES` are used only where a table declares them (the variables of section 2.3.3); the host never supplies them through this table. Vectors EV-49, EV-69, EV-71, EV-80, EV-81, EV-87.

**The read of a context variable (normative).** `CONTEXT_VARIABLE.value` is the host system-variable read by name (the managed Application.GetSystemVariable of the variable name), taken while the target document is the active document and its lock is held, with the value exactly as that read returns it; the typed Database properties (for example Database.Clayer, an ObjectId, Database.Cecolor, a Color, Database.Celweight, a LineWeight, Database.Cetransparency, a Transparency) are never the read. The value is then typed by the table above, by the host type that read returns (an `Int16`, `Int32` or `Int64` is `INT`, vector EV-87); because a typed Database property is never the read, no context variable becomes `UNKNOWN` through an `ObjectId`, `Color`, `LineWeight` or `Transparency` object. The expected host type of each variable a kind baseline may capture (the AR3-11 keys, the names of the current text and dimension styles, and `PSTYLEMODE` of AR4-09) is:

| Variable | Expected host type of the read | `TYPED_VALUE` kind |
|---|---|---|
| `CLAYER` | `System.String` | `STRING` |
| `CECOLOR` | `System.String` | `STRING` |
| `CELTYPE` | `System.String` | `STRING` |
| `CELTSCALE` | `System.Double` | `DOUBLE` |
| `CELWEIGHT` | `System.Int16` | `INT` |
| `CETRANSPARENCY` | `System.String` | `STRING` |
| `CPLOTSTYLE` | `System.String` | `STRING` |
| `TEXTSTYLE` | `System.String` | `STRING` |
| `DIMSTYLE` | `System.String` | `STRING` |
| `PSTYLEMODE` | `System.Int16` | `INT` |

This table is an **expectation** that the I-12 qualification confirms on the exact build **before the candidate** of this artifact (header PREREQUISITES; section 5 step 5); a different observed type is corrected here before the seal (section 2.6). A variable that a kind baseline lists and this table does not is typed the same way, and the qualification records its host type.

### 2.2 Ordering (normative)

| Container | Order |
|---|---|
| ordered entity containers (the entity sequence of a block table record) | **the host iteration order** as reported (V2.1 28.1; V2.2 PA-9). It is **not necessarily the drawing order**: the drawing order of a `SortentsTable` is outside layer 1 (section 2.4; AR2-34) |
| unordered containers (symbol-table name sets, dictionary entries, sibling sets, maps, an `ENUMERATION` with `ordered = 0`) | stored-name or key order, ordinal (UTF-8 byte order); the **encoder sorts**, so the order in which a producer supplies them is irrelevant (vectors EV-15 = EV-16, EV-61 = EV-73; handles sort as strings, EV-86) |
| fields of a record | ordinal order of the field name (section 2.1) |

### 2.3 Record types (schema version 1; field tables)

The field tables below are generated from the schema registry of the reference encoder (`SCHEMAS`), which is the normative source; every cell is identical to the registry (a literal `|` inside a cell is written `\|`; the check is mechanical, section 7). A record whose field set differs from its table, or that violates a rule stated in a table cell or in sections 2.3.1 to 2.3.6, is `REJECTED`. `ENTITY_*` records carry the common entity fields (`dxfName`, `layer`, `linetype`, `linetypeScale`, `lineweight`, `visible`, the colour fields and the transparency fields) **merged with** their concrete fields; the merged set is streamed in ordinal order. The colour and transparency fields are conditional: `colorRgb` is present exactly when `colorMethod = ByColor`, and `transparencyAlpha` exactly when `transparencyMethod = ByAlpha` (vectors RJ-07..RJ-10); the `table` field of a `SYMBOL_RECORD_*` equals its table (RJ-13). The record type of a host object is chosen by its **exact host class** (section 2.3.1); an object of any other class is **`UNKNOWN`** (section 2.5).

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
| `colorRgb` | integer | R*65536 + G*256 + B, present exactly when colorMethod = ByColor (else REJECTED) |
| `colorBookName` | string | color book name, or absent |
| `colorName` | string | color name in the book, or absent |
| `transparencyMethod` | enum (stored name as a string) | stored name of the host TransparencyMethod |
| `transparencyAlpha` | integer | alpha 0-255, present exactly when transparencyMethod = ByAlpha (else REJECTED) |
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
| `colorRgb` | integer | R*65536 + G*256 + B, present exactly when colorMethod = ByColor (else REJECTED) |
| `colorBookName` | string | color book name, or absent |
| `colorName` | string | color name in the book, or absent |
| `transparencyMethod` | enum (stored name as a string) | stored name of the host TransparencyMethod |
| `transparencyAlpha` | integer | alpha 0-255, present exactly when transparencyMethod = ByAlpha (else REJECTED) |
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
| `colorRgb` | integer | R*65536 + G*256 + B, present exactly when colorMethod = ByColor (else REJECTED) |
| `colorBookName` | string | color book name, or absent |
| `colorName` | string | color name in the book, or absent |
| `transparencyMethod` | enum (stored name as a string) | stored name of the host TransparencyMethod |
| `transparencyAlpha` | integer | alpha 0-255, present exactly when transparencyMethod = ByAlpha (else REJECTED) |
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
| `colorRgb` | integer | R*65536 + G*256 + B, present exactly when colorMethod = ByColor (else REJECTED) |
| `colorBookName` | string | color book name, or absent |
| `colorName` | string | color name in the book, or absent |
| `transparencyMethod` | enum (stored name as a string) | stored name of the host TransparencyMethod |
| `transparencyAlpha` | integer | alpha 0-255, present exactly when transparencyMethod = ByAlpha (else REJECTED) |
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
| `colorRgb` | integer | R*65536 + G*256 + B, present exactly when colorMethod = ByColor (else REJECTED) |
| `colorBookName` | string | color book name, or absent |
| `colorName` | string | color name in the book, or absent |
| `transparencyMethod` | enum (stored name as a string) | stored name of the host TransparencyMethod |
| `transparencyAlpha` | integer | alpha 0-255, present exactly when transparencyMethod = ByAlpha (else REJECTED) |
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
| `colorRgb` | integer | R*65536 + G*256 + B, present exactly when colorMethod = ByColor (else REJECTED) |
| `colorBookName` | string | color book name, or absent |
| `colorName` | string | color name in the book, or absent |
| `transparencyMethod` | enum (stored name as a string) | stored name of the host TransparencyMethod |
| `transparencyAlpha` | integer | alpha 0-255, present exactly when transparencyMethod = ByAlpha (else REJECTED) |
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
| `colorRgb` | integer | R*65536 + G*256 + B, present exactly when colorMethod = ByColor (else REJECTED) |
| `colorBookName` | string | color book name, or absent |
| `colorName` | string | color name in the book, or absent |
| `transparencyMethod` | enum (stored name as a string) | stored name of the host TransparencyMethod |
| `transparencyAlpha` | integer | alpha 0-255, present exactly when transparencyMethod = ByAlpha (else REJECTED) |
| `blockName` | string | the definition name; for a dynamic reference, the name of its dynamic block definition (never the anonymous *U name); a reference to an anonymous definition that is not a dynamic-block representation is UNKNOWN (section 2.3.2) |
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
| `colorRgb` | integer | R*65536 + G*256 + B, present exactly when colorMethod = ByColor (else REJECTED) |
| `colorBookName` | string | color book name, or absent |
| `colorName` | string | color name in the book, or absent |
| `transparencyMethod` | enum (stored name as a string) | stored name of the host TransparencyMethod |
| `transparencyAlpha` | integer | alpha 0-255, present exactly when transparencyMethod = ByAlpha (else REJECTED) |
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
| `colorRgb` | integer | R*65536 + G*256 + B, present exactly when colorMethod = ByColor (else REJECTED) |
| `colorBookName` | string | color book name, or absent |
| `colorName` | string | color name in the book, or absent |
| `transparencyMethod` | enum (stored name as a string) | stored name of the host TransparencyMethod |
| `transparencyAlpha` | integer | alpha 0-255, present exactly when transparencyMethod = ByAlpha (else REJECTED) |
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
| `colorRgb` | integer | R*65536 + G*256 + B, present exactly when colorMethod = ByColor (else REJECTED) |
| `colorBookName` | string | color book name, or absent |
| `colorName` | string | color name in the book, or absent |
| `transparencyMethod` | enum (stored name as a string) | stored name of the host TransparencyMethod |
| `transparencyAlpha` | integer | alpha 0-255, present exactly when transparencyMethod = ByAlpha (else REJECTED) |
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
| `overrides` | ordered sequence of `RB` records | the stored per-entity dimension-variable overrides (AR4-10): the typed values of the DSTYLE section of the entity's ACAD extended data, in host order (section 2.3.2); empty when there is no DSTYLE section, never absent; the host-derived *D block is excluded |

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
| `colorRgb` | integer | R*65536 + G*256 + B, present exactly when colorMethod = ByColor (else REJECTED) |
| `colorBookName` | string | color book name, or absent |
| `colorName` | string | color name in the book, or absent |
| `transparencyMethod` | enum (stored name as a string) | stored name of the host TransparencyMethod |
| `transparencyAlpha` | integer | alpha 0-255, present exactly when transparencyMethod = ByAlpha (else REJECTED) |
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
| `overrides` | ordered sequence of `RB` records | as ENTITY_ROTATED_DIMENSION (section 2.3.2) |

**`SYMBOL_RECORD_LAYER`**

| Field | Value kind | Content |
|---|---|---|
| `table` | enum (stored name as a string) | LAYER |
| `name` | string | stored case |
| `colorMethod` | enum (stored name as a string) | stored name of the host ColorMethod |
| `colorIndex` | integer | ACI index as the host reports it |
| `colorRgb` | integer | R*65536 + G*256 + B, present exactly when colorMethod = ByColor (else REJECTED) |
| `colorBookName` | string | color book name, or absent |
| `colorName` | string | color name in the book, or absent |
| `linetype` | string | linetype name |
| `lineweight` | enum (stored name as a string) | - |
| `isOff` | boolean | - |
| `isFrozen` | boolean | - |
| `isLocked` | boolean | - |
| `isPlottable` | boolean | - |
| `transparencyMethod` | enum (stored name as a string) | stored name of the host TransparencyMethod |
| `transparencyAlpha` | integer | alpha 0-255, present exactly when transparencyMethod = ByAlpha (else REJECTED) |

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
| `dimvars` | unordered set of inline `DIMVAR_VALUE` records keyed by `name` | EVERY variable of the closed list of section 2.3.3 with its declared kind, sorted by name; a missing variable (unreadable) is UNKNOWN, an extra variable or another kind is REJECTED |

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
| `data` | ordered sequence of `RB` records | the stored ResultBuffer of the Xrecord: every (code, value) in host order, chunk boundaries and non-text entries preserved; absent when there is no payload |

**`NOD_ENTRY`**

| Field | Value kind | Content |
|---|---|---|
| `path` | string | the dictionary keys from the named-object dictionary root, stored case, joined by "/" |
| `objectClass` | enum (stored name as a string) | derived from the exact host class of the object (section 2.3.1; never supplied): XRECORD for AcDbXrecord, DICTIONARY for AcDbDictionary; any other class (a subclass included) is UNKNOWN |
| `data` | ordered sequence of `RB` records | XRECORD: its ResultBuffer (never absent); DICTIONARY: absent |
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
| `value` | record `TYPED_VALUE` (from the host value, section 2.1.3) | the value returned by the system-variable read by name (section 2.1.3), as a TYPED_VALUE by its host type |

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
| `colorRgb` | integer | R*65536 + G*256 + B, present exactly when colorMethod = ByColor (else REJECTED) |
| `colorBookName` | string | color book name, or absent |
| `colorName` | string | color name in the book, or absent |

**`TYPED_VALUE`**

| Field | Value kind | Content |
|---|---|---|
| `kind` | enum (stored name as a string) | one of ABSENT, BOOL, INT, DOUBLE, STRING, NAME, BYTES, POINT3D, COLOR |
| `value` | by the kind of the enclosing record | encoded by the kind; absent exactly when the kind is ABSENT (else REJECTED) |

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
| `memberKey` | string | category\|recordKind\|expectedName (the set key; "\|" is a literal character); another form is REJECTED |
| `category` | enum (stored name as a string) | READ_SET, CLOSURE or CONTEXT (section 2.3.5) |
| `recordKind` | string | one of the record kinds of section 2.3.5 |
| `expectedName` | string | the name queried with host symbol-table semantics, or the key of section 2.3.5 (REFERENCE: a handle in the item form HANDLE, else REJECTED) |
| `storedName` | string | by the storedName column of section 2.3.5: present exactly when the member is PRESENT and its record kind is named |
| `state` | enum (stored name as a string) | PRESENT, ABSENT or UNKNOWN; any other value is REJECTED |
| `digest` | 64 lowercase hex digits (no prefix) | the exact digest of the member record (record type of section 2.3.5), present exactly when PRESENT |

**`MANIFEST_ADDITION`**

| Field | Value kind | Content |
|---|---|---|
| `memberKey` | string | the memberKey of the corresponding closure member: CLOSURE\|recordKind\|expectedName, its recordKind part equal to recordKind (else REJECTED) |
| `recordKind` | string | as STATE_MEMBER (section 2.3.5) |
| `storedName` | string | the stored name of the added record (never absent) |
| `digest` | 64 lowercase hex digits (no prefix) | exact digest recorded after the import |

**`MANIFEST_REUSED`**

| Field | Value kind | Content |
|---|---|---|
| `memberKey` | string | as MANIFEST_ADDITION |
| `recordKind` | string | as STATE_MEMBER (section 2.3.5) |
| `storedName` | string | the stored name of the pre-existing record (never absent) |
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

**`TEST_TYPED`**

| Field | Value kind | Content |
|---|---|---|
| `value` | record `TYPED_VALUE` | - |

#### 2.3.1 Host class to record type (closed map; BA05V3-N05, R1:BA05V4-07)

The record type of a host object is chosen by **exact equality of its host class name** (the `RXClass` name, for example `AcDbBlockReference`): never by a subclass test (`is`, `IsDerivedFrom`) and never by the DXF name. The `dxfName` field still records the DXF name the host reports.

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
| `AcDbXrecord` | Xrecord | `NOD_ENTRY` with `objectClass = XRECORD` | an object of the named-object dictionary |
| `AcDbDictionary` | DBDictionary | `NOD_ENTRY` with `objectClass = DICTIONARY` | an object of the named-object dictionary |

**Every other class is `UNKNOWN`**, a subclass of a listed class included: for example `AcDbMInsertBlock` (DXF name `INSERT`) and `AcDbTable`, both subclasses of `AcDbBlockReference` (vector EV-65), and `AcDbProxyEntity` (vector EV-51). A subclass that has its own row maps by its own row: `AcDbAttributeDefinition` (a subclass of `AcDbText`) is `ENTITY_ATTDEF`, never `ENTITY_TEXT` (vector EV-66). The classes RackCad itself draws (evidence `sa2`: `BlockReference`, `DBText`, `RotatedDimension`, `Polyline`, `Circle`; AR2-36) are all in the map. **Objects of the named-object dictionary** follow the same rule through their own host-class slot: the producer supplies `[host class, fields]` without `objectClass`, and `objectClass` is **derived** from the host class by the last two rows (`AcDbXrecord` gives `XRECORD`, `AcDbDictionary` gives `DICTIONARY`; vectors EV-59, EV-60); every other class, for example `AcDbDictionaryWithDefault`, is `UNKNOWN` (vector EV-75), and an input that supplies `objectClass` itself is `REJECTED` (vector RJ-24).

#### 2.3.2 Extraction of the map-valued, reference and stored-override fields (BA05V3-N06; R1:BA05V4-05 (b); R2:BA05V4-N09; AR4-10)

**`ENTITY_INSERT.dynamicProperties`.** The members are the entries of the reference's dynamic-property collection whose **read-only flag is false** (the writable properties: the ones a writer can assign, which is what the product assigns, `LateralHeaderDrawer.cs:430-436` at `3375aadb`). Each becomes a `DYNPROP` with the stored property name and the typed value of its host value (section 2.1.3). The producer supplies each entry as `{"name", "readOnly", "value"}`; an entry of another shape is `REJECTED` (vector RJ-23). Read-only entries are not fingerprinted (section 2.4), so two read-only entries with the same name do not matter (vector EV-49). Two **writable** entries with the same name make the record `UNKNOWN` (a duplicate key; vector EV-70): a disclosed availability cost, whose occurrence in the library the census records (section 5). A writable property whose host value has no typed-value kind (for example a `Point2d`) is `UNKNOWN` (vector EV-71). A static reference has an empty set.

**`ENTITY_INSERT.blockName` and `blockDigest` of a reference to an anonymous definition.** A dynamic reference is fingerprinted by the name and digest of its dynamic block definition, never by its anonymous representation. A reference whose definition is an **anonymous record that is not a dynamic-block representation** (a `*U` or other host-generated name, for example inside a library block or a pre-existing homologue) has no fingerprintable definition, because section 2.4 excludes anonymous records as definitions: the reference is **`UNKNOWN`** (vector EV-88; the producer's `blockName` begins with `*`), and so is every definition that holds it (section 2.5). This is a disclosed availability cost whose occurrence the census records (section 5).

**`overrides` of `ENTITY_ROTATED_DIMENSION` and `ENTITY_ALIGNED_DIMENSION` (ruled by AR4-10, section 217).** The source is the **stored override set**, not an effective value compared with the style: the typed values of the entity's extended data of the registered application `ACAD` that lie inside its `DSTYLE` section (after the string `DSTYLE` and the opening control string `{`, up to the matching closing control string `}`), in host order, encoded as the `RB` sequence of section 2.1.2. An override that the host stores is state even when its value equals the style value. An entity without a `DSTYLE` section has an empty sequence (never absent, vector RJ-05); a malformed section, or a value of an unsupported code (a handle, code 1005), is `UNKNOWN` (vector EV-83): **handle-valued overrides stay `UNKNOWN` and are disclosed** as an availability cost (AR4-10). The product sets such overrides when the style is not named (`LateralHeaderDrawer.cs:362-372` at `3375aadb`). **Conditions before the candidate of this artifact (AR4-10):** the census (section 5 steps 2 and 5) and the I-12 qualification confirm on the exact build (a) the storage location (the `DSTYLE` section of the `ACAD` extended data), (b) the host order of the stored pairs, and (c) whether the host stores or drops an override whose value equals the style value; a different finding changes this table before the seal (section 2.6). The kind baseline's oracle predicts the stored set under the confirmed behaviour. The `DSTYLE` section is carved out of the extended-data boundary of section 2.4. Vectors EV-52, EV-53, EV-83; layer 2 compares the set as a map (section 3.1, vector SV-16).

#### 2.3.3 Closed list of dimension variables of `SYMBOL_RECORD_DIMSTYLE`

`DIMADEC` (INT), `DIMALT` (BOOL), `DIMALTD` (INT), `DIMALTF` (DOUBLE), `DIMALTMZF` (DOUBLE), `DIMALTMZS` (STRING), `DIMALTRND` (DOUBLE), `DIMALTTD` (INT), `DIMALTTZ` (INT), `DIMALTU` (INT), `DIMALTZ` (INT), `DIMAPOST` (STRING), `DIMARCSYM` (INT), `DIMASZ` (DOUBLE), `DIMATFIT` (INT), `DIMAUNIT` (INT), `DIMAZIN` (INT), `DIMBLK` (NAME), `DIMBLK1` (NAME), `DIMBLK2` (NAME), `DIMCEN` (DOUBLE), `DIMCLRD` (COLOR), `DIMCLRE` (COLOR), `DIMCLRT` (COLOR), `DIMDEC` (INT), `DIMDLE` (DOUBLE), `DIMDLI` (DOUBLE), `DIMDSEP` (INT), `DIMEXE` (DOUBLE), `DIMEXO` (DOUBLE), `DIMFRAC` (INT), `DIMFXL` (DOUBLE), `DIMFXLON` (BOOL), `DIMGAP` (DOUBLE), `DIMJOGANG` (DOUBLE), `DIMJUST` (INT), `DIMLDRBLK` (NAME), `DIMLFAC` (DOUBLE), `DIMLIM` (BOOL), `DIMLTEX1` (NAME), `DIMLTEX2` (NAME), `DIMLTYPE` (NAME), `DIMLUNIT` (INT), `DIMLWD` (INT), `DIMLWE` (INT), `DIMMZF` (DOUBLE), `DIMMZS` (STRING), `DIMPOST` (STRING), `DIMRND` (DOUBLE), `DIMSAH` (BOOL), `DIMSCALE` (DOUBLE), `DIMSD1` (BOOL), `DIMSD2` (BOOL), `DIMSE1` (BOOL), `DIMSE2` (BOOL), `DIMSOXD` (BOOL), `DIMTAD` (INT), `DIMTDEC` (INT), `DIMTFAC` (DOUBLE), `DIMTFILL` (INT), `DIMTFILLCLR` (COLOR), `DIMTIH` (BOOL), `DIMTIX` (BOOL), `DIMTM` (DOUBLE), `DIMTMOVE` (INT), `DIMTOFL` (BOOL), `DIMTOH` (BOOL), `DIMTOL` (BOOL), `DIMTOLJ` (INT), `DIMTP` (DOUBLE), `DIMTSZ` (DOUBLE), `DIMTVP` (DOUBLE), `DIMTXSTY` (NAME), `DIMTXT` (DOUBLE), `DIMTXTDIRECTION` (BOOL), `DIMTZIN` (INT), `DIMUPT` (BOOL), `DIMZIN` (INT). The record holds **every** variable of the list with its declared kind. A variable the host cannot read is omitted by the producer, and the record is then `UNKNOWN` (no digest; vector EV-89); a variable outside the list, or a value of another kind, is `REJECTED` (vectors RJ-11, RJ-12). `NAME` values are the stored names of the referenced records (blocks, linetypes, text styles).

#### 2.3.4 Record-level `ABSENT` and `UNKNOWN` members

In a `PRE_COMMAND_STATE_RECORD` a member whose record does not exist has `state = ABSENT` and no digest and no stored name (vector EV-62); a member whose record cannot be read has `state = UNKNOWN`, no digest and no stored name, and then **the whole record is `UNKNOWN`** and PREPARE-R refuses the run (O1, V2.1 6; vector EV-63). Any other `state` value, a digest on a member that is not `PRESENT`, or a missing digest on a `PRESENT` member is `REJECTED` (vectors RJ-14, RJ-15). A field value that is absent is `~` (vector EV-12). The contract's "raw stored bytes" of the payload (V2.1 28.1) is realized as the lossless `RB` sequence of section 2.1.2, because the host stores an `Xrecord` as a typed `ResultBuffer`, not as bytes (BA05-F04; accepted as the Architect's reading, AR3-21).

#### 2.3.5 Categories and record kinds of `STATE_MEMBER` (BA05V3-N07; AR4-08, AR4-09, AR4-11)

| Category | Meaning |
|---|---|
| `READ_SET` | a component of the abort read-set (V2.1 11.1) as the kind baseline specifies it |
| `CLOSURE` | a member of the static dependency closure: the pre-existing homologue or `ABSENT` (V2.1 11.3) |
| `CONTEXT` | a plan-time input the kind baseline captures in the record for its oracle (for Selective: the consumed context variables and the presentation of the source top-level reference, AR3-11). **Ruled by AR4-08 (section 217):** like every member of the record, a `CONTEXT` member (`CONTEXT_VARIABLE` and `REFERENCE`) is part of `EXPECTED_AFTER_ABORT` and ABORT-VERIFY compares it exactly (V2.1 11, 11.2; 11.3 "Consequence"); the contract fixes this, and it is not a choice of the kind baseline. Only the **domain of E-08** (which members are re-observed at PS/PV) belongs to the kind baseline; for Selective, AR4-08 rules that E-08 compares the `REFERENCE` **and** `CONTEXT_VARIABLE` members at PS/PV, so a change of context between the capture and PV aborts as class A |

| `recordKind` | Record type of `digest` | `expectedName` | `storedName` |
|---|---|---|---|
| `BLOCK` | `DEFINITION_DUMP` | the block name | PRESENT: the stored name of the pre-existing block table record; else absent |
| `LAYER` | `SYMBOL_RECORD_LAYER` | the layer name | PRESENT: the stored name of the pre-existing layer; else absent |
| `LTYPE` | `SYMBOL_RECORD_LTYPE` | the linetype name | PRESENT: the stored name of the pre-existing linetype; else absent |
| `STYLE` | `SYMBOL_RECORD_STYLE` | the text style name | PRESENT: the stored name of the pre-existing text style; else absent |
| `DIMSTYLE` | `SYMBOL_RECORD_DIMSTYLE` | the dimension style name | PRESENT: the stored name of the pre-existing dimension style; else absent |
| `APPID` | `SYMBOL_RECORD_APPID` | the registered application name | PRESENT: the stored name of the pre-existing registered application; else absent |
| `PAYLOAD` | `PAYLOAD_EXACT` | the name of the definition that owns the payload | PRESENT: the stored name of that definition (the field owner of its PAYLOAD_EXACT); else absent |
| `NOD_ENTRY` | `NOD_ENTRY` | the NOD path | PRESENT: the stored path (the field path of its NOD_ENTRY: the stored keys joined by "/"); else absent |
| `ENUMERATION` | `ENUMERATION` | the container role name | always absent (a role name, not the name of a stored record) |
| `CONTEXT_VARIABLE` | `CONTEXT_VARIABLE` | the variable name | always absent (a variable is not a stored record) |
| `REFERENCE` | `ENTITY_INSERT` | the handle of the reference in the item form HANDLE (session-local; AR4-11) | always absent (an entity has no stored name) |

The record kinds `BLOCK`, `LAYER`, `LTYPE`, `STYLE`, `DIMSTYLE`, `APPID`, `PAYLOAD` and `NOD_ENTRY` are **named**; for them `storedName` is present exactly when the member is `PRESENT`. For `ENUMERATION`, `CONTEXT_VARIABLE` and `REFERENCE` it is always absent. Another use of `storedName` is `REJECTED` (vector RJ-16). Vector EV-62 carries members of the named kinds `BLOCK`, `PAYLOAD`, `NOD_ENTRY` and `LAYER` and of the unnamed kinds `ENUMERATION`, `CONTEXT_VARIABLE` and `REFERENCE`.

`memberKey` = `category|recordKind|expectedName` (a literal `|`); another form is `REJECTED` (vector RJ-03). The `expectedName` of a `REFERENCE` member is the handle of the reference in the item form `HANDLE` (section 2.3.6); another value is `REJECTED` (vector RJ-17). **Bindings and addresses** (V2.1 11.1: "per-view `RackViewAddress` and grouping") have **no record kind of their own**: the persisted address (`Kind`, `View`, `Section`) and the grouping identity (`Id`) are members of the envelope that the product writes on each view definition (`RackEmbedDocument.cs:38-50` and `RackDefinitionCreator.cs:197-204` at `3375aadb`), so they are inside the exact `RB` sequence of that definition's `PAYLOAD` member, and any change to them changes its digest. The decoded address is a function of those bytes (the total codec `RackViewCodec`) and is not a separate layer-1 record. **The class of a reference** in a reference set is carried by the `ENUMERATION` item form `HANDLE_CLASS` (section 2.3.6; vector EV-74).

**Session scope of handle keys (ruled by AR4-11, section 217; disclosed in section 7).** A `REFERENCE` member is keyed by the handle of the reference (its `expectedName`, hence its `memberKey`). This is the Architect's reading of V2.1 28.1 with the scope of the `ENUMERATION` exception: the key is valid **within one session** only. A `PRE_COMMAND_STATE_RECORD`, and a `SCOPED_RECORD_COMPARISON` (section 4.1), that holds a handle-keyed member (a `REFERENCE` member, or an `ENUMERATION` in a handle item form) is **session-bound**: it is compared only with observations of the same session, and it is **excluded from the cross-session determinism control of V2.1 28.3**, as a handle `ENUMERATION` already is. The `ENTITY_INSERT` record of the member holds no handle, so its own digest is not session-bound.

**The `REFERENCE` member and the plot style (ruled by AR4-09, section 217).** A `REFERENCE` member is the exact `ENTITY_INSERT` record of one reference (vector EV-82): it carries the layer, colour, linetype, linetype scale, lineweight, transparency and visibility of the reference, but **not its plot style name**. FPSPEC-V1 adds no plot-style field: the gap is **declared**. Its conditions (AR4-09), carried by the kind baseline: (1) the kind baseline binds `PSTYLEMODE = 1` in the pinned fixture bytes and verifies it in the fixture conformance record, or captures `PSTYLEMODE` as a `CONTEXT_VARIABLE` member (expected host type in section 2.1.3); (2) a governing run on a drawing with named plot styles makes SL-R `UNKNOWN` (O5), never `EQUAL`; (3) the gap is disclosed among the Owner Act 2 coverage limits (this artifact lists it in the coverage boundary of section 2.4). Covering drawings with named plot styles needs a table change before the seal (section 2.6), or `FPSPEC-V2` and a new baseline after it.

**Member values for the oracle (R2:BA05V4-N05).** A member carries only its digest, and the digest of the record is what V2.1 6 logs. A consumer that needs the **values** of a member (for Selective, the kind baseline's oracle reads the ST-13 set of each source reference and the context keys, AR3-11 conditions 2 and 3) reads them from the member's **canonical stream**: PREPARE-R retains, with the record in the PR checkpoint evidence, the exact stream (section 2.1) of every `CONTEXT` member and of every other member whose values the kind baseline's oracle consumes. A consumer uses a retained stream only if its SHA-256 equals the member's `digest` in the record, and decodes it with the field tables of section 2.3 (the stream is unambiguous for a known record type); a missing or non-matching stream makes that input `UNKNOWN`. The record, its digest and the comparison rules do not change.

#### 2.3.6 `ENUMERATION` item forms (BA05V3-N07, N08)

| `itemForm` | Item | Pattern |
|---|---|---|
| `NAME` | the stored name | - |
| `HANDLE` | the handle in lowercase hexadecimal without leading zeros | `^[1-9a-f][0-9a-f]{0,15}$` |
| `HANDLE_CLASS` | the handle as in HANDLE, a colon, and the exact host class name (RXClass name) of the object | `^[1-9a-f][0-9a-f]{0,15}:[A-Za-z0-9_]+$` |

With `ordered = 0` the **encoder** sorts the items in ordinal (UTF-8) order and a duplicate item is `UNKNOWN`; with `ordered = 1` the items are in host iteration order. Handles therefore sort **as strings, not as numbers**: `1f0` sorts before `a` although 0x1f0 > 0xa (vector EV-86, whose supplied numeric order the encoder does not keep). An item not in its form is `REJECTED` (vector RJ-06). Handle forms are valid only within one session and are excluded from cross-session determinism (V2.1 28.1; section 2.3.5 for the records that hold them). Vectors EV-61, EV-73, EV-74, EV-85, EV-86.

### 2.4 Exclusions and coverage boundary (BA05-F11)

**Excluded:** `ObjectId`; database-local handles, except inside an `ENUMERATION` within one session (V2.1 28.1) and as the key of a `REFERENCE` member within one session (the Architect's reading of V2.1 28.1, AR4-11; section 2.3.5, where the records that hold either are declared session-bound); timestamps; volatile identifiers; reactors and host-local pointers; anonymous block table records (`*U`, `*D`, any host-generated anonymous name) as definitions (a reference to one that is not a dynamic-block representation is `UNKNOWN`, section 2.3.2).

**Not fingerprinted (the coverage boundary of "equal over the fingerprinted fields"):** extension-dictionary entries of a definition other than the RackCad payload (including the `ACAD_SORTENTS` drawing-order table and dynamic-block data); extended data and extension dictionaries of entities, **except** the `DSTYLE` section of the `ACAD` extended data of a dimension (field `overrides`, section 2.3.2); the read-only dynamic properties of a reference (section 2.3.2); entity properties not listed in the tables (for example material and shadow properties, hyperlinks, and the **plot style name**: a declared gap ruled by AR4-09, with its conditions in section 2.3.5, disclosed among the Owner Act 2 coverage limits); the metadata of an `Xrecord` beyond its data (merge style, ownership flags); symbol-record properties not listed (for example layer descriptions and viewport overrides). A change confined to these is not detected by layer 1.

### 2.5 Unknown and unsupported (rule for classes outside the tables)

A proxy entity, a custom entity, an object whose exact class is not in section 2.3.1, a reference to an anonymous definition that is not a dynamic-block representation, a non-finite number, an ill-formed string, an unsupported `RB` code or host type, a host value without a typed-value kind, a duplicate set member or key, a dimension-style variable the host cannot read, or an unreadable record yields **`UNKNOWN`**, never a partial digest (unless a rule of section 2.3 is also violated: then `REJECTED`, section 2.1). Consequences:

- a member of the **static dependency closure** with such content makes the closure `UNKNOWN` and **refuses the run (O1)** (V2.1 6);
- a **pre-existing homologue** of the user's drawing with such content makes the record `UNKNOWN` and refuses the run (O1): an **availability cost** disclosed in Owner Act 2;
- **`UNKNOWN` propagates through `blockDigest` (AR3-18).** A definition that references, at any depth, a definition that is `UNKNOWN` is itself `UNKNOWN` (vectors EV-67, EV-68). So a RackCad view definition, or a pre-existing homologue, that references a library block holding a class without a table, or a reference to an anonymous non-dynamic record (EV-88), is `UNKNOWN`, and the run is refused (O1). Because the digest of a RackCad definition chains through the digests of the library blocks it references, the **library census (section 5) is a precondition of every such digest**;
- **two sources of the class list (AR2-36):** the **library census** (section 5) and the **static derivation of the classes RackCad draws** in every kind whose definitions can be in the read-set (evidence `sa2`: `BlockReference`, `DBText`, `RotatedDimension`, `Polyline` and `Circle`; Selective, Dynamic and Push Back draw the first three, Cantilever and RACKSECCION the last two). Every product-drawn class is in section 2.3.1, so a RackCad-owned definition of any kind is not refused **for its product-drawn content**; it is still `UNKNOWN` when a library block it references is (previous bullet);
- a payload whose chunking split a surrogate pair (the product chunks at 255 UTF-16 units) is `UNKNOWN` (vector EV-31): a disclosed availability cost. The same holds for two writable dynamic properties with the same name (EV-70), for a handle value in a `DSTYLE` section (EV-83, AR4-10) and for a reference to an anonymous non-dynamic record (EV-88).

### 2.6 Versioning (BA05-F02, BA05V3-N09)

| When | Version effect |
|---|---|
| **before the seal**: a type table, a class-map row, a code range, a rule of section 2.3 or a vector is added, changed or regenerated (for example after the census or the qualification confirmations) | a new `ARTIFACT_VERSION` of BA-05; `FINGERPRINT_SPEC_VERSION` stays **`FPSPEC-V1`** and the domain separator stays; vectors **may be added or regenerated** (a changed table changes its own vectors and every vector that nests them through `blockDigest` or member digests). V4 regenerated EV-52, EV-53 and EV-60 to EV-64. **This version** regenerates EV-62 and EV-64 (a consistent drawing state and the `storedName` rule), changes the inputs but not the results of EV-59, EV-60, EV-63, EV-75, EV-76, EV-77, EV-78 and of RJ-01, RJ-02 (the host-class slot of NOD objects and the `storedName` rule), and adds EV-84 to EV-89, RJ-07 to RJ-25 and SV-14 to SV-18 (section 7) |
| **at the seal** | the vectors are **frozen** with the sealed bytes |
| **after the seal** | any change is a new `FINGERPRINT_SPEC_VERSION` (`FPSPEC-V2`, new domain separator) and a **new baseline** |

### 2.7 NORMATIVE exact vectors

The JSON file is **authoritative for the exact bytes** through the field **`streamHex`** of each vector; `streamDisplay` is informative only (backslash doubled, LF shown as `\n`) (BA05-F07). Every vector carries its producer-side **`input`**. The input conventions (JSON field `encoding.inputConventions`) are: a record as a JSON object; a double as `{"binary64": <16 hex>}`; an integer host value (`Int16`, `Int32` or `Int64`) as a JSON integer; a host point as `{"point3d": [...]}` or `{"point2d": [...]}`; an ill-formed string as its UTF-16 code units; an `RB` entry as `[code, value]`; a typed value as `[kind, value]`; a nested reference as `[name, digest or "UNKNOWN"]`; an entity of a definition or an attribute as `[host class, fields]`; **a NOD object as `[host class, fields without objectClass]`** (section 2.3.1); a dynamic property as `{"name", "readOnly", "value"}`; a dimension-style variable the host could not read is omitted. A conformance harness feeds the same input to I-12. These vectors become normative when this artifact is sealed. 62 digests and 19 `UNKNOWN` results, 81 vectors in all, plus 25 `REJECTED` inputs; at least one digest per production record type (the encoder asserts it).

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
| EV-51 | DEFINITION_DUMP holding an entity of a class outside the map (a proxy, AcDbProxyEntity): UNKNOWN | `DEFINITION_DUMP` | **UNKNOWN** (entity of host class AcDbProxyEntity has no record type (exact class, section 2.3.1)) |
| EV-52 | ENTITY_ROTATED_DIMENSION; overrides = the DSTYLE pairs as stored, in host order (illustrative pairs) | `ENTITY_ROTATED_DIMENSION` | `b528f4898adcf10366355b29d88597c0928f04a4408688aed802f9da9ffc2eeb` |
| EV-53 | ENTITY_ALIGNED_DIMENSION | `ENTITY_ALIGNED_DIMENSION` | `e426f3c586f8351f6d8f3b8fd752fdee72569d69ef218f23657c23e65b882da0` |
| EV-54 | SYMBOL_RECORD_LAYER (RACKCAD_ANOTACIONES; the read-set layer of EV-62) | `SYMBOL_RECORD_LAYER` | `087023825ee388b5969f85f88be6b95da37e8fb00b7f0c6ba368dc89b91b2856` |
| EV-55 | SYMBOL_RECORD_LTYPE | `SYMBOL_RECORD_LTYPE` | `98c57e7db8a5192a35674b352c4d67946f23dc5064efebc8945f9cfb141f19ad` |
| EV-56 | SYMBOL_RECORD_STYLE | `SYMBOL_RECORD_STYLE` | `39e47384546fd9e832a1e322df84642b79dfdf851d529ad1a9e4ba87d04228b1` |
| EV-57 | SYMBOL_RECORD_DIMSTYLE with every variable of the closed list (illustrative values) | `SYMBOL_RECORD_DIMSTYLE` | `1423f9ee723868053442d1c0defd694cbabbfbfb9789ccb3e0161c7bddf4b848` |
| EV-58 | SYMBOL_RECORD_APPID | `SYMBOL_RECORD_APPID` | `cabe53b3432232e3fe0cf106002246d9c03602fe583e95b9cb401b7d39e3d1e2` |
| EV-59 | NOD_ENTRY, an XRECORD (RACKCAD_PROJECT, as the product writes it: text chunks of code 1); input [host class AcDbXrecord, fields] | `NOD_ENTRY` | `8c245e3263eb6231cde7faef67c3d131e85cba9e6c1d8cf2727bb9b1654f9710` |
| EV-60 | NOD_ENTRY, a DICTIONARY: the host dictionary ACAD_GROUP with two synthetic group keys (children sorted); input [host class AcDbDictionary, fields] | `NOD_ENTRY` | `22d585022d3b93308ca0184f169d9843cc014b676dfb40da795ad7d8d6003b5d` |
| EV-61 | ENUMERATION, unordered names given sorted | `ENUMERATION` | `5ec9eabd51f232b1e176b59ef5313b153724d67d548efcf81b93a97a1cbea28e` |
| EV-62 | PRE_COMMAND_STATE_RECORD (synthetic names; a consistent subset of a record): the read-set definition, payload, block-table enumeration and NOD entry (EV-50, EV-19, EV-85, EV-59); the annotation layer the source text uses (EV-54) and an ABSENT dimension layer; the closure block the source references (EV-48, REUSE_AS_IS), an ABSENT closure block only the mirrored plan requires, the pre-existing layer 0 (EV-79); two CONTEXT members (EV-80, EV-82) | `PRE_COMMAND_STATE_RECORD` | `48e7eb0b9ab18538a33654270dee7994e2c7712e390195cb3fdfef77e4a7d1ac` |
| EV-63 | PRE_COMMAND_STATE_RECORD with an UNKNOWN member: UNKNOWN | `PRE_COMMAND_STATE_RECORD` | **UNKNOWN** (a member of the PRE_COMMAND_STATE_RECORD is UNKNOWN: no partial digest (refusal O1, V2.1 6)) |
| EV-64 | PREPARE_RESIDUE_MANIFEST (synthetic names): the import adds the ABSENT closure block of EV-62 (PLACA_BASE_3X3, EV-84; ADD) and binds its entity to the pre-existing layer 0 (EV-79; REUSED_BOUND); POSTE_3X3, PRESENT in EV-62, is REUSE_AS_IS and not in the manifest | `PREPARE_RESIDUE_MANIFEST` | `285d7eb48ce446e85a01aa6ea23004293555cb6dc6f33b6877c663b08d98db5d` |
| EV-65 | DEFINITION_DUMP holding an AcDbMInsertBlock (DXF name INSERT, a subclass of AcDbBlockReference): UNKNOWN by the exact-class map | `DEFINITION_DUMP` | **UNKNOWN** (entity of host class AcDbMInsertBlock has no record type (exact class, section 2.3.1)) |
| EV-66 | DEFINITION_DUMP holding an AcDbAttributeDefinition (a subclass of AcDbText): ENTITY_ATTDEF by the exact-class map, never ENTITY_TEXT | `DEFINITION_DUMP` | `3e9dfbc44f4a803f2ef2efa4354a343b7c295793e84834bd42f2ef21ec81ea7c` |
| EV-67 | ENTITY_INSERT whose referenced definition is UNKNOWN: UNKNOWN (propagation through blockDigest, AR3-18) | `ENTITY_INSERT` | **UNKNOWN** (the referenced definition POSTE_PROXY is UNKNOWN: UNKNOWN propagates through the nested digest (AR3-18)) |
| EV-68 | DEFINITION_DUMP holding the reference of EV-67: UNKNOWN (propagates to every definition that references it, AR3-18) | `DEFINITION_DUMP` | **UNKNOWN** (the referenced definition POSTE_PROXY is UNKNOWN: UNKNOWN propagates through the nested digest (AR3-18)) |
| EV-69 | ENTITY_INSERT with writable dynamic properties of host types String, Int16 and Double (synthetic names): STRING, INT, DOUBLE (section 2.1.3) | `ENTITY_INSERT` | `a76fa8dc37a20784147bca0c33b5d6bbde160a76ad722459e5a014ef82b0b1a8` |
| EV-70 | ENTITY_INSERT with two WRITABLE dynamic properties of the same name: UNKNOWN (duplicate key) | `ENTITY_INSERT` | **UNKNOWN** (duplicate key in an unordered set of DYNPROP) |
| EV-71 | ENTITY_INSERT with a writable dynamic property whose host value is a Point2d: UNKNOWN (no typed-value kind) | `ENTITY_INSERT` | **UNKNOWN** (a host value of type Point2d has no typed-value kind (section 2.1.3)) |
| EV-72 | unordered set with a DUPLICATE member: UNKNOWN | `TEST_SET` | **UNKNOWN** (duplicate member in an unordered set) |
| EV-73 | ENUMERATION ordered = 0 given UNSORTED: the encoder sorts; must EQUAL EV-61 | `ENUMERATION` | `5ec9eabd51f232b1e176b59ef5313b153724d67d548efcf81b93a97a1cbea28e` |
| EV-74 | ENUMERATION of a reference set with class (item form HANDLE_CLASS; synthetic handles; sorted as strings) | `ENUMERATION` | `cb27e3ef9bdfeb2f21993f496e4d0f1c761b4c93ad829a4c103ad35cddfeab6b` |
| EV-75 | NOD_ENTRY of host class AcDbDictionaryWithDefault (a subclass of AcDbDictionary, supplied in the host-class slot): UNKNOWN | `NOD_ENTRY` | **UNKNOWN** (NOD object of host class AcDbDictionaryWithDefault: only AcDbXrecord and AcDbDictionary (exact class, section 2.3.1) are supported) |
| EV-76 | NOD_ENTRY XRECORD with a code 5 entry (a handle): UNKNOWN (AR3-21) | `NOD_ENTRY` | **UNKNOWN** (RB code 5 (entity handle) is a volatile identifier) |
| EV-77 | NOD_ENTRY XRECORD with a point-valued code (10): an inline POINT3D (AR3-21) | `NOD_ENTRY` | `520ab963764be5776bc1c1cee97adb2cbbfcec080415c907101e374ff17c5f27` |
| EV-78 | NOD_ENTRY XRECORD whose code 70 entry holds a Double (not the integer of its range): UNKNOWN (fail closed) | `NOD_ENTRY` | **UNKNOWN** (RB code 70 holds a host value of type Double, not Int16, Int32 or Int64) |
| EV-79 | SYMBOL_RECORD_LAYER of layer 0 (illustrative properties; the closure layer of EV-62, REUSED_BOUND in EV-64) | `SYMBOL_RECORD_LAYER` | `e3d72fcfd19b2d3790603002b70b7562ba58009115f76e3a82e258d355565dc1` |
| EV-80 | CONTEXT_VARIABLE CLAYER, host value a String | `CONTEXT_VARIABLE` | `a0e3612b6164eed4804bc1a4b4fdfdb71b3606aecfe1999c77f49aae0baf09ba` |
| EV-81 | CONTEXT_VARIABLE CELTSCALE, host value a Double | `CONTEXT_VARIABLE` | `d7b4f921bb2bd121486f917d386c546902625e5ab7a8a3dbf481f47966d70bbe` |
| EV-82 | ENTITY_INSERT of the source top-level reference (static; its definition is EV-50): the REFERENCE member of EV-62 | `ENTITY_INSERT` | `bd5de186fe639980ea58fc1e92552a40d10d5a15e2d05059fabd81b4d28656b4` |
| EV-83 | ENTITY_ROTATED_DIMENSION whose DSTYLE section holds a handle value (code 1005): UNKNOWN (a disclosed availability cost) | `ENTITY_ROTATED_DIMENSION` | **UNKNOWN** (RB code 1005 (extended-data handle) is a volatile identifier) |
| EV-84 | DEFINITION_DUMP of the library block PLACA_BASE_3X3 (synthetic; one LWPOLYLINE on layer 0): the ABSENT closure block of EV-62 that EV-64 adds | `DEFINITION_DUMP` | `58379e1ffd70e630670c5fa0dc8b0ad5649937e898cf4e801708430be40c01f5` |
| EV-85 | ENUMERATION of the block-table name set of the drawing of EV-62 (synthetic; layout records included): every PRESENT block of EV-62 is in it, the ABSENT one is not | `ENUMERATION` | `77445ff28c9a685fe12ca89bebcdf04c12726de5ce2d209408d8251b34f57ebf` |
| EV-86 | ENUMERATION of handles supplied in numeric order (a = 10, 1f0 = 496; synthetic): the encoder sorts them as strings (1f0 before a), so a numeric sort gives other bytes | `ENUMERATION` | `67ea91ccde17f6c1311455fe61bb2bcc4e5443d22def707ac7d18922c5a0affb` |
| EV-87 | CONTEXT_VARIABLE CELWEIGHT from the system-variable read by name, host value an Int16 (-1, illustrative): INT (section 2.1.3) | `CONTEXT_VARIABLE` | `3196dd65ea413068d57b794e3d29681f60e9a53138f5aa6354767809e6562848` |
| EV-88 | ENTITY_INSERT of an anonymous definition that is not a dynamic-block representation (*U7, synthetic; a digest is supplied): UNKNOWN (section 2.3.2) | `ENTITY_INSERT` | **UNKNOWN** (ENTITY_INSERT: the definition *U7 is anonymous and not a dynamic-block representation: UNKNOWN (section 2.3.2)) |
| EV-89 | SYMBOL_RECORD_DIMSTYLE without DIMTXT (a variable the host could not read): UNKNOWN (section 2.3.3) | `SYMBOL_RECORD_DIMSTYLE` | **UNKNOWN** (SYMBOL_RECORD_DIMSTYLE: variable(s) DIMTXT of the closed list missing (the host could not read them): no digest (section 2.3.3)) |

**Rejected inputs (malformed producer records).**

| Id | Purpose | Record type | Result |
|---|---|---|---|
| RJ-01 | NOD_ENTRY XRECORD with children present | `NOD_ENTRY` | **REJECTED** (NOD_ENTRY XRECORD: data must be present and children absent) |
| RJ-02 | NOD_ENTRY DICTIONARY with data present | `NOD_ENTRY` | **REJECTED** (NOD_ENTRY DICTIONARY: data must be absent and children present) |
| RJ-03 | STATE_MEMBER whose memberKey is not category\|recordKind\|expectedName (written with "/") | `PRE_COMMAND_STATE_RECORD` | **REJECTED** (STATE_MEMBER: memberKey is not category\|recordKind\|expectedName) |
| RJ-04 | record with a missing field (TEST_OPTIONAL without layer) | `TEST_OPTIONAL` | **REJECTED** (TEST_OPTIONAL: fields ['layer', 'name'] expected, got ['name']) |
| RJ-05 | ENTITY_ROTATED_DIMENSION with overrides absent | `ENTITY_ROTATED_DIMENSION` | **REJECTED** (ENTITY_ROTATED_DIMENSION: overrides is never absent (empty when there is no DSTYLE section)) |
| RJ-06 | ENUMERATION HANDLE item with a leading zero | `ENUMERATION` | **REJECTED** (ENUMERATION: item '01f' is not in the form HANDLE) |
| RJ-07 | ENTITY_LINE with colorRgb present and colorMethod ByLayer | `ENTITY_LINE` | **REJECTED** (ENTITY_LINE: colorRgb is present exactly when colorMethod = ByColor) |
| RJ-08 | ENTITY_LINE with colorMethod ByColor and colorRgb absent | `ENTITY_LINE` | **REJECTED** (ENTITY_LINE: colorRgb is present exactly when colorMethod = ByColor) |
| RJ-09 | SYMBOL_RECORD_LAYER with transparencyAlpha present and transparencyMethod ByLayer | `SYMBOL_RECORD_LAYER` | **REJECTED** (SYMBOL_RECORD_LAYER: transparencyAlpha is present exactly when transparencyMethod = ByAlpha) |
| RJ-10 | SYMBOL_RECORD_LAYER with transparencyMethod ByAlpha and transparencyAlpha absent | `SYMBOL_RECORD_LAYER` | **REJECTED** (SYMBOL_RECORD_LAYER: transparencyAlpha is present exactly when transparencyMethod = ByAlpha) |
| RJ-11 | SYMBOL_RECORD_DIMSTYLE with an extra variable DIMFOO | `SYMBOL_RECORD_DIMSTYLE` | **REJECTED** (SYMBOL_RECORD_DIMSTYLE: 'DIMFOO' is not a variable of the closed list (section 2.3.3)) |
| RJ-12 | SYMBOL_RECORD_DIMSTYLE with DIMTXT typed INT (declared DOUBLE) | `SYMBOL_RECORD_DIMSTYLE` | **REJECTED** (SYMBOL_RECORD_DIMSTYLE: DIMTXT must be a TYPED_VALUE of kind DOUBLE (section 2.3.3)) |
| RJ-13 | SYMBOL_RECORD_LAYER with table = LTYPE | `SYMBOL_RECORD_LAYER` | **REJECTED** (SYMBOL_RECORD_LAYER: table must be LAYER) |
| RJ-14 | STATE_MEMBER with state FOO | `PRE_COMMAND_STATE_RECORD` | **REJECTED** (STATE_MEMBER: state 'FOO' is not PRESENT, ABSENT or UNKNOWN) |
| RJ-15 | STATE_MEMBER UNKNOWN that carries a digest (REJECTED takes precedence over the UNKNOWN of the record) | `PRE_COMMAND_STATE_RECORD` | **REJECTED** (STATE_MEMBER: digest is present exactly when the state is PRESENT) |
| RJ-16 | STATE_MEMBER of kind REFERENCE with a storedName (always absent for REFERENCE) | `PRE_COMMAND_STATE_RECORD` | **REJECTED** (STATE_MEMBER: storedName is present exactly when the member is PRESENT and its record kind is named (section 2.3.5)) |
| RJ-17 | STATE_MEMBER of kind REFERENCE whose expectedName is not a handle | `PRE_COMMAND_STATE_RECORD` | **REJECTED** (STATE_MEMBER: the expectedName of a REFERENCE member is a handle in the item form HANDLE (section 2.3.5)) |
| RJ-18 | TYPED_VALUE of kind ABSENT with a value | `TEST_TYPED` | **REJECTED** (TYPED_VALUE: the value is absent exactly when the kind is ABSENT (section 2.1)) |
| RJ-19 | TYPED_VALUE of kind INT without a value | `TEST_TYPED` | **REJECTED** (TYPED_VALUE: the value is absent exactly when the kind is ABSENT (section 2.1)) |
| RJ-20 | MANIFEST_ADDITION whose memberKey is not in the form CLOSURE\|recordKind\|expectedName | `PREPARE_RESIDUE_MANIFEST` | **REJECTED** (MANIFEST_ADDITION: memberKey must be CLOSURE\|<recordKind>\|<expectedName> of the corresponding closure member, with recordKind BLOCK) |
| RJ-21 | MANIFEST_ADDITION whose memberKey names another record kind (LAYER) than its recordKind (BLOCK) | `PREPARE_RESIDUE_MANIFEST` | **REJECTED** (MANIFEST_ADDITION: memberKey must be CLOSURE\|<recordKind>\|<expectedName> of the corresponding closure member, with recordKind BLOCK) |
| RJ-22 | MANIFEST_REUSED whose memberKey is not a closure member key (category READ_SET) | `PREPARE_RESIDUE_MANIFEST` | **REJECTED** (MANIFEST_REUSED: memberKey must be CLOSURE\|<recordKind>\|<expectedName> of the corresponding closure member, with recordKind LAYER) |
| RJ-23 | ENTITY_INSERT with a dynamic property without its readOnly flag | `ENTITY_INSERT` | **REJECTED** (a dynamic property is {"name": string, "readOnly": boolean, "value": host value} (section 2.3.2)) |
| RJ-24 | NOD_ENTRY input that supplies objectClass (derived from the host class) | `NOD_ENTRY` | **REJECTED** (NOD_ENTRY: objectClass is derived from the exact host class (section 2.3.1), never supplied) |
| RJ-25 | PRECEDENCE: ENTITY_LINE with a NaN endX (UNKNOWN, earlier in field order) and an integer in the boolean field visible (REJECTED, later): REJECTED | `ENTITY_LINE` | **REJECTED** (bool expected) |

Relations that the vectors pin: EV-01 != EV-02 (signed zero); EV-01 != EV-03 (1 ulp); EV-08 != EV-09 (case); EV-13 != EV-14 (sequence order is state); EV-15 == EV-16, EV-21 == EV-22 and EV-61 == EV-73 (set order, field order and the encoder's sort of an unordered enumeration); EV-19 != EV-20 (an unknown member changes the payload digest); EV-26 != EV-27 != EV-28 (chunk boundaries and type codes are state); EV-49 keeps its V3 digest although its input carries two read-only entries (they are not fingerprinted); EV-59, EV-60 and EV-77 keep their V4 digests with the host-class input (the derived `objectClass` is the same).

**The example state of EV-62 and EV-64 is consistent (R1:BA05V4-03, R2:BA05V4-N03).** Names in EV-60 to EV-64, EV-69, EV-74, EV-82, EV-84 to EV-86 and EV-88 are synthetic (the value of EV-87 is illustrative); the record types, roles and structures follow the product and V2.1 11.3. EV-62 is a **subset** of a complete record (it omits, for example, the linetype records the layers reference), and its members are mutually consistent: the read-set definition `SEL_FRONTAL_0` (EV-50) holds a reference to `POSTE_3X3` and a text on `RACKCAD_ANOTACIONES`, so both are `PRESENT` (the closure block `POSTE_3X3` with EV-48, a pre-existing required top-level library block, therefore `REUSE_AS_IS` and not in the manifest; the layer with EV-54); the block-table enumeration (EV-85) lists every `PRESENT` block and not the `ABSENT` one; the `ABSENT` members are records that nothing present references (the dimension layer `RACKCAD_COTAS`, since the source holds no dimension, and the closure block `PLACA_BASE_3X3`, which only the mirrored plan requires); the source reference (EV-82) is on layer 0, which is `PRESENT` (EV-79) and is `CLAYER` (EV-80). EV-64 has the import **add** `PLACA_BASE_3X3` (EV-84, `ADD`) and bind its entity to the pre-existing layer 0 (`REUSED_BOUND` with the pre-command digest EV-79). The RackCad NOD entries are `Xrecord`s (EV-59), and the annotation layer is added by the seam, not by the import. The encoder asserts the block-table consistency.

## 3. Layer 2: `SEMANTIC_COMPARISON`

### 3.1 Comparator responsibilities and tolerance slots

The comparator parses the persisted record with the **authoritative reader** into its typed form and compares it with the expected typed form computed by the kind baseline's oracle (`ORACLE_SPEC`). It is independent of the writer and of `mu_k`. **This specification names the slots and states no value.** V2.2 PA-9 gives the tolerances and normalizations to the kind baseline: their values, the rule of `NORM_ANGLE` and their status live **only** there (for Selective, the tolerance section of the **sealed** BA-08; AR2-35, **AR3-17**). No value appears in this document or in its vector file (section 3.3). A comparison whose slot has no sealed value in the kind baseline cannot be declared. **The routing is total (R1:BA05V4-08, R2:BA05V4-N07):** every compared value of a typed form falls in exactly one row below, and section 3.1.2 assigns every double field of every production and helper record to one class (the encoder asserts it); only the eight slots of the kind baseline are used, so no new slot and no new edge arise.

| Compared value | Rule | Slot (value, rule and status: the sealed kind baseline) |
|---|---|---|
| positions, insertion points, origins, vertices, lengths, radii, widths, thicknesses, elevations (fields: section 3.1.2) | predicate `ABS_DIFF_LE(TOL_LENGTH)` | `TOL_LENGTH` |
| text heights (`height`, `textHeight`, `textSize`) | predicate `ABS_DIFF_LE(TOL_TEXT_HEIGHT)` | `TOL_TEXT_HEIGHT` |
| dimension points (`xLine1Point`, `xLine2Point`, `dimLinePoint`, `textPosition`) | predicate `ABS_DIFF_LE(TOL_DIM)` | `TOL_DIM` |
| rotations and other angles (section 3.1.2) | predicate `ANGLE_DIFF_LE(TOL_ANGLE)`, wrapped difference by the rule of `NORM_ANGLE` | `TOL_ANGLE`, `NORM_ANGLE` |
| normals (the three components as one vector) | predicate `NORMAL_ANGLE_LE(TOL_NORMAL)` | `TOL_NORMAL` |
| in-plane direction vectors (`ENTITY_MTEXT.direction`) | predicate `DIRECTION_ANGLE_LE(TOL_ANGLE)` | `TOL_ANGLE` |
| scale factors of a reference (`ENTITY_INSERT.scaleX/Y/Z`) | `ABS_DIFF_LE(TOL_SCALE)`, absolute | `TOL_SCALE` |
| dynamic-property values (`DYNPROP.value`), by the units type the census records for the property (section 5): distance | `ABS_DIFF_LE(TOL_DYNAMIC)` (a `POINT3D` value: per component) | `TOL_DYNAMIC` |
| dynamic-property values of units type angular | `ANGLE_DIFF_LE(TOL_ANGLE)` | `TOL_ANGLE`, `NORM_ANGLE` |
| dynamic-property `DOUBLE` values of units type area or none | `EXACT_DOUBLE` | none |
| dynamic-property values whose units type the census did not record | `UNKNOWN` | none |
| stored dimension overrides (`overrides`, section 2.3.2; ruled by AR4-10) | `OVERRIDE_MAP_EQUAL`: a map keyed by the DIMVAR group code, **host order not significant**, each value by the class of its variable (section 3.1.2) | the slot of that class |
| dimension-style variables (`DIMVAR_VALUE.value`) | by the class of the variable (section 3.1.2) | the slot of that class |
| dimensionless factors (`linetypeScale`, `widthFactor`, `xScale`, `lineSpacingFactor`, `bulge`, `shapeScale`) and **every other double** (for example a double inside an `RB` or `TYPED_VALUE` that no row above routes) | `EXACT_DOUBLE` (binary64 equality, `-0.0` = `+0.0`) | none |
| integers, booleans, enum values (for example `DIMTAD`, `DIMDEC`, flags, `colorIndex`, stored enum names) | `EXACT` | none |
| texts, names | `EXACT_STRING` | none |
| nested-digest fields (`blockDigest`) | not compared at layer 2 (a digest never decides equivalence, V2.1 28): the reference is compared by `blockName`, and its definition by its own typed form | none |
| sibling sets | unordered | none |
| sequences whose order the plan determines | order-sensitive | none |
| authored payload and envelope | typed members, `ExtensionData` and unknown members (section 3.2), `CANONICAL_JSON_EQUAL` | none |
| unrecognized schema version, unsupported class, unparsable record | `UNKNOWN` | none |

### 3.1.1 Predicates (normative)

| Predicate | Definition |
|---|---|
| `ABS_DIFF_LE(TOL)` | \|a - b\| <= TOL, per scalar component (never a Euclidean distance; AR3-20), absolute (never relative); -0.0 equals +0.0; a non-finite value is UNKNOWN |
| `ANGLE_DIFF_LE(TOL_ANGLE)` | EQUAL when the wrapped difference of the two angles, computed by the rule of the slot NORM_ANGLE, is <= TOL_ANGLE; the rule of NORM_ANGLE is written only in the kind baseline (V2.2 PA-9), this specification names the slot |
| `NORMAL_ANGLE_LE(TOL_NORMAL)` | both vectors normalized (a zero-length vector is UNKNOWN); EQUAL when the angle between them <= TOL_NORMAL; the angle is computed stably, for example atan2(\|a x b\|, a . b); a comparator that computes it as acos of the binary64 dot product is not conformant (SV-11; AR4-11) |
| `DIRECTION_ANGLE_LE(TOL_ANGLE)` | as NORMAL_ANGLE_LE, for an in-plane direction vector, with the slot TOL_ANGLE |
| `EXACT_DOUBLE` | binary64 equality of the two values with -0.0 equal to +0.0; a non-finite value is UNKNOWN; the rule for every double that section 3.1.2 routes to no slot |
| `EXACT` | equality of integers, booleans and enum values (enum values by their stored names) |
| `OVERRIDE_MAP_EQUAL` | the stored overrides of both sides (section 2.3.2) read as maps from the DIMVAR group code (the value of the code-1070 entry that precedes each value) to that value; host order is not significant; an unpaired entry, a repeated group code or a code that designates no variable of section 2.3.3 is UNKNOWN; EQUAL when both maps have the same codes and every value is equal under the class of its variable (section 3.1.2) |
| `EXACT_STRING` | ordinal equality of the stored text |
| `CANONICAL_JSON_EQUAL` | both sides parsed from the wire text; objects compared as maps with ordinally sorted keys; arrays by position; strings exact; numbers by the exact JSON number token (AR3-19); whitespace not significant; a member present on either side that the authoritative reader does not expose is UNKNOWN_MEMBER_UNSUPPORTED |
| `SCHEMA_VERSION_RECOGNIZED` | an unrecognized schema version makes the comparison UNKNOWN |
| `TYPE_SUPPORTED` | an object whose exact host class is outside section 2.3.1 makes the comparison UNKNOWN |

### 3.1.2 Field routing (normative, total; R1:BA05V4-08, R2:BA05V4-N07)

Every double field of the production and helper records of section 2.3 has exactly one class (157 fields; `POINT3D` is routed by the value that carries it, as in section 3.1; test-only records are exempt). A triple `…X/Y/Z` that is a normal or a direction is compared as one vector.

| Class | Predicate | Slot | Values | Fields (record types) |
|---|---|---|---|---|
| `LENGTH` | `ABS_DIFF_LE(TOL_LENGTH)` | `TOL_LENGTH` | positions, insertion points, origins, vertices, lengths, radii, widths, thicknesses and elevations | `alignmentX/Y/Z` (ENTITY_ATTDEF, ENTITY_ATTRIB, ENTITY_TEXT); `centerX/Y/Z` (ENTITY_ARC, ENTITY_CIRCLE); `elevation` (ENTITY_LWPOLYLINE); `endWidth` (PLINE_VERTEX); `endX/Y/Z` (ENTITY_LINE); `length` (LTYPE_DASH); `locationX/Y/Z` (ENTITY_MTEXT); `originX/Y/Z` (DEFINITION_DUMP); `patternLength` (SYMBOL_RECORD_LTYPE); `positionX/Y/Z` (ENTITY_ATTDEF, ENTITY_ATTRIB, ENTITY_INSERT, ENTITY_TEXT); `radius` (ENTITY_ARC, ENTITY_CIRCLE); `shapeOffsetX` (LTYPE_DASH); `shapeOffsetY` (LTYPE_DASH); `startWidth` (PLINE_VERTEX); `startX/Y/Z` (ENTITY_LINE); `thickness` (ENTITY_ARC, ENTITY_CIRCLE, ENTITY_LINE, ENTITY_LWPOLYLINE, ENTITY_TEXT); `width` (ENTITY_MTEXT); `x` (PLINE_VERTEX); `y` (PLINE_VERTEX) |
| `TEXT_HEIGHT` | `ABS_DIFF_LE(TOL_TEXT_HEIGHT)` | `TOL_TEXT_HEIGHT` | text heights | `height` (ENTITY_ATTDEF, ENTITY_ATTRIB, ENTITY_TEXT); `textHeight` (ENTITY_MTEXT); `textSize` (SYMBOL_RECORD_STYLE) |
| `DIM_POINT` | `ABS_DIFF_LE(TOL_DIM)` | `TOL_DIM` | dimension points | `dimLinePointX/Y/Z` (ENTITY_ALIGNED_DIMENSION, ENTITY_ROTATED_DIMENSION); `textPositionX/Y/Z` (ENTITY_ALIGNED_DIMENSION, ENTITY_ROTATED_DIMENSION); `xLine1PointX/Y/Z` (ENTITY_ALIGNED_DIMENSION, ENTITY_ROTATED_DIMENSION); `xLine2PointX/Y/Z` (ENTITY_ALIGNED_DIMENSION, ENTITY_ROTATED_DIMENSION) |
| `ANGLE` | `ANGLE_DIFF_LE(TOL_ANGLE)` | `TOL_ANGLE`, `NORM_ANGLE` | rotations and other angles | `endAngle` (ENTITY_ARC); `oblique` (ENTITY_ALIGNED_DIMENSION, ENTITY_ATTDEF, ENTITY_ATTRIB, ENTITY_ROTATED_DIMENSION, ENTITY_TEXT); `obliquingAngle` (SYMBOL_RECORD_STYLE); `rotation` (ENTITY_ATTDEF, ENTITY_ATTRIB, ENTITY_INSERT, ENTITY_MTEXT, ENTITY_ROTATED_DIMENSION, ENTITY_TEXT); `shapeRotation` (LTYPE_DASH); `startAngle` (ENTITY_ARC) |
| `NORMAL` | `NORMAL_ANGLE_LE(TOL_NORMAL)` | `TOL_NORMAL` | normals (the three components as one vector) | `normalX/Y/Z` (ENTITY_ALIGNED_DIMENSION, ENTITY_ARC, ENTITY_ATTDEF, ENTITY_ATTRIB, ENTITY_CIRCLE, ENTITY_INSERT, ENTITY_LINE, ENTITY_LWPOLYLINE, ENTITY_MTEXT, ENTITY_ROTATED_DIMENSION, ENTITY_TEXT) |
| `DIRECTION` | `DIRECTION_ANGLE_LE(TOL_ANGLE)` | `TOL_ANGLE` | in-plane direction vectors (the three components as one vector) | `directionX/Y/Z` (ENTITY_MTEXT) |
| `SCALE` | `ABS_DIFF_LE(TOL_SCALE)` | `TOL_SCALE` | the scale factors of a reference | `scaleX/Y/Z` (ENTITY_INSERT) |
| `EXACT_DOUBLE` | `EXACT_DOUBLE` | none | dimensionless factors and every double that no other row routes | `bulge` (PLINE_VERTEX); `lineSpacingFactor` (ENTITY_MTEXT); `linetypeScale` (ENTITY_ALIGNED_DIMENSION, ENTITY_ARC, ENTITY_ATTDEF, ENTITY_ATTRIB, ENTITY_CIRCLE, ENTITY_INSERT, ENTITY_LINE, ENTITY_LWPOLYLINE, ENTITY_MTEXT, ENTITY_ROTATED_DIMENSION, ENTITY_TEXT); `shapeScale` (LTYPE_DASH); `widthFactor` (ENTITY_ATTDEF, ENTITY_ATTRIB, ENTITY_TEXT); `xScale` (SYMBOL_RECORD_STYLE) |

**Dimension variables by class** (the `DOUBLE` variables of section 2.3.3; every `INT`, `BOOL` and `COLOR` variable is `EXACT`, every `STRING` and `NAME` variable `EXACT_STRING`). `DIMSCALE` is a dimensionless factor compared exactly, **not** a scale factor of `TOL_SCALE`, so a comparison of the stored overrides never waits for `TOL_SCALE`.

| Class | Variables |
|---|---|
| `TEXT_HEIGHT` | `DIMTXT` |
| `LENGTH` | `DIMASZ`, `DIMCEN`, `DIMDLE`, `DIMDLI`, `DIMEXE`, `DIMEXO`, `DIMFXL`, `DIMGAP`, `DIMRND`, `DIMTM`, `DIMTP`, `DIMTSZ` |
| `ANGLE` | `DIMJOGANG` |
| `EXACT_DOUBLE` | `DIMALTF`, `DIMALTMZF`, `DIMALTRND`, `DIMLFAC`, `DIMMZF`, `DIMSCALE`, `DIMTFAC`, `DIMTVP` |

In the stored overrides, the group code that precedes each value designates its variable by the host's DXF group-code assignment of dimension variables, which the census and the I-12 qualification confirm with the storage (section 5 step 5). For the variables the product writes (`LateralHeaderDrawer.cs:362-372` at `3375aadb`) the codes are `DIMSCALE` 40, `DIMASZ` 41, `DIMEXO` 42, `DIMEXE` 44, `DIMTAD` 77, `DIMTXT` 140, `DIMGAP` 147 and `DIMDEC` 271 (to be confirmed by the same step).

### 3.2 `ExtensionData`, authored and unknown members (V2.2 PA-8)

The typed form compared for the authored payload and envelope includes `ExtensionData`, the authored and forward-compatible members, and the unknown members preserved by contract (I-11). Unknown and extension members are compared exactly after canonical serialization: object keys in ordinal order, **numbers by their exact JSON number token** (so `1` and `1.0` differ: vector SV-12). This token reading of PA-8 rule 1 "exact numbers" is recorded as the **Architect's interpretation of PA-8 (AR3-19)**. A writer that **drops or alters** an unknown member **does not pass** (DIFFERENT; O4 through S-2 or S-3). A member that cannot be compared yields **`UNKNOWN_MEMBER_UNSUPPORTED`**, surfaced explicitly, giving `UNKNOWN` (O5 path). An unrecognized schema version is `UNKNOWN`.

### 3.3 NORMATIVE semantic comparison vectors (BA05-F08, BA05V3-N01, N10; R1:BA05V4-06)

Every input is concrete. **Wire** form = the exact persisted JSON text (the comparator parses it with the authoritative reader); **typed** form = a typed value (doubles as the 16-hex binary64 pattern); **host class** = the exact class name of section 2.3.1. SV-06, SV-07, SV-10, SV-11, SV-14 and SV-15 are written in **slot units** and carry no tolerance value (AR3-17): each persisted value is an expression in the slot, and its binary64 instance is generated by the `Q-FPSPEC-VECTORS` harness (section 3.3.1). Every slot-unit predicate has an `EQUAL` and a `DIFFERENT` vector (SV-06 / SV-07, SV-10 / SV-14, SV-15 / SV-11), so neither an always-`EQUAL` nor an always-`DIFFERENT` comparator passes.

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
| SV-11 | `ENTITY_INSERT` `normal` | typed (binary64 vector), slot units | `0000000000000000 / 0000000000000000 / 3ff0000000000000` | `(tan(2 * TOL_NORMAL), 0, 1)` (slot units) | `NORMAL_ANGLE_LE(TOL_NORMAL)` | **DIFFERENT** | the angle between the normalized vectors is 2 * TOL_NORMAL; a zero-length vector is UNKNOWN. Meant to reject a comparator that computes the angle as acos of the binary64 dot product: at slot values small enough that the dot product rounds to 1 such a comparator returns 0, hence EQUAL; a conformant comparator computes the angle stably, for example atan2(\|a x b\|, a . b) (AR4-11) |
| SV-12 | `ENVELOPE` `(unknown member futureKey)` | wire (envelope JSON text) | `{"SchemaVersion":"1.0","Kind":"selective","Id":"g-1","futureKey":1}` | `{"SchemaVersion":"1.0","Kind":"selective","Id":"g-1","futureKey":1.0}` | `CANONICAL_JSON_EQUAL` | **DIFFERENT** | PA-8 rule 1 "exact numbers" read as the exact JSON number token (Architect interpretation, AR3-19): 1 and 1.0 differ |
| SV-13 | `ENTITY_TEXT` `textString` | typed (string) | `Nivel 1` | `NIVEL 1` | `EXACT_STRING` | **DIFFERENT** | texts, names and enum values are compared exactly (stored case) |
| SV-14 | `ENTITY_INSERT` `rotation` | typed (binary64, radians), slot units | `0000000000000000` | `expected + 2*pi - 2 * TOL_ANGLE` (slot units) | `ANGLE_DIFF_LE(TOL_ANGLE)` | **DIFFERENT** | the DIFFERENT counterpart of SV-10: the two angles differ by 2 * TOL_ANGLE across the wrap at 2pi; an always-EQUAL angle comparator fails it |
| SV-15 | `ENTITY_INSERT` `normal` | typed (binary64 vector), slot units | `0000000000000000 / 0000000000000000 / 3ff0000000000000` | `(tan(0.5 * TOL_NORMAL), 0, 1)` (slot units) | `NORMAL_ANGLE_LE(TOL_NORMAL)` | **EQUAL** | the EQUAL counterpart of SV-11: the angle between the normalized vectors is 0.5 * TOL_NORMAL; an always-DIFFERENT normal comparator fails it |
| SV-16 | `ENTITY_ROTATED_DIMENSION` `overrides` | typed (RB sequence as code:value; doubles as binary64) | `1070:140 / 1040:4010000000000000 / 1070:41 / 1040:4006666666666666` | `1070:41 / 1040:4006666666666666 / 1070:140 / 1040:4010000000000000` | `OVERRIDE_MAP_EQUAL` | **EQUAL** | the stored overrides are compared as a map keyed by the DIMVAR group code (140 = DIMTXT, 41 = DIMASZ); host order is not significant; the values are equal, so the result holds for any slot value (the layer-1 digests of the two orders differ, section 2.1.2) |
| SV-17 | `ENTITY_LINE` `linetypeScale` | typed (binary64) | `3ff0000000000000` | `3ff0000000000001` | `EXACT_DOUBLE` | **DIFFERENT** | a double that section 3.1.2 routes to no slot is compared exactly: 1 ulp differs; linetypeScale is not a scale factor of TOL_SCALE |
| SV-18 | `PLINE_VERTEX` `bulge` | typed (binary64) | `0000000000000000` | `8000000000000000` | `EXACT_DOUBLE` | **EQUAL** | EXACT_DOUBLE treats -0.0 and +0.0 as equal (V2.1 28.2); secondary finding BIT_LEVEL_DIFFERENCE_DOMAIN_EQUIVALENT |

#### 3.3.1 Instances of the slot-unit vectors (AR3-17; R2:BA05V4-N01)

SV-06, SV-07, SV-10, SV-11, SV-14 and SV-15 carry no tolerance value. The Q-FPSPEC-VECTORS harness instantiates them from the sealed slot values of the kind baseline, each slot taken as the binary64 nearest to its sealed value: persisted = the binary64 nearest (ties to even) to the exact real value of slotExpression, component by component (pi and tan evaluated exactly, then rounded once). It then evaluates the realized quantity of the vector's predicate: ABS_DIFF_LE: |persisted - expected| computed exactly (rational arithmetic) on the binary64 values; ANGLE_DIFF_LE: the wrapped difference of the binary64 angles under the sealed NORM_ANGLE rule, evaluated as a certified enclosure with exact pi (interval bounds that contain the exact value); NORMAL_ANGLE_LE: the angle between the normalized binary64 vectors, evaluated as a certified enclosure. An EQUAL vector requires the whole enclosure <= the slot and a DIFFERENT vector requires it > the slot; an instance that fails its check, or whose enclosure straddles the slot, is INVALID (reported, never PASS). A NORM_ANGLE rule that does not reduce modulo 2pi therefore makes SV-10 INVALID. The harness reads the slot values from the **sealed** kind baseline (for Selective, the tolerance section of the sealed BA-08), so the instance depends on that seal and never on this document; the base values are chosen so that the vectors discriminate: 10 for lengths (an absolute reading and a relative one give different results in SV-07), 0 and the wrap at 2pi for rotations, `+Z` for normals. SV-11 is meant to reject a comparator that computes the angle as `acos` of the binary64 dot product, which returns `EQUAL` whenever that dot product rounds to 1 (AR4-11). `TOL_SCALE` is not used by any vector and is not a gate of this artifact (AR3-17). The results of SV-05, SV-16, SV-17 and SV-18 hold for any slot value.

### 3.4 Use of the layers

| Use | Layer |
|---|---|
| S-1, S-2, S-3, the comparator of SL-6 | `SEMANTIC_COMPARISON` |
| E-08 (S-4), S-5, `PRE_COMMAND_STATE_RECORD`, `PREPARE_RESIDUE_MANIFEST`, `REUSED_BOUND`, `LIBRARY_EQUIVALENCE_OBSERVATION` | `EXACT_STATE_FINGERPRINT` |

## 4. Other artifacts (BA05-F10)

`PRE_COMMAND_STATE_RECORD` and `PREPARE_RESIDUE_MANIFEST` are **record types of this specification** (section 2.3); their digests are the ones V2.1 6 and 7 log, and the canonical streams of the members whose values an oracle consumes are retained with the record (section 2.3.5). A `PREPARE_RESIDUE_MANIFEST` entry is keyed by the `memberKey` of its closure member (`CLOSURE|recordKind|expectedName`; vectors RJ-20..RJ-22). The **loaded-code manifest** (E-10, E-11M, E-13), the custody log and the scenario catalog are not fingerprints: this specification does not define their serialization or hashing (the manifest and custody artifact defines its own canonical serialization, AR3-23; the BA-11 registry defines the artifact hashes).

### 4.1 No full-database fingerprint: `SCOPED_RECORD_COMPARISON` (BA05V3-N12)

This specification defines **no fingerprint of a whole database**: layer 1 is a set of per-record digests with a declared coverage boundary (section 2.4), and any object of a class outside section 2.3.1 is `UNKNOWN` (section 2.5). A before-and-after comparison of database state (for example a dynamic-trace source that looks for net effects) is a **`SCOPED_RECORD_COMPARISON`**:

1. **Scope.** A list, written before the first observation, of the members it covers, each a `(recordKind, key)` of section 2.3.5, plus the `ENUMERATION` record of every container it covers (so that an added or removed record shows as a change of that enumeration).
2. **Observation.** At each observation every member is read as a `STATE_MEMBER` (`PRESENT` with its layer-1 digest, `ABSENT`, or `UNKNOWN`), as in a `PRE_COMMAND_STATE_RECORD`, except that an `UNKNOWN` member does not hide the others: it is reported as `UNKNOWN`.
3. **Result per member.** `EQUAL`, `DIFFERENT`, `ADDED` (`ABSENT` then `PRESENT`), `REMOVED` (`PRESENT` then `ABSENT`) or `UNKNOWN` (either side `UNKNOWN`).
4. **Coverage limit (declared).** Only the declared scope and only the fingerprinted fields are observed: a change outside the scope, or confined to what section 2.4 does not fingerprint, is **not seen**; a member of a class outside section 2.3.1 is `UNKNOWN`. An artifact that uses this comparison names its scope (for example the read-set records of the kind baseline) and repeats this limit.
5. **Session scope (AR4-11).** A scope that holds a handle-keyed member (a `REFERENCE` member or a handle `ENUMERATION`) makes the comparison **session-bound**: both observations must come from the same session, and the comparison is outside the cross-session determinism control of V2.1 28.3 (section 2.3.5).

## 5. Library entity-type census (procedure and schema; NOT performed)

The census is **required before the candidate of this artifact**: its classes are added to this draft as in section 2.6, which changes the bytes (a PREREQUISITE in the BA-11 registry entry of this artifact). It reads the library drawing, which requires host work (**not authorized**).

**Universe (AR3-17).** The census covers **every block table record of the library file** (named, anonymous and layout records alike): a superset of any plan's requirement set, defined without reference to any kind baseline. A kind baseline maps its own requirement universe onto the census (the edge runs from the kind baseline to this artifact, never the reverse).

**Procedure (for a future host-authorized gate).** On the private copy of the exact library file of the baseline (a tuple field), read-only:

1. enumerate every block table record of the library file;
2. for each record, record: the exact host class (`RXClass` name) and the DXF name of every entity, with counts; the nested block names; the symbol records referenced; whether any proxy or custom object is present; for every block reference, whether its definition is anonymous and not a dynamic-block representation (such a reference is `UNKNOWN`, section 2.3.2); for every dimension entity, whether its `ACAD` extended data has a `DSTYLE` section and the `(code, host value type)` pairs in it **in host order** (section 2.3.2); for every dynamic block definition, the dynamic properties a reference to it exposes, read through a reference created in a separate scratch database of the census (never in the library copy): name, read-only flag, host value type, **units type** (distance, angular, area or none; the layer-2 class of section 3.1), the value set the host reports for the property (its allowed values, or none), and whether two properties share a name;
3. compute the class universe;
4. **add to this draft** (new `ARTIFACT_VERSION`, `FINGERPRINT_SPEC_VERSION` unchanged) a class-map row, a table and at least one vector for every class outside section 2.3.1 that occurs in a **named, non-layout** block table record. A class seen only in anonymous or layout records is recorded (`anonymousOrLayoutOnly`) and disclosed, and gets no table: such a record is never fingerprinted as a definition (section 2.4), cannot be a closure member (V2.1 11.3), and a reference to an anonymous non-dynamic record is `UNKNOWN` whatever it holds (section 2.3.2);
5. confirm, or correct before the candidate, the host facts this draft leaves to the census and to the I-12 qualification on the exact build: (a) **AR4-10:** the storage location of per-entity dimension overrides (the `DSTYLE` section), the host order of its pairs, and whether the host stores or drops an override whose value equals the style value (the census reads the library dimensions; the qualification writes each override the product writes, once with a value different from the style and once equal to it, and reads the stored section back), together with the group-code designation of section 3.1.2; (b) the host value types of the `RB` ranges (section 2.1.2); (c) the host type of each context variable read by name (section 2.1.3).

**Schema of the census record.**

| Field | Content |
|---|---|
| `libraryFileSha256`, `libraryPath`, `censusUtc`, `instrumentBuild`, `dynamicPropertyMethod` | identity of the census and how the dynamic properties were read |
| `blocks[]` | `blockName`, `isLayout`, `isAnonymous`, `isDynamic`, `entityClasses[]` (`rxClassName`, `dxfName`, `count`), `nestedBlocks[]`, `referencesAnonymousStatic` (boolean), `symbolRecords` (`layers[]`, `linetypes[]`, `textStyles[]`, `dimStyles[]`), `dimensionOverrides[]` (`hasDstyleSection`, `pairs[]` of `code` and `hostValueType`, in host order), `dynamicProperties[]` (`name`, `readOnly`, `hostValueType`, `unitsType`, `allowedValues[]` or absent, `sharesNameWithAnother`), `containsProxyOrCustom` |
| `classUniverse[]` | the distinct `rxClassName` values, each with its `dxfName` values |
| `classesWithoutTable[]` | the classes outside section 2.3.1 at the time of the census, each marked `named` (it gets a table, step 4) or `anonymousOrLayoutOnly` (recorded and disclosed) |

## 6. What keeps NB-3 open

| Id | Item | Gate |
|---|---|---|
| L-1 | the library census (section 5) and the tables of its classes | host (not authorized); **before the candidate** |
| L-2 | **retired in V4.** `TOL_SCALE` is not a gate of BA-05 (AR3-17): it gates the kind baseline and the layer-2 comparisons that need it, and its value and status live only there | - |
| L-3 | candidate ruling, ratifications and seal of this artifact (document + vectors + reference encoder) | baseline review, after the Architect agrees the sealing workflow of the BA-11 registry (AR3-25, AR4-22) |
| L-4 | the confirmations of section 5 step 5 by the census and the I-12 qualification on the exact build (AR4-10 for the stored overrides; the `RB` range types; the context-variable types) | host (not authorized); **before the candidate** |

```text
NB-3 = ADVANCES_TO_BASELINE_ARTIFACT_PREPARATION ; OPEN (library census; qualification confirmations)
FPSPEC-V1 vectors (V5 files) = 81 exact (62 digests, 19 UNKNOWN) + 25 REJECTED inputs + 18 semantic (6 in slot units; normative when sealed)
CT21D_AUTHORITY_BASELINE_READY = FALSE
```

## 7. Delta V5 (decisions section 217)

Section numbering of V4 is kept; the new subsection is 3.1.2, and this section replaces the V4 delta table (which stays in the V4 file, blob 5d6e4d0de5e4801af796ba5969b70f3457fb21be). **Mechanical checks of this version** (scripts over the actual bytes, Python 3.13): the reference encoder regenerates the JSON byte for byte (two runs, equal SHA-256); every cell of the 43 field tables of section 2.3 equals the registry after unescaping `\|` (the check parses every row and compares name, value-kind text and content); every V4 exact result is reproduced except the digests of EV-62 and EV-64, which changed on purpose; the inputs of EV-59, EV-60, EV-63, EV-75, EV-76, EV-77, EV-78 and RJ-01, RJ-02 changed with the same results; every V4 `UNKNOWN` stays `UNKNOWN` and every V4 `REJECTED` stays `REJECTED`; the encoder asserts that the layer-2 routing covers every double field (157) and every `DOUBLE` dimension variable (22), and that the block table of EV-85 is consistent with EV-62; no decimal number with an exponent occurs in this document or in the JSON (the check searches both for the pattern), so no tolerance value is written; the SHA-256 values of the header are computed over the LF-normalized bytes.

**Architect's readings recorded in this artifact (disclosure required by AR4-11).** **AR4-11** (section 217): the handle key of a `REFERENCE` member (its `expectedName` and `memberKey`, section 2.3.5) is accepted as the Architect's reading of V2.1 28.1, with the scope of the `ENUMERATION` exception: valid within one session and outside the cross-session determinism control of V2.1 28.3; every record and `SCOPED_RECORD_COMPARISON` that holds such a member is session-bound (sections 2.3.5, 2.4, 4.1). AR4-11 also records that SV-11 is meant to reject a comparator that computes the angle as `acos` of the binary64 dot product; a conformant comparator computes it stably, for example `atan2(|a x b|, a . b)` (sections 3.1.1, 3.3.1). The earlier readings stay where they are cited: AR2-34 (2.2), AR3-19 (3.2), AR3-20 (3.1.1), AR3-21 (2.1.2, 2.3.4).

**Choices of this version inside BA-05's own tables (before the seal, section 2.6), for the Architect's exact delta check.** (1) `REJECTED` takes precedence over `UNKNOWN` (2.1, RJ-25), so the result of a record that is both malformed and not fingerprintable is determinate. (2) A dimension-style variable the host cannot read is **omitted** and gives `UNKNOWN`, while an extra variable or another kind gives `REJECTED` (2.3.3): this follows R1:BA05V4-06 and the rule "a variable the host cannot read is `UNKNOWN`" that R2:BA05V4-N02 cites; R2's literal fix listed the missing variable under `REJECTED`, and both readings deny a digest. (3) A manifest entry's `memberKey` must be the key of a `CLOSURE` member (V2.1 11.3: the ADD set and the `REUSED_BOUND` records are closure members; RJ-22). (4) Linetype scale, width factors, bulge and `DIMSCALE` are compared exactly, not under `TOL_SCALE` (3.1, 3.1.2).

| Finding id | Decision | Fix (section / field) |
|---|---|---|
| R1:BA05V3-N11 (closure remnant: impossible EV-62/EV-64 state) | AR4-20 | FIXED by the fix of R1:BA05V4-03: the section 2.7 claim that the structures follow the product is now true (2.7, EV-62, EV-64, EV-84, EV-85) |
| R1:BA05V4-01 (handle keys of `REFERENCE` members) | AR4-11 | FIXED: the reading is recorded with its decision id (2.3.5, 2.4, this section); records and scoped comparisons that hold handle-keyed members are declared session-bound and excluded from the cross-session determinism control of V2.1 28.3 (2.3.5, 4.1 item 5) |
| R1:BA05V4-02 (`CONTEXT` row hands a contract choice to the kind baseline) | AR4-08 | FIXED: the row says that every member, `CONTEXT` included, is part of `EXPECTED_AFTER_ABORT` and compared exactly by ABORT-VERIFY (V2.1 11, 11.2, 11.3), and that only the E-08 domain is the kind baseline's (for Selective, AR4-08: `REFERENCE` and `CONTEXT_VARIABLE` at PS/PV) (2.3.5) |
| R1:BA05V4-03 (EV-62/EV-64 impossible state) | AR4-20 | FIXED (first option of the required fix): the referenced block and layer are `PRESENT` (`POSTE_3X3` with EV-48, `REUSE_AS_IS`; `RACKCAD_ANOTACIONES` with EV-54); the `ABSENT` members are unreferenced (`RACKCAD_COTAS`, `PLACA_BASE_3X3`); EV-64 adds `PLACA_BASE_3X3` (new EV-84) and binds layer 0; vectors regenerated under 2.6 (2.7; EV-62, EV-64, EV-84, EV-85) |
| R1:BA05V4-04 (`storedName` undefined for five kinds) | AR4-20 | FIXED: `storedName` column for every record kind, named kinds versus always-absent kinds (2.3.5; `RECORD_KINDS`); enforced (RJ-16); EV-62 now holds an `ENUMERATION` member (EV-85) and a `NOD_ENTRY` member (EV-59) |
| R1:BA05V4-05 (a) (context-variable read), (b) (reference to an anonymous definition) | AR4-20 | FIXED: (a) the read is the system-variable read by name, never a typed Database property; expected host type per key, confirmed by the I-12 qualification before the candidate (2.1.3, 5 step 5, header); Int16 vector EV-87 (CELWEIGHT). (b) a reference to an anonymous definition that is not a dynamic-block representation is `UNKNOWN` (2.3.2, 2.4, 2.5; EV-88) |
| R1:BA05V4-06 (documented rules not enforced or pinned) | AR4-20 | FIXED: the encoder enforces every listed rule and a vector pins each: DIMSTYLE missing variable `UNKNOWN` (EV-89), extra variable or wrong kind `REJECTED` (RJ-11, RJ-12); `colorRgb` / `transparencyAlpha` conditional presence (RJ-07..RJ-10); `state` closed list and conditional fields (RJ-14, RJ-15); `MANIFEST_*` key form and consistency with `recordKind` (RJ-20..RJ-22); `REFERENCE` handle form (RJ-17); malformed dynamic property `REJECTED` instead of a crash (RJ-23); handle enumeration whose string and numeric orders differ (EV-86); `SV-14` (the `DIFFERENT` counterpart of SV-10) and `SV-15` (the `EQUAL` counterpart of SV-11) |
| R1:BA05V4-07 (NOD input model) | AR4-20 | FIXED: NOD objects have a host-class input slot `[host class, fields]` and `objectClass` is derived by the closed map (2.3.1, 2.7, `NOD_CLASS_MAP`, `inputConventions`); EV-59, EV-60, EV-75..EV-78, RJ-01, RJ-02 re-expressed with the same results; a supplied `objectClass` is `REJECTED` (RJ-24) |
| R1:BA05V4-08 (layer-2 routing not total) | AR4-20 | FIXED: total routing with a default rule (`EXACT_DOUBLE` for every double not routed to a slot) and a field-to-class table generated from the registry, asserted total by the encoder; `linetypeScale` and `widthFactor` are exact, the reference scale factors use `TOL_SCALE` (3.1, 3.1.1, 3.1.2; SV-17, SV-18) |
| R2:BA05V4-N01 (instance rule of the slot-unit vectors) | AR4-20, AR4-11 | FIXED on the BA-05 side: the realized quantity is defined per predicate (exact rational difference; wrapped difference under the sealed `NORM_ANGLE` rule by a certified enclosure with exact pi; the angle between the normalized vectors by a certified enclosure), and an enclosure that straddles the slot is `INVALID`; the same text is the JSON `semanticSlotInstanceRule` (3.3.1). The `Q-FPSPEC-VECTORS` text lives in the scenario catalog (BA-04 V5), which must carry the same rule (cross-artifact) |
| R2:BA05V4-N02 (closed-set and conditional rules not enforced) | AR4-20 | FIXED: as R1:BA05V4-06, plus `table` equal to the record type's table (RJ-13) and `TYPED_VALUE` absent exactly for `ABSENT` (RJ-18, RJ-19); the missing dimension-style variable gives `UNKNOWN` (choice (2) above); the `overrides` value-kind cell no longer says "or absent" (2.3 tables) |
| R2:BA05V4-N03 (EV-62/EV-64 impossible state) | AR4-20 | FIXED: as R1:BA05V4-03 |
| R2:BA05V4-N04 (`CONTEXT` allocation) | AR4-08 | FIXED: as R1:BA05V4-02 |
| R2:BA05V4-N05 (member values cannot reach the oracle) | AR4-20 (with AR4-08) | FIXED: the canonical stream of every `CONTEXT` member, and of every other member the oracle consumes, is retained with the record and bound by its member digest; the oracle reads values only from a matching stream, else `UNKNOWN` (2.3.5, 4); no field table changes |
| R2:BA05V4-N06 (context-variable read path) | AR4-20 | FIXED: as R1:BA05V4-05 (a) (2.1.3; EV-87) |
| R2:BA05V4-N07 (overrides, integers, booleans and direction vectors unrouted) | AR4-20, AR4-10 | FIXED: the stored override set is compared as a map keyed by the DIMVAR group code, host order not significant, each value by the class of its variable, all 22 `DOUBLE` variables classed (`DIMSCALE` exact); integers and booleans `EXACT`; direction vectors `DIRECTION_ANGLE_LE(TOL_ANGLE)` (3.1, 3.1.1, 3.1.2; SV-16) |
| R2:BA05V4-N08 (handle-keyed members without a decision) | AR4-11 | FIXED: as R1:BA05V4-01 |
| R2:BA05V4-N09 (anonymous references; census tables for classes never fingerprinted) | AR4-20 | FIXED: a reference to an anonymous non-dynamic definition is `UNKNOWN` (EV-88); census step 4 restricted to named non-layout records, classes found only in anonymous or layout records recorded and disclosed (`anonymousOrLayoutOnly`) (2.3.2, 5) |
| R2:BA05V4-N10 (normative references to draft versions) | AR4-17, AR4-20 | FIXED: section 3.1 cites the sealed kind baseline (the tolerance section of the sealed BA-08), as 3.3.1 does; the BA-11 citations name the registry entry, not a version (header, 5, 6); no normative reference to a draft version number remains |
| AR4-08 (applied) | AR4-08 | the `CONTEXT` row (2.3.5): ABORT-VERIFY compares every record member exactly by contract; the E-08 domain is the kind baseline's |
| AR4-09 (applied) | AR4-09 | the plot style name is a declared gap with its three conditions and the Owner Act 2 disclosure (2.3.5, 2.4); `PSTYLEMODE` has an expected host type (2.1.3) |
| AR4-10 (applied) | AR4-10 | `overrides` = the stored override set; the confirmations of storage location, host order and equal-to-style behaviour before the candidate; handle-valued overrides `UNKNOWN` and disclosed (2.3.2, 5 step 5, 6 L-4, header) |
| AR4-11 (applied) | AR4-11 | the handle-key reading disclosed here with its id; the SV-11 note on a stable angle computation (2.3.5, 2.4, 3.1.1, 3.3.1, 4.1, this section) |
| AR4-17 (applied) | AR4-17 | the BA-11 registry is cited by its entry, not by a version (header, 4, 5, 6) |
| AR4-20 (applied) | AR4-20 | every confirmed minor of both passes is applied (rows above) |
