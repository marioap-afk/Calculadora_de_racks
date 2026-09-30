# I-52 — CT-21D Baseline Artifact BA-05: Fingerprint Specification FPSPEC-V1 (DRAFT V3)

> **BASELINE ARTIFACT BA-05 V3 — DRAFT, NOT SEALED. Design / documentation only. Nothing here is executed or authorized.**
>
> ```text
> ARTIFACT_ID               = CT21D-BASE-FINGERPRINT-SPEC
> FINGERPRINT_SPEC_VERSION  = FPSPEC-V1 (a tuple field; unchanged by this draft version, see section 2.6)
> ARTIFACT_VERSION          = 3-DRAFT   (supersedes 2-DRAFT, blob 22ff7ea08de0a8245d1da19ef4f71a3e7273ca65, which stays as history with its vector files)
> ARTIFACT FILES            = this document + docs/automation/evidence/I-52-ct21d-fpspec-v1-vectors-v3.gen.py (reference encoder and schema registry,
>                             SHA-256 b55e85a7e64b58dacd42f3c9cc776776ffb80acd4e283e4d169133dfa93a305c) + docs/automation/evidence/I-52-ct21d-fpspec-v1-vectors-v3.json (NORMATIVE vectors, SHA-256 42b34fe7f44cff8995164d2e4a7cceb3875c714d642df0925ba8eed4a7860d99)
> STATUS AUTHORITY          = the BA-11 registry entry of this artifact is the only authority for its status and hashes;
>                             this header is frozen, non-authoritative text of the candidate bytes (BA-11 V3 section 2)
> AUTHORITY_CONTRACT        = CT-21D V2.1 28 + V2.2 PA-8, PA-9 + V2.3 + V2.4
> PHASE-2 RULING APPLIED    = BA05-F01 (AR2-32: ordinal field order), F02, F03, F04, F05 (AR2-36), F06 (AR2-33), F07, F08, F09 (AR2-35), F10, F11 (AR2-34)
> BLOCKER                   = NB-3 (ADVANCES_TO_BASELINE_ARTIFACT_PREPARATION; OPEN: library census requires host; TOL_SCALE has no authority)
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

`stream = "FPSPEC-V1|EXACT|" + <record type> + "|" + fields`, and for each field **in ordinal (UTF-8 byte) order of the field name** (V2.1 28.1 "property keys inside a record are in sorted ordinal order"; AR2-32): `<name>` `=` `<value>` LF. The order in which a producer supplies the fields, and the order in which section 2.3 lists them, are irrelevant (vectors EV-21 = EV-22). The digest is SHA-256 of the stream, lowercase hexadecimal. The reference encoder is `I-52-ct21d-fpspec-v1-vectors-v3.gen.py`; an implementation of instrument I-12 is conformant **only** if it reproduces every vector of section 2.7 byte for byte.

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
| unordered set of strings | the elements sorted by **ordinal order = Unicode scalar value order = UTF-8 byte order** (never UTF-16 code-unit order and never culture order; AR2-33; vectors EV-23, EV-24), then encoded as an ordered sequence; a duplicate member is `UNKNOWN` |
| unordered set of records | sorted by the declared key field of the record (UTF-8 byte order), then encoded as an ordered sequence; a duplicate key is `UNKNOWN` |
| nested record inline | `{` + record type + LF + its fields (ordinal order) + `}` |
| nested definition by reference | `@` + the name encoded as a string + `#` + its 64-hex digest |
| typed value (`TYPED_VALUE`) | an inline record with `kind` (one of `ABSENT`, `BOOL`, `INT`, `DOUBLE`, `STRING`, `NAME`, `BYTES`, `POINT3D`, `COLOR`) and `value` encoded by that kind; used for maps (pairs) and polymorphic values |

#### 2.1.1 Maps and pairs

A map (dynamic properties, dimension variables, overrides) is an **unordered set of records** keyed by the name, each record carrying `name` and a `TYPED_VALUE`. There is no other pair encoding.

#### 2.1.2 ResultBuffer typed values (`RB`)

A stored `ResultBuffer` (the data of an `Xrecord`) is encoded as an ordered sequence of `RB` records `{code, value}` in host order; **chunk boundaries and non-text entries are preserved** (vectors EV-26 != EV-27, EV-27 != EV-28). The value kind comes from the DXF group-code range: 0-9: string; 10-59: double; 60-79: int; 90-99: int; 100-100: string; 102-102: string; 110-149: double; 160-169: int; 170-179: int; 210-239: double; 270-289: int; 290-299: bool; 300-309: string; 310-319: bytes; 370-389: int; 400-409: int; 410-419: string; 420-429: int; 430-439: string; 440-459: int; 460-469: double; 470-479: string; 999-1003: string; 1004-1004: bytes; 1005-1009: string; 1010-1059: double; 1060-1071: int. **Unsupported, therefore `UNKNOWN`:** handle and object-id codes (5, 105, 320-369, 390-399, 480-481, 1005 as handle) and every code outside the table.

### 2.2 Ordering (normative)

| Container | Order |
|---|---|
| ordered entity containers (the entity sequence of a block table record) | **the host iteration order** as reported (V2.1 28.1; V2.2 PA-9). It is **not necessarily the drawing order**: the drawing order of a `SortentsTable` is outside layer 1 (section 2.4; AR2-34) |
| unordered containers (symbol-table name sets, dictionary entries, sibling sets, maps) | stored-name or key order, ordinal (UTF-8 byte order) |
| fields of a record | ordinal order of the field name (section 2.1) |

### 2.3 Record types (schema version 1; field tables)

The field tables below are generated from the schema registry of the reference encoder (`SCHEMAS`), which is the normative source; a record whose field set differs from its table is rejected by the encoder. `ENTITY_*` records carry the common entity fields (`dxfName`, `layer`, `linetype`, `linetypeScale`, `lineweight`, `visible`, the colour fields and the transparency fields) **merged with** their concrete fields; the merged set is streamed in ordinal order. An entity of a type without a table is **`UNKNOWN`** (section 2.5).

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
| `entities` | ordered sequence of inline `ENTITY_*` records | every entity of the record in HOST ITERATION ORDER, each an inline ENTITY_* record (section 2.2) |

**`ENTITY_LINE`**

| Field | Value kind | Content |
|---|---|---|
| `dxfName` | string | DXF class name as the host reports it |
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
| `dxfName` | string | DXF class name as the host reports it |
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
| `dxfName` | string | DXF class name as the host reports it |
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
| `dxfName` | string | DXF class name as the host reports it |
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
| `dxfName` | string | DXF class name as the host reports it |
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
| `dxfName` | string | DXF class name as the host reports it |
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
| `dxfName` | string | DXF class name as the host reports it |
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
| `blockDigest` | nested definition by reference | reference to the DEFINITION_DUMP digest of that definition (nested-digest field) |
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
| `dynamicProperties` | unordered set of inline `DYNPROP` records keyed by `name` | dynamic property values, sorted by name; empty for a static reference |
| `attributes` | ordered sequence of inline `ENTITY_ATTRIB` records | attribute references in host iteration order |

**`ENTITY_ATTRIB`**

| Field | Value kind | Content |
|---|---|---|
| `dxfName` | string | DXF class name as the host reports it |
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
| `dxfName` | string | DXF class name as the host reports it |
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
| `dxfName` | string | DXF class name as the host reports it |
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
| `overrides` | unordered set of inline `DIMVAR_VALUE` records keyed by `name` | per-entity dimension-variable overrides, sorted by name; the host-derived *D block is excluded |

**`ENTITY_ALIGNED_DIMENSION`**

| Field | Value kind | Content |
|---|---|---|
| `dxfName` | string | DXF class name as the host reports it |
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
| `overrides` | unordered set of inline `DIMVAR_VALUE` records keyed by `name` | as ENTITY_ROTATED_DIMENSION |

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
| `objectClass` | enum (stored name as a string) | XRECORD or DICTIONARY (any other class is UNKNOWN) |
| `data` | ordered sequence of `RB` records, or absent | XRECORD: its ResultBuffer; DICTIONARY: absent |
| `children` | unordered set of strings | DICTIONARY: the child keys (each child is its own NOD_ENTRY); XRECORD: absent |

**`ENUMERATION`**

| Field | Value kind | Content |
|---|---|---|
| `container` | string | role name of the enumerated container |
| `ordered` | boolean | - |
| `items` | ordered sequence of strings | names, or handles as lowercase hexadecimal without leading zeros; when ordered = 0 the producer sorts the items in ordinal (UTF-8) order first; handle lists are valid only within one session |

**`PRE_COMMAND_STATE_RECORD`**

| Field | Value kind | Content |
|---|---|---|
| `kind` | string | the rack kind (selective) |
| `members` | unordered set of inline `STATE_MEMBER` records keyed by `memberKey` | every read-set component and every closure member (V2.1 6, 11.3) |

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
| `value` | record `TYPED_VALUE` | typed value |

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
| `memberKey` | string | category/recordKind/expectedName (the set key) |
| `category` | enum (stored name as a string) | READ_SET or CLOSURE (V2.1 6, 11.3) |
| `recordKind` | string | BLOCK, LAYER, LTYPE, STYLE, DIMSTYLE, APPID, PAYLOAD, NOD_ENTRY, ENUMERATION or BINDING |
| `expectedName` | string | the name queried with host symbol-table semantics |
| `storedName` | string | the stored case of the pre-existing record, absent when ABSENT |
| `state` | enum (stored name as a string) | PRESENT, ABSENT or UNKNOWN |
| `digest` | 64 lowercase hex digits (no prefix) | the exact digest of the member record when PRESENT, else absent |

**`MANIFEST_ADDITION`**

| Field | Value kind | Content |
|---|---|---|
| `memberKey` | string | as STATE_MEMBER |
| `recordKind` | string | - |
| `storedName` | string | - |
| `digest` | 64 lowercase hex digits (no prefix) | exact digest recorded after the import |

**`MANIFEST_REUSED`**

| Field | Value kind | Content |
|---|---|---|
| `memberKey` | string | as STATE_MEMBER |
| `recordKind` | string | - |
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


#### 2.3.3 Closed list of dimension variables of `SYMBOL_RECORD_DIMSTYLE` and of the dimension overrides

`DIMADEC` (INT), `DIMALT` (BOOL), `DIMALTD` (INT), `DIMALTF` (DOUBLE), `DIMALTMZF` (DOUBLE), `DIMALTMZS` (STRING), `DIMALTRND` (DOUBLE), `DIMALTTD` (INT), `DIMALTTZ` (INT), `DIMALTU` (INT), `DIMALTZ` (INT), `DIMAPOST` (STRING), `DIMARCSYM` (INT), `DIMASZ` (DOUBLE), `DIMATFIT` (INT), `DIMAUNIT` (INT), `DIMAZIN` (INT), `DIMBLK` (NAME), `DIMBLK1` (NAME), `DIMBLK2` (NAME), `DIMCEN` (DOUBLE), `DIMCLRD` (COLOR), `DIMCLRE` (COLOR), `DIMCLRT` (COLOR), `DIMDEC` (INT), `DIMDLE` (DOUBLE), `DIMDLI` (DOUBLE), `DIMDSEP` (INT), `DIMEXE` (DOUBLE), `DIMEXO` (DOUBLE), `DIMFRAC` (INT), `DIMFXL` (DOUBLE), `DIMFXLON` (BOOL), `DIMGAP` (DOUBLE), `DIMJOGANG` (DOUBLE), `DIMJUST` (INT), `DIMLDRBLK` (NAME), `DIMLFAC` (DOUBLE), `DIMLIM` (BOOL), `DIMLTEX1` (NAME), `DIMLTEX2` (NAME), `DIMLTYPE` (NAME), `DIMLUNIT` (INT), `DIMLWD` (INT), `DIMLWE` (INT), `DIMMZF` (DOUBLE), `DIMMZS` (STRING), `DIMPOST` (STRING), `DIMRND` (DOUBLE), `DIMSAH` (BOOL), `DIMSCALE` (DOUBLE), `DIMSD1` (BOOL), `DIMSD2` (BOOL), `DIMSE1` (BOOL), `DIMSE2` (BOOL), `DIMSOXD` (BOOL), `DIMTAD` (INT), `DIMTDEC` (INT), `DIMTFAC` (DOUBLE), `DIMTFILL` (INT), `DIMTFILLCLR` (COLOR), `DIMTIH` (BOOL), `DIMTIX` (BOOL), `DIMTM` (DOUBLE), `DIMTMOVE` (INT), `DIMTOFL` (BOOL), `DIMTOH` (BOOL), `DIMTOL` (BOOL), `DIMTOLJ` (INT), `DIMTP` (DOUBLE), `DIMTSZ` (DOUBLE), `DIMTVP` (DOUBLE), `DIMTXSTY` (NAME), `DIMTXT` (DOUBLE), `DIMTXTDIRECTION` (BOOL), `DIMTZIN` (INT), `DIMUPT` (BOOL), `DIMZIN` (INT). A variable the host cannot read is `UNKNOWN` (the record has no digest). `NAME` values are the stored names of the referenced records (blocks, linetypes, text styles).

#### 2.3.4 Record-level `ABSENT` and `UNKNOWN` members

In a `PRE_COMMAND_STATE_RECORD` a member whose record does not exist has `state = ABSENT` and no digest (vector EV-62); a member whose record cannot be read has `state = UNKNOWN`, and then **the whole record is `UNKNOWN`** and PREPARE-R refuses the run (O1, V2.1 6; vector EV-63). A field value that is absent is `~` (vector EV-12). The contract's "raw stored bytes" of the payload (V2.1 28.1) is realized as the lossless `RB` sequence of section 2.1.2, because the host stores an `Xrecord` as a typed `ResultBuffer`, not as bytes (BA05-F04).

### 2.4 Exclusions and coverage boundary (BA05-F11)

**Excluded:** `ObjectId`; database-local handles (except inside `ENUMERATION` within one session); timestamps; volatile identifiers; reactors and host-local pointers; anonymous block table records (`*U`, `*D`, any host-generated anonymous name) as definitions.

**Not fingerprinted (the coverage boundary of "equal over the fingerprinted fields"):** extension-dictionary entries of a definition other than the RackCad payload (including the `ACAD_SORTENTS` drawing-order table and dynamic-block data); XData and extension dictionaries of entities; entity properties not listed in the tables (for example the plot style name, material and shadow properties, hyperlinks); the metadata of an `Xrecord` beyond its data (merge style, ownership flags); symbol-record properties not listed (for example layer descriptions and viewport overrides). A change confined to these is not detected by layer 1.

### 2.5 Unknown and unsupported (rule for types outside the tables)

A proxy entity, a custom entity, a type whose table is not in section 2.3, a non-finite number, an ill-formed string, an unsupported `RB` code or an unreadable record yields **`UNKNOWN`**, never a partial digest. Consequences:

- a member of the **static dependency closure** with such a type makes the closure `UNKNOWN` and **refuses the run (O1)** (V2.1 6);
- a **pre-existing homologue** of the user's drawing with such a type makes the record `UNKNOWN` and refuses the run (O1): an **availability cost** disclosed in Owner Act 2;
- **two sources of the type list (AR2-36):** the **library census** (section 5) and the **static derivation of the types RackCad draws** in every kind whose definitions can be in the read-set (evidence `sa2`: `BlockReference`, `DBText`, `RotatedDimension`, `Polyline` and `Circle`; Selective, Dynamic and Push Back draw the first three, Cantilever and RACKSECCION the last two). Every product-drawn type has a table in section 2.3, so a RackCad-owned definition of any kind is not refused for its product-drawn content;
- a payload whose chunking split a surrogate pair (the product chunks at 255 UTF-16 units) is `UNKNOWN` (vector EV-31): a disclosed availability cost.

### 2.6 Versioning (BA05-F02)

| When a type table is added or changed | Version effect |
|---|---|
| **before the seal** (for example the census types): added to this **draft** | a new `ARTIFACT_VERSION` of BA-05; `FINGERPRINT_SPEC_VERSION` stays **`FPSPEC-V1`**; the domain separator and every existing vector stay unchanged; new vectors are added |
| **after the seal** | a new `FINGERPRINT_SPEC_VERSION` (`FPSPEC-V2`, new domain separator) and a **new baseline** |

### 2.7 NORMATIVE exact vectors

The JSON file is **authoritative for the exact bytes** through the field **`streamHex`** of each vector; `streamDisplay` is informative only (backslash doubled, LF shown as `\n`) (BA05-F07). These vectors become normative when this artifact is sealed. 49 digests and 7 `UNKNOWN` results, 56 vectors in all; at least one per production record type (the encoder asserts it).

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
| EV-30 | PAYLOAD_EXACT with a handle-valued entry (code 1005): UNKNOWN | `PAYLOAD_EXACT` | **UNKNOWN** (RB code 1005 (handle) is a volatile identifier) |
| EV-31 | PAYLOAD_EXACT whose chunking split a surrogate pair (RackBlockData chunks at 255 UTF-16 units): UNKNOWN | `PAYLOAD_EXACT` | **UNKNOWN** (string is not well-formed UTF-16 (lone surrogate)) |
| EV-40 | ENTITY_LINE | `ENTITY_LINE` | `21a7b93317b70859a49c4eaf66f83a0cd061239918bbcd952e1e239091b6a5b3` |
| EV-41 | ENTITY_ARC | `ENTITY_ARC` | `5c737004ef762b8c0ffee395ae0d32881437b7577fed69b0af731233765087f2` |
| EV-42 | ENTITY_CIRCLE | `ENTITY_CIRCLE` | `ab35af53476d555e1feb4ef8e3cbe508bcdd21bf5d12f564c1be8a3ebb355021` |
| EV-43 | ENTITY_LWPOLYLINE | `ENTITY_LWPOLYLINE` | `1b0c147265dc5ec5b102fe76378abac46d88010da8037a9bc503ebbb6d98451c` |
| EV-44 | ENTITY_TEXT | `ENTITY_TEXT` | `66bdad8149e2fe7e46ee4d1ad1eec4d7e4ae5d3d6415aa084bdee58ecc2a734b` |
| EV-45 | ENTITY_MTEXT | `ENTITY_MTEXT` | `583d7e4b35b3b8d31f23818ae98be18e2cd3c7956b27723aae0aa9a4f76bc851` |
| EV-46 | ENTITY_ATTRIB | `ENTITY_ATTRIB` | `244c80e7951987f8b967184d4de62966eebbdf7a8c79f76a1588c395c366c188` |
| EV-47 | ENTITY_ATTDEF | `ENTITY_ATTDEF` | `1f41aa134ca5eb198bb3b74e26ac1b8dedbcd6cacba04e4b996e5851fcdfe258` |
| EV-48 | DEFINITION_DUMP (one LINE) | `DEFINITION_DUMP` | `b31c3550049136622fbccf010d5c90d51a981df57a4cac48b2757606dc67b9f4` |
| EV-49 | ENTITY_INSERT with nested-digest field and dynamic properties (sorted by name: FRENTE before LONGITUD) | `ENTITY_INSERT` | `fc0f8fdef5c610e9cf88e68d5d5ec6362bb0f5000df9975c7aee12ce7c276211` |
| EV-50 | DEFINITION_DUMP holding an INSERT (nested definition by reference) and a TEXT, in host iteration order | `DEFINITION_DUMP` | `f51d99b36401200e585b2e421ea6a3629b3de59e0e8879dc200882bf1f607d0c` |
| EV-51 | DEFINITION_DUMP holding an entity type outside the specification (a proxy): UNKNOWN | `DEFINITION_DUMP` | **UNKNOWN** (entity type ENTITY_PROXY is not in the specification) |
| EV-52 | ENTITY_ROTATED_DIMENSION (overrides sorted by name) | `ENTITY_ROTATED_DIMENSION` | `4f7b89d8fa7370a9284c683877c4a751d3d4d7a7a4c2f0037e8487f86cc7c5d2` |
| EV-53 | ENTITY_ALIGNED_DIMENSION | `ENTITY_ALIGNED_DIMENSION` | `7e20c0288597f4a036c2fc5735b2a4eab67b0ddb0ba8c6bf8ceb4dc81a9391b1` |
| EV-54 | SYMBOL_RECORD_LAYER | `SYMBOL_RECORD_LAYER` | `087023825ee388b5969f85f88be6b95da37e8fb00b7f0c6ba368dc89b91b2856` |
| EV-55 | SYMBOL_RECORD_LTYPE | `SYMBOL_RECORD_LTYPE` | `98c57e7db8a5192a35674b352c4d67946f23dc5064efebc8945f9cfb141f19ad` |
| EV-56 | SYMBOL_RECORD_STYLE | `SYMBOL_RECORD_STYLE` | `39e47384546fd9e832a1e322df84642b79dfdf851d529ad1a9e4ba87d04228b1` |
| EV-57 | SYMBOL_RECORD_DIMSTYLE with every variable of the closed list (illustrative values) | `SYMBOL_RECORD_DIMSTYLE` | `1423f9ee723868053442d1c0defd694cbabbfbfb9789ccb3e0161c7bddf4b848` |
| EV-58 | SYMBOL_RECORD_APPID | `SYMBOL_RECORD_APPID` | `cabe53b3432232e3fe0cf106002246d9c03602fe583e95b9cb401b7d39e3d1e2` |
| EV-59 | NOD_ENTRY, an XRECORD | `NOD_ENTRY` | `8c245e3263eb6231cde7faef67c3d131e85cba9e6c1d8cf2727bb9b1654f9710` |
| EV-60 | NOD_ENTRY, a DICTIONARY (children sorted) | `NOD_ENTRY` | `4158db1b68e001e85d99f87f7e74be5f71abfbdd1fc97f9298c1372ac606fca6` |
| EV-61 | ENUMERATION, unordered names (sorted by the producer) | `ENUMERATION` | `b7b139ab83366e3ce352d67ce8ffac53d62bc5e54123b09c63fe5fcf79826f59` |
| EV-62 | PRE_COMMAND_STATE_RECORD with a record-level ABSENT member | `PRE_COMMAND_STATE_RECORD` | `53e63bb0f53f8d7e22054811bc103b9e03814f617d9deabfe8076979b57856de` |
| EV-63 | PRE_COMMAND_STATE_RECORD with an UNKNOWN member: UNKNOWN | `PRE_COMMAND_STATE_RECORD` | **UNKNOWN** (a member of the PRE_COMMAND_STATE_RECORD is UNKNOWN: no partial digest (refusal O1, V2.1 6)) |
| EV-64 | PREPARE_RESIDUE_MANIFEST | `PREPARE_RESIDUE_MANIFEST` | `2c30a265f0dfba0b5640f8bdb818f1bc158802de520f387b6163a657b886b548` |

Relations that the vectors pin: EV-01 != EV-02 (signed zero); EV-01 != EV-03 (1 ulp); EV-08 != EV-09 (case); EV-13 != EV-14 (sequence order is state); EV-15 == EV-16 and EV-21 == EV-22 (set order and field order); EV-19 != EV-20 (an unknown member changes the payload digest); EV-26 != EV-27 != EV-28 (chunk boundaries and type codes are state).

## 3. Layer 2: `SEMANTIC_COMPARISON`

### 3.1 Comparator responsibilities and tolerance slots

The comparator parses the persisted record with the **authoritative reader** into its typed form and compares it with the expected typed form computed by the oracle (BA-08 V3, `ORACLE_SPEC`). It is independent of the writer and of `mu_k`. **The tolerance values live only in BA-08 V3 section 10** (V2.2 PA-9 assigns them to the Selective kind baseline); this section names the slots (AR2-35).

| Compared value | Rule | Slot (value in BA-08 V3 section 10) |
|---|---|---|
| positions, text heights, dimension points, lengths | predicate `ABS_DIFF_LE(TOL_LENGTH)` | `TOL_LENGTH` |
| rotations | predicate `ANGLE_DIFF_LE(TOL_ANGLE)` after `NORM_ANGLE` | `TOL_ANGLE`, `NORM_ANGLE` |
| normals | predicate `NORMAL_ANGLE_LE(TOL_NORMAL)` | `TOL_NORMAL` |
| dynamic-block property values that are lengths | `ABS_DIFF_LE(TOL_DYNAMIC)` | `TOL_DYNAMIC` |
| **scale factors** | `ABS_DIFF_LE(TOL_SCALE)`, absolute | **`TOL_SCALE` = UNSET, no authority for CT-21D** (K-3); a comparison that needs it cannot be declared |
| texts, names, enum values | `EXACT_STRING` | none |
| sibling sets | unordered | none |
| sequences whose order the plan determines | order-sensitive | none |
| authored payload and envelope | typed members, `ExtensionData` and unknown members (section 3.2), `CANONICAL_JSON_EQUAL` | none |
| unrecognized schema version, unsupported type, unparsable record | `UNKNOWN` | none |

### 3.1.1 Predicates (normative)

| Predicate | Definition |
|---|---|
| `ABS_DIFF_LE(TOL)` | /a - b/ <= TOL, per scalar component, absolute (never relative); -0.0 equals +0.0; a non-finite value is UNKNOWN |
| `ANGLE_DIFF_LE(TOL_ANGLE)` | d = /a - b/ reduced modulo 2pi into [0, 2pi); EQUAL when min(d, 2pi - d) <= TOL_ANGLE |
| `NORMAL_ANGLE_LE(TOL_NORMAL)` | both vectors normalized (a zero-length vector is UNKNOWN); EQUAL when the angle between them <= TOL_NORMAL |
| `EXACT_STRING` | ordinal equality of the stored text |
| `CANONICAL_JSON_EQUAL` | both sides parsed; objects compared as maps with ordinally sorted keys; arrays by position; strings exact; numbers by exact JSON number token; a member the reader does not expose is UNKNOWN_MEMBER_UNSUPPORTED |
| `SCHEMA_VERSION_RECOGNIZED` | an unrecognized schema version makes the comparison UNKNOWN |
| `TYPE_SUPPORTED` | a record type outside the specification makes the comparison UNKNOWN |

### 3.2 `ExtensionData`, authored and unknown members (V2.2 PA-8)

The typed form compared for the authored payload and envelope includes `ExtensionData`, the authored and forward-compatible members, and the unknown members preserved by contract (I-11). Unknown and extension members are compared exactly after canonical serialization: object keys in ordinal order, **numbers by their exact JSON number token** (so `1` and `1.0` differ: vector SV-12). A writer that **drops or alters** an unknown member **does not pass** (DIFFERENT; O4 through S-2 or S-3). A member that cannot be compared yields **`UNKNOWN_MEMBER_UNSUPPORTED`**, surfaced explicitly, giving `UNKNOWN` (O5 path). An unrecognized schema version is `UNKNOWN`.

### 3.3 NORMATIVE semantic comparison vectors (typed inputs; BA05-F08)

Doubles are given as the 16-hex binary64 pattern; JSON as the exact text.

| Id | Record and field (form) | Expected | Persisted | Predicate | Result | Rule |
|---|---|---|---|---|---|---|
| SV-01 | `ENVELOPE` `ExtensionData` (typed (parsed JSON)) | `{"Id":"g-1","ExtensionData":{"futureKey":1}}` | `{"ExtensionData":{"futureKey":1},"Id":"g-1"}` | `CANONICAL_JSON_EQUAL` | **EQUAL** | object keys compared after ordinal sorting; the extension member present and equal |
| SV-02 | `ENVELOPE` `ExtensionData` (typed (parsed JSON)) | `{"Id":"g-1","ExtensionData":{"futureKey":1}}` | `{"Id":"g-1"}` | `CANONICAL_JSON_EQUAL` | **DIFFERENT** | a writer that DROPS an unknown member does not pass (O4 through S-2/S-3) |
| SV-03 | `ENVELOPE` `ExtensionData` (typed (parsed JSON)) | `{"Id":"g-1","ExtensionData":{"futureKey":1}}` | `{"Id":"g-1","ExtensionData":{"futureKey":2}}` | `CANONICAL_JSON_EQUAL` | **DIFFERENT** | a writer that ALTERS an unknown member does not pass |
| SV-04 | `DESIGN` `a known sub-object whose type declares no ExtensionData` (typed (parsed JSON)) | `{"SchemaVersion":"1.0","Bays":[{"Levels":2}]}` | `{"SchemaVersion":"1.0","Bays":[{"Levels":2,"futureKey":1}]}` | `CANONICAL_JSON_EQUAL` | **UNKNOWN_MEMBER_UNSUPPORTED** | the unknown member sits in a sub-object with no ExtensionData (JsonExtensionData is not recursive, I-11): surfaced explicitly, gives UNKNOWN (O5 path) |
| SV-05 | `ENTITY_TEXT` `positionY` (typed (binary64)) | `0000000000000000` | `8000000000000000` | `ABS_DIFF_LE(TOL_LENGTH)` | **EQUAL** | signed zero is domain-equivalent; secondary finding BIT_LEVEL_DIFFERENCE_DOMAIN_EQUIVALENT; the exact digests differ (EV-01 / EV-02) |
| SV-06 | `ENTITY_LINE` `endX` (typed (binary64)) | `4024000000000000` | `4024000000044b83` | `ABS_DIFF_LE(TOL_LENGTH)` | **EQUAL** | /a - b/ = 5e-10 <= TOL_LENGTH = 1e-9 in (per component, absolute) |
| SV-07 | `ENTITY_LINE` `endX` (typed (binary64)) | `4024000000000000` | `4024000000112e0c` | `ABS_DIFF_LE(TOL_LENGTH)` | **DIFFERENT** | /a - b/ = 2e-9 > TOL_LENGTH |
| SV-08 | `ENVELOPE` `SchemaVersion` (typed (string)) | `1.0` | `9.0` | `SCHEMA_VERSION_RECOGNIZED` | **UNKNOWN** | an unrecognized schema version is UNKNOWN |
| SV-09 | `ENTITY_PROXY` `(record)` (typed) | `a record of a type outside the supported list` | `same` | `TYPE_SUPPORTED` | **UNKNOWN** | layer-2 consequence: the comparison is UNKNOWN (the O1 consequence for closure members belongs to section 2.5) |
| SV-10 | `ENTITY_INSERT` `rotation` (typed (binary64, radians)) | `0000000000000000` | `401921fb543b9612` | `ANGLE_DIFF_LE(TOL_ANGLE)` | **EQUAL** | d = /a - b/ mod 2pi; min(d, 2pi - d) = 5e-10 <= TOL_ANGLE = 1e-9 rad |
| SV-11 | `ENTITY_INSERT` `normal` (typed (binary64 vector)) | `0000000000000000 / 0000000000000000 / 3ff0000000000000` | `3e212e0be826d695 / 0000000000000000 / 3ff0000000000000` | `NORMAL_ANGLE_LE(TOL_NORMAL)` | **DIFFERENT** | angle between the normalized vectors = atan(2e-9) > TOL_NORMAL = 1e-9 rad; a zero-length vector is UNKNOWN |
| SV-12 | `ENVELOPE` `ExtensionData.futureKey` (typed (JSON number token)) | `1` | `1.0` | `CANONICAL_JSON_EQUAL` | **DIFFERENT** | PA-8 "exact numbers": an unknown or extension number is compared by its exact JSON number token (1 and 1.0 differ); whitespace and key order are not significant |
| SV-13 | `ENTITY_TEXT` `textString` (typed (string)) | `Nivel 1` | `NIVEL 1` | `EXACT_STRING` | **DIFFERENT** | texts, names and enum values are compared exactly (stored case) |

### 3.4 Use of the layers

| Use | Layer |
|---|---|
| S-1, S-2, S-3, the comparator of SL-6 | `SEMANTIC_COMPARISON` |
| E-08 (S-4), S-5, `PRE_COMMAND_STATE_RECORD`, `PREPARE_RESIDUE_MANIFEST`, `REUSED_BOUND`, `LIBRARY_EQUIVALENCE_OBSERVATION` | `EXACT_STATE_FINGERPRINT` |

## 4. Other artifacts (BA05-F10)

`PRE_COMMAND_STATE_RECORD` and `PREPARE_RESIDUE_MANIFEST` are **record types of this specification** (section 2.3); their digests are the ones V2.1 6 and 7 log. The **loaded-code manifest** (E-10, E-11M, E-13), the custody log and the scenario catalog are not fingerprints: their canonical serialization and hashing are defined by BA-07 V3 (manifest and custody) and BA-11 V3 (artifacts).

## 5. Library entity-type census (procedure and schema; NOT performed)

The census is **required before this artifact is sealed**, and its types are added to this draft as in section 2.6. It reads the library drawing, which requires host work (**not authorized**).

**Procedure (for a future host-authorized gate).** On the private copy of the exact library file of the baseline (a tuple field), read-only: (1) enumerate every block definition of the static universe of BA-08 V3 section 7 (46 names) and the closure of their nested definitions; (2) for each, record the entity `dxfName` values with counts, the nested block names, the symbol records referenced, the writable dynamic properties exposed (BA-08 V3 section 11 rule 2) and whether any proxy or custom entity is present; (3) compute the type universe; (4) **add to this draft** (new `ARTIFACT_VERSION`, `FINGERPRINT_SPEC_VERSION` unchanged) a table and at least one vector for every type outside section 2.3.

**Schema of the census record.**

| Field | Content |
|---|---|
| `libraryFileSha256`, `libraryPath`, `censusUtc`, `instrumentBuild` | identity of the census |
| `blocks[]` | `blockName`, `requiredBy` (BA-08 V3 roles), `entityTypes[]` (`dxfName`, `count`), `nestedBlocks[]`, `symbolRecords` (`layers[]`, `linetypes[]`, `textStyles[]`, `dimStyles[]`), `dynamicProperties[]` (`name`, `writable`), `containsProxyOrCustom` |
| `typeUniverse[]` | the distinct `dxfName` values |
| `typesWithoutTable[]` | the types outside section 2.3 at the time of the census |

## 6. What keeps NB-3 open

| Id | Item | Gate |
|---|---|---|
| L-1 | the library census (section 5) and the tables of its types | host (not authorized) |
| L-2 | `TOL_SCALE` decision, test and record (BA-08 V3 K-3) | separate decision; not decided by this session |
| L-3 | candidate ruling, ratifications and seal of this artifact (document + vectors + reference encoder) | baseline review |

```text
NB-3 = ADVANCES_TO_BASELINE_ARTIFACT_PREPARATION ; OPEN (census, TOL_SCALE)
FPSPEC-V1 vectors (V3 files) = 56 exact (49 digests, 7 UNKNOWN) + 13 semantic (normative when sealed)
CT21D_AUTHORITY_BASELINE_READY = FALSE
```
