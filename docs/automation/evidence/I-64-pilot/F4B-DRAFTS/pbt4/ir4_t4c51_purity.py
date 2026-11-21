"""CA-T4c recheck, I-01: the NEWFILE blocks of the v5.1 pieces (GREEN and skeleton), with the purity of CT-PBT3A-14 as CT-15 copies it
(the nullable directive, one namespace in block form, the forbidden terms; PushBackFullReadGuardTests.cs:286-300 at the revision) and the
rule of section 5.0 (no '?' on a reference type in a file without '#nullable enable'), for every new file (read-only).

Usage: python -I ir4_t4c51_purity.py <green-piece> <skeleton-piece> <out.txt>
"""
import re
import sys

green_p, skel_p, out_p = sys.argv[1:4]
FORBIDDEN = ["Autodesk", "RackCad.Plugin", "RackCad.UI", "System.Windows", "Transaction", "Database", "async", "await", "Task", "File.",
             "System.IO", "RackEmbedComposer", "new RackEmbedDocument"]


def purity(source):
    out = []
    if re.search(r"^\s*#nullable", source, re.M):
        out.append("#nullable")
    namespaces = len(re.findall(r"^\s*namespace\s", source, re.M))
    block = len(re.findall(r"^namespace RackCad\.Application\.Systems\.PushBack\.Draft\s*\r?\n\{", source, re.M))
    if namespaces != 1 or block != 1:
        out.append("namespace RackCad.Application.Systems.PushBack.Draft")
    out += [t for t in FORBIDDEN if t in source]
    return out


def newfiles(piece):
    text = open(piece, encoding="utf-8").read().replace("\r\n", "\n")
    out = {}
    for m in re.finditer(r"^@@@@ NEWFILE (\S+)\n(.*?)^@@@@ END$", text, re.M | re.S):
        out[m.group(1)] = m.group(2)
    fromgreen = re.findall(r"^@@@@ FROMGREEN (\S+)$", text, re.M)
    return out, fromgreen


def code_only(t):
    t = re.sub(r'@"(?:[^"]|"")*"', '""', t)
    t = re.sub(r'"(?:\\.|[^"\\\n])*"', '""', t)
    t = re.sub(r"/\*.*?\*/", "", t, flags=re.S)
    return re.sub(r"//[^\n]*", "", t)


# a '?' that annotates a type: after an identifier or '>' or ']' and before an identifier, ')' ',' '>' or '[' (not '?.', '??', '?:' or '? x :')
NULLABLE = re.compile(r"\b[A-Z]\w*(?:<[^<>;]*>)?\?(?=\s+\w+\s*[;=,)(]|\s*[,)>\]])")
L = []
for label, piece in (("GREEN", green_p), ("skeleton", skel_p)):
    files, fromgreen = newfiles(piece)
    L.append("== %s: NEWFILE %d; FROMGREEN %s" % (label, len(files), fromgreen))
    for path, text in sorted(files.items()):
        directive = bool(re.search(r"^\s*#nullable enable", text, re.M))
        annotations = sorted(set(NULLABLE.findall(code_only(text))))
        line = "  %s: #nullable=%s; reference '?' annotations=%s" % (path, directive, annotations)
        if "/Systems/PushBack/Draft/" in path:
            line += "; CT-15 purity violations=%s" % purity(text)
        if not directive and annotations:
            line += "  <== section 5.0 rule broken"
        L.append(line)
open(out_p, "w", encoding="utf-8").write("\n".join(L) + "\n")
print("\n".join(L))
