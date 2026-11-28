import re
import subprocess
import sys

# Uso: python -I cat3c_enum.py <checkout> <rev>
# Las guardas que leen TODOS los .cs de src/ o del Plugin (Directory.GetFiles / EnumerateFiles): la fachada nueva de PB-T3c entra en su
# corpus sin que nadie la nombre. Para cada una: la linea de la enumeracion, su filtro, y los literales de la guarda que casan con el
# cuerpo previsto de la fachada (el de EraseViewBlocks para una definicion, D-PBT3-11) o con su firma. No escribe nada.
sys.stdout.reconfigure(encoding="utf-8", errors="replace")
REPO, REV = sys.argv[1], sys.argv[2]
FACADE = """#nullable enable
using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace RackCad.Plugin.Views
{
    internal static class RackViewBlockEraser
    {
        internal static IReadOnlyCollection<ObjectId> EraseInTransaction(Transaction transaction, ObjectId definitionId)
        {
            if (!(transaction.GetObject(definitionId, OpenMode.ForRead) is BlockTableRecord definition) || definition.IsLayout)
            {
                throw new InvalidOperationException("La definicion no es una vista.");
            }

            var nested = new HashSet<ObjectId>();
            foreach (ObjectId childId in definition)
            {
                if (transaction.GetObject(childId, OpenMode.ForRead) is BlockReference child && !child.BlockTableRecord.IsNull)
                {
                    nested.Add(child.BlockTableRecord);
                }
            }

            foreach (ObjectId referenceId in definition.GetBlockReferenceIds(directOnly: true, forceValidity: true))
            {
                if (transaction.GetObject(referenceId, OpenMode.ForWrite) is BlockReference reference)
                {
                    reference.Erase();
                }
            }

            definition.UpgradeOpen();
            definition.Erase();
            return nested;
        }
    }
}
"""


def git(*a):
    return subprocess.run(["git", "-C", REPO, *a], capture_output=True, check=True).stdout.decode("utf-8", "replace")


files = [p for p in git("ls-tree", "-r", "--name-only", REV, "tests").split("\n") if p.endswith(".cs")]
enum_rx = re.compile(r"(GetFiles|EnumerateFiles)\s*\(")
lit_rx = re.compile(r'@"((?:[^"]|"")*)"|(?<!@)"((?:\\.|[^"\\\n])*)"')
for p in files:
    t = git("show", REV + ":" + p)
    if not enum_rx.search(t):
        continue
    lines = t.split("\n")
    hits = [k for k, l in enumerate(lines) if enum_rx.search(l)]
    scope = []
    for k in hits:
        ctx = " ".join(x.strip() for x in lines[max(0, k - 3):k + 4])
        if re.search(r'"src"|RackCad\.Plugin|PluginDir|Plugin"|pluginDir|"src/|SrcRoot|SourceRoot|Root\(\)|\broot\b|RepoRoot', ctx):
            scope.append((k + 1, re.sub(r"\s+", " ", lines[k].strip())[:150]))
    if not scope:
        continue
    match = []
    for m in lit_rx.finditer(t):
        s = m.group(1) if m.group(1) is not None else m.group(2)
        if m.group(1) is not None:
            s = s.replace('""', '"')
        else:
            s = s.encode("utf-8").decode("unicode_escape", "replace") if "\\" in s else s
        if len(s) < 3:
            continue
        try:
            rx = re.compile(s)
        except re.error:
            rx = None
        found = (rx.search(FACADE) if rx else None) or (s in FACADE and None)
        if (rx and found) or (s in FACADE):
            line = t[:m.start()].count("\n") + 1
            match.append((line, s[:90]))
    print("== %s" % p)
    for k, s in scope:
        print("   enum :%d %s" % (k, s))
    print("   literales que casan con la fachada prevista: %d" % len(match))
    for line, s in match[:14]:
        print("      :%d %r" % (line, s))
    if len(match) > 14:
        print("      ...")
