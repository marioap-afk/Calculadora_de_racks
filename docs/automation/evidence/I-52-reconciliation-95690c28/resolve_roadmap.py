"""Resolve a docs/ROADMAP.md rebase conflict (class B) without dropping any row.
Case 1: pure additions on both sides (empty base) -> keep both, branch rows first.
Case 2: main kept the base rows unchanged and appended rows; the branch edited those base rows -> branch rows + main's appended rows.
Any other shape fails (manual ruling needed)."""
import io
import re
import sys

NL = '\n'
p = sys.argv[1]
raw = io.open(p, encoding='utf-8', newline='').read()
crlf = '\r\n' in raw
s = raw.replace('\r\n', NL)
pat = re.compile(r'<<<<<<< [^\n]*\n(.*?)\|\|\|\|\|\|\| [^\n]*\n(.*?)=======\n(.*?)>>>>>>> [^\n]*\n', re.S)
n = 0


def lines(block):
    return [x for x in block.strip(NL).split(NL) if x]


def fix(m):
    global n
    ol, bl, tl = lines(m.group(1)), lines(m.group(2)), lines(m.group(3))
    for block in (ol, bl, tl):
        assert all(x.startswith('| ') for x in block), 'non-row line in hunk: manual ruling needed'
    n += 1
    if not bl:
        return NL.join(tl + ol) + NL
    assert ol[:len(bl)] == bl, 'main changed a base row: manual ruling needed'
    return NL.join(tl + ol[len(bl):]) + NL


s2 = pat.sub(fix, s)
assert '<<<<<<<' not in s2 and '>>>>>>>' not in s2
if crlf:
    s2 = s2.replace(NL, '\r\n')
io.open(p, 'w', encoding='utf-8', newline='').write(s2)
print('resolved hunks:', n)
