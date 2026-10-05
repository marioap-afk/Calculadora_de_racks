r"""I-62 F4 preparation (DEP-F4-YAML): minimal FAIL-CLOSED reader for the exact YAML subset of `rackcad-automation-state/v2`.

Prototype, not production (F4 ports it to C# in tests/RackCad.Tests; no package dependency). It is NOT a general YAML parser: anything outside the
subset raises YamlSubsetError with the line number.

Subset (the canonical serialization that the F4 writer must emit):
  - block mappings `key: value` and `key:` + an indented block; keys are [A-Za-z_][A-Za-z0-9_]* (snake_case of B.8);
  - block sequences `- item`, indented under their key; items are scalars or block mappings (`- k: v` continued at the item indentation);
  - indentation by spaces only, a fixed step per nesting level (any positive step, but consistent inside one block);
  - scalars: plain (single line, no leading indicator, no ': ' and no ' #'), double-quoted (\\ \" \n \t \/ \uXXXX) or single-quoted (''), single line;
  - typed plain scalars: null, true, false, integers (0 or -?[1-9][0-9]*); everything else is a string. A writer must quote a string that would
    otherwise read as another type;
  - empty collections only as the exact flow literals [] and {};
  - full-line comments (#) and blank lines.
Rejected: anchors (&), aliases (*), tags (!), directives (%), document markers (--- / ...), block scalars (| >), flow collections other than [] {},
complex keys (?), tabs, duplicate keys, inconsistent indentation, an empty value without a nested block, multi-line quoted scalars, a value after a
nested block, trailing comments.

Modes: STRICT (the whole file is in the subset: state/v2); HEADER (state/v1 for the E.2 classifier, which consumes only `schema` and
`automation_state.claim_id`, plus initiative and branch: those are parsed strictly; everything else is skipped as opaque and NEVER consumed).
"""
import re

KEY = re.compile(r"^([A-Za-z_][A-Za-z0-9_]*):(?: (.*))?$")
INT = re.compile(r"^(0|-?[1-9][0-9]*)$")
INDICATORS = set("[]{}&*!|>'\"%@`#,?")


class YamlSubsetError(ValueError):
    def __init__(self, line, msg):
        super().__init__("line %d: %s" % (line, msg))
        self.line = line


def _scalar(text, ln):
    t = text
    if t == "":
        raise YamlSubsetError(ln, "empty scalar")
    if t in ("[]",):
        return []
    if t in ("{}",):
        return {}
    if t[0] == '"':
        if len(t) < 2 or t[-1] != '"':
            raise YamlSubsetError(ln, "unterminated or multi-line double-quoted scalar")
        body, out, i = t[1:-1], [], 0
        while i < len(body):
            c = body[i]
            if c == '"':
                raise YamlSubsetError(ln, "unescaped quote inside a double-quoted scalar")
            if c == "\\":
                if i + 1 >= len(body):
                    raise YamlSubsetError(ln, "dangling escape")
                e = body[i + 1]
                if e in '"\\/':
                    out.append(e); i += 2
                elif e == "n":
                    out.append("\n"); i += 2
                elif e == "t":
                    out.append("\t"); i += 2
                elif e == "u" and re.match(r"^[0-9A-Fa-f]{4}$", body[i + 2:i + 6] or ""):
                    out.append(chr(int(body[i + 2:i + 6], 16))); i += 6
                else:
                    raise YamlSubsetError(ln, "unsupported escape \\" + e)
            else:
                out.append(c); i += 1
        return "".join(out)
    if t[0] == "'":
        if len(t) < 2 or t[-1] != "'":
            raise YamlSubsetError(ln, "unterminated or multi-line single-quoted scalar")
        body = t[1:-1]
        if re.search(r"(?<!')'(?!')", body.replace("''", "")):
            raise YamlSubsetError(ln, "unescaped quote inside a single-quoted scalar")
        return body.replace("''", "'")
    if t[0] in INDICATORS or t.startswith("- ") or t == "-":
        raise YamlSubsetError(ln, "plain scalar starts with an indicator: " + t[:20])
    if ": " in t or t.endswith(":"):
        raise YamlSubsetError(ln, "plain scalar contains ': ' (quote it)")
    if " #" in t:
        raise YamlSubsetError(ln, "plain scalar contains ' #' (comment or ambiguity: quote it)")
    if t != t.strip():
        raise YamlSubsetError(ln, "plain scalar with surrounding spaces")
    if t == "null":
        return None
    if t == "true":
        return True
    if t == "false":
        return False
    if INT.match(t):
        return int(t)
    if t in ("~", "Null", "NULL", "True", "TRUE", "False", "FALSE", "yes", "no", "on", "off") or re.match(r"^-?0[0-9]+$", t) or re.match(r"^[-+]?(\.[0-9]+|[0-9]+\.[0-9]*)([eE][-+]?[0-9]+)?$", t):
        raise YamlSubsetError(ln, "ambiguous plain scalar (YAML would type it differently): " + t)
    return t


def _lines(text):
    out = []
    for n, raw in enumerate(text.replace("\r\n", "\n").split("\n"), 1):
        if "\t" in raw[:len(raw) - len(raw.lstrip(" \t"))]:
            raise YamlSubsetError(n, "tab in indentation")
        s = raw.rstrip(" ")
        if s.strip() == "" or s.lstrip().startswith("#"):
            continue
        if s.startswith(("---", "...")) and s.strip() in ("---", "..."):
            raise YamlSubsetError(n, "document markers are not supported")
        if s.startswith("%"):
            raise YamlSubsetError(n, "directives are not supported")
        ind = len(s) - len(s.lstrip(" "))
        out.append((n, ind, s[ind:]))
    return out


def _parse_block(lines, i, ind):
    """Parse the block whose first line is lines[i] at indentation ind. Returns (value, next index)."""
    n, li, body = lines[i]
    if body.startswith("- ") or body == "-":
        return _parse_seq(lines, i, ind)
    return _parse_map(lines, i, ind)


def _value_after_key(lines, i, ind, rest, ln):
    """rest = text after 'key: ' (None if 'key:' alone). Returns (value, next index)."""
    if rest is not None:
        if rest.startswith(("|", ">")):
            raise YamlSubsetError(ln, "block scalars (| >) are not supported")
        if rest.startswith(("&", "*", "!")):
            raise YamlSubsetError(ln, "anchors, aliases and tags are not supported")
        if rest.startswith(("[", "{")) and rest not in ("[]", "{}"):
            raise YamlSubsetError(ln, "flow collections other than [] and {} are not supported")
        v = _scalar(rest, ln)
        if i + 1 < len(lines) and lines[i + 1][1] > ind:
            raise YamlSubsetError(lines[i + 1][0], "indented content after a scalar value (multi-line scalar)")
        return v, i + 1
    if i + 1 >= len(lines) or lines[i + 1][1] <= ind:
        if i + 1 < len(lines) and lines[i + 1][1] == ind and lines[i + 1][2].startswith("- "):
            raise YamlSubsetError(lines[i + 1][0], "sequence must be indented under its key")
        raise YamlSubsetError(ln, "empty value without a nested block (write null, [] or {})")
    return _parse_block(lines, i + 1, lines[i + 1][1])


def _parse_map(lines, i, ind):
    out, start = {}, i
    while i < len(lines):
        n, li, body = lines[i]
        if li < ind:
            break
        if li > ind:
            raise YamlSubsetError(n, "unexpected indentation")
        if body.startswith("- ") or body == "-":
            raise YamlSubsetError(n, "sequence item where a mapping key was expected")
        if body.startswith("?"):
            raise YamlSubsetError(n, "complex keys are not supported")
        m = KEY.match(body)
        if not m:
            if body.startswith(('"', "'")):
                raise YamlSubsetError(n, "quoted keys are not supported")
            raise YamlSubsetError(n, "malformed mapping entry: " + body[:40])
        k, rest = m.group(1), m.group(2)
        if k in out:
            raise YamlSubsetError(n, "duplicate key: " + k)
        if rest is not None and rest.strip() == "":
            rest = None
        out[k], i = _value_after_key(lines, i, ind, rest, n)
    return out, i


def _parse_seq(lines, i, ind):
    out = []
    while i < len(lines):
        n, li, body = lines[i]
        if li < ind:
            break
        if li > ind:
            raise YamlSubsetError(n, "unexpected indentation inside a sequence")
        if not (body.startswith("- ") or body == "-"):
            raise YamlSubsetError(n, "mapping key where a sequence item was expected")
        if body == "-":
            raise YamlSubsetError(n, "empty sequence item")
        item = body[2:]
        if item.startswith("- "):
            raise YamlSubsetError(n, "nested inline sequences are not supported")
        m = KEY.match(item)
        if m:
            # mapping item: its keys live at indentation ind + 2
            sub = [(n, ind + 2, item)]
            j = i + 1
            while j < len(lines) and lines[j][1] > ind:
                sub.append(lines[j])
                j += 1
            val, k = _parse_map(sub, 0, ind + 2)
            if k != len(sub):
                raise YamlSubsetError(sub[k][0], "malformed sequence item")
            out.append(val)
            i = j
        else:
            if item.startswith(("&", "*", "!", "|", ">")):
                raise YamlSubsetError(n, "unsupported construct in a sequence item")
            if item.startswith(("[", "{")) and item not in ("[]", "{}"):
                raise YamlSubsetError(n, "flow collections other than [] and {} are not supported")
            out.append(_scalar(item, n))
            if i + 1 < len(lines) and lines[i + 1][1] > ind:
                raise YamlSubsetError(lines[i + 1][0], "indented content after a scalar item (multi-line scalar)")
            i += 1
    return out, i


def loads(text, mode="STRICT"):
    lines = _lines(text)
    if not lines:
        raise YamlSubsetError(1, "empty document")
    if lines[0][1] != 0:
        raise YamlSubsetError(lines[0][0], "the document must start at column 0")
    if mode == "STRICT":
        v, i = _parse_map(lines, 0, 0)
        if i != len(lines):
            raise YamlSubsetError(lines[i][0], "trailing content")
        return v
    if mode != "HEADER":
        raise ValueError("mode")
    # HEADER (state/v1 for the E.2 classifier): split into top-level blocks. Only `schema` and three direct children of `automation_state`
    # (initiative, branch, claim_id) are consumed, each as a single-line scalar parsed in STRICT form; every other top-level block, and every other
    # child of automation_state together with its deeper continuation lines (e.g. a folded next_action), is skipped and never consumed.
    CONSUMED = ("initiative", "branch", "claim_id")
    blocks, cur = [], None
    for ln in lines:
        if ln[1] == 0:
            cur = [ln]
            blocks.append(cur)
        else:
            if cur is None:
                raise YamlSubsetError(ln[0], "indented content before any key")
            cur.append(ln)
    out, seen = {"__skipped__": []}, set()
    for b in blocks:
        m = KEY.match(b[0][2])
        if not m:
            raise YamlSubsetError(b[0][0], "malformed top-level entry")
        k = m.group(1)
        if k in seen:
            raise YamlSubsetError(b[0][0], "duplicate key: " + k)
        seen.add(k)
        if k == "schema":
            if len(b) != 1 or m.group(2) is None:
                raise YamlSubsetError(b[0][0], "schema must be a single-line scalar")
            out["schema"] = _scalar(m.group(2), b[0][0])
        elif k == "automation_state":
            if m.group(2) not in (None, ""):
                raise YamlSubsetError(b[0][0], "automation_state must be a block mapping")
            if len(b) < 2:
                raise YamlSubsetError(b[0][0], "empty automation_state")
            child_ind, st, current = b[1][1], {}, None
            for n, ind, body in b[1:]:
                if ind == child_ind:
                    cm = KEY.match(body)
                    if not cm:
                        raise YamlSubsetError(n, "malformed automation_state entry")
                    ck = cm.group(1)
                    if ck in st or ck == current:
                        raise YamlSubsetError(n, "duplicate key: " + ck)
                    current = ck
                    if ck in CONSUMED:
                        if cm.group(2) in (None, ""):
                            raise YamlSubsetError(n, ck + " must be a single-line scalar")
                        st[ck] = _scalar(cm.group(2), n)
                    else:
                        st.setdefault("__skipped__", []).append(ck)
                elif ind > child_ind:
                    if current in CONSUMED:
                        raise YamlSubsetError(n, current + " must be a single-line scalar (continuation found)")
                else:
                    raise YamlSubsetError(n, "unexpected indentation in automation_state")
            out["automation_state"] = st
        else:
            out["__skipped__"].append(k)
    return out


import json  # noqa: E402


def write(v, ind=0):
    """Canonical writer for the subset (the form the F4 writer must emit): 2-space steps, sequences indented under the key, strings quoted when needed."""
    pad = " " * ind
    lines = []

    def scal(x):
        if x is None:
            return "null"
        if x is True:
            return "true"
        if x is False:
            return "false"
        if isinstance(x, int):
            return str(x)
        if x == []:
            return "[]"
        if x == {}:
            return "{}"
        try:
            back = _scalar(x, 0)
            if back == x and isinstance(back, str):
                return x
        except YamlSubsetError:
            pass
        return json.dumps(x, ensure_ascii=False)

    for k, val in v.items():
        if isinstance(val, dict) and val:
            lines.append(pad + k + ":")
            lines += write(val, ind + 2)
        elif isinstance(val, list) and val:
            lines.append(pad + k + ":")
            for it in val:
                if isinstance(it, dict) and it:
                    sub = write(it, ind + 4)
                    lines.append(pad + "  - " + sub[0].lstrip())
                    lines += sub[1:]
                else:
                    lines.append(pad + "  - " + scal(it))
        else:
            lines.append(pad + k + ": " + scal(val))
    return lines


def dumps(v):
    return "\n".join(write(v)) + "\n"
