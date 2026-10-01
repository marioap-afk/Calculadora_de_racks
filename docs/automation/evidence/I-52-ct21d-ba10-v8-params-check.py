#!/usr/bin/env python3
"""I-52 CT-21D BA-10 V8: reproducibility check of the Owner-derived counts N_A, N_B, N_det, n (decisions section 233).

Usage: python I-52-ct21d-ba10-v8-params-check.py [--sheet <BA-10 V8 sheet>] [--write <output json>]

Read-only except for the optional --write target. Standard library only; no host, no AutoCAD, no network.

Inputs:
  * the Owner pairs recorded verbatim in docs/automation/evidence/I-52-ct21d-owner-decisions-q-o1-q-o5-and-host-gate.md
    (embedded below as OWNER_PAIRS, as exact decimal strings) and the counts the Owner reported (OWNER_COUNTS);
  * the "value block" of the BA-10 V8 sheet (section 2.4: the lines `p_A = ...`, `N_A = ...`, ...), read from --sheet
    (default: the V8 sheet next to this file's repository layout).

Method (BA-10 V8 sections 2.2 PARAM-01, PARAM-02, PARAM-03):
  zero-failure detection  N >= ln(alpha) / ln(1 - p)   (rounded up), for (p_A, alpha_A), (p_B, alpha_B), (p_det, alpha_det);
  false-rejection sample  n >= ln(1 - c) / ln(1 - u)   (rounded up), i.e. the same formula with p = u and alpha = 1 - c;
  k = 0 one-sided Clopper-Pearson upper bound at confidence c:  1 - (1 - c)^(1 / n).
Every count is computed twice: with the logarithm formula in 60-digit decimal arithmetic and exactly, as the smallest
integer N with (1 - p)^N <= alpha in rational arithmetic. The two must agree.

Exit code 0 iff: both computations agree for every count; the counts equal OWNER_COUNTS; the k = 0 bound is above u at
n - 1 and at most u at n; WU_DRY_RUNS equals N_B; and (when the sheet is readable) the value block of the sheet equals
the recomputed table. Exit code 1 on any mismatch. Exit code 2 on a usage error (a --sheet path that cannot be read).
Output: JSON on stdout (sorted keys, LF). The sheet's hash is not recorded (the sheet cites this script by path only).
"""
import json
import os
import re
import sys
from decimal import Decimal, getcontext
from fractions import Fraction

getcontext().prec = 60

# Owner pairs, verbatim (decisions section 233; Q-O1..Q-O4). Exact decimal strings.
OWNER_PAIRS = {
    "A": {"p": "0.10", "alpha": "0.01"},
    "B": {"p": "0.10", "alpha": "0.05"},
    "det": {"p": "0.05", "alpha": "0.01"},
    "clean": {"u": "0.05", "c": "0.95"},
}
# Counts as reported by the Owner (N_clean = "n" of PARAM-03).
OWNER_COUNTS = {"N_A": 44, "N_B": 29, "N_det": 90, "n": 59, "WU_DRY_RUNS": 29}

DEFAULT_SHEET = os.path.join(
    os.path.dirname(os.path.abspath(__file__)), "..", "..", "initiatives",
    "I-52-ct21d-baseline-ba-10-parameter-sheet-v8.md")

SHEET_KEYS = ["p_A", "alpha_A", "p_B", "alpha_B", "p_det", "alpha_det", "u", "c",
              "N_A", "N_B", "N_det", "n", "WU_DRY_RUNS"]


def count_log(alpha, p):
    """ceil(ln(alpha) / ln(1 - p)) in 60-digit decimal arithmetic; returns (ratio, count)."""
    ratio = Decimal(alpha).ln() / (Decimal(1) - Decimal(p)).ln()
    count = int(ratio.to_integral_value(rounding="ROUND_CEILING"))
    return ratio, count


def count_exact(alpha, p):
    """Smallest integer N >= 1 with (1 - p)^N <= alpha, in exact rational arithmetic."""
    q = 1 - Fraction(p)
    a = Fraction(alpha)
    k, power = 0, Fraction(1)
    while power > a:
        power *= q
        k += 1
    return k


def bound_k0(c, n):
    """One-sided Clopper-Pearson upper bound for k = 0: 1 - (1 - c)^(1 / n), 60-digit decimal arithmetic."""
    return Decimal(1) - ((Decimal(1) - Decimal(c)).ln() / Decimal(n)).exp()


def fmt(x, digits=30):
    return format(x, "." + str(digits) + "f")


def read_sheet_block(path):
    values, problems = {}, []
    with open(path, "r", encoding="utf-8", newline="") as fh:
        text = fh.read()
    if "\r" in text:
        problems.append("sheet contains CR characters (not LF)")
    for key in SHEET_KEYS:
        found = re.findall(r"^" + re.escape(key) + r" = (\S+)\s*$", text, flags=re.M)
        if len(found) != 1:
            problems.append("sheet value block: key %s found %d times (expected 1)" % (key, len(found)))
        else:
            values[key] = found[0]
    return values, problems


def main(argv):
    sheet, write = None, None
    args = list(argv[1:])
    while args:
        a = args.pop(0)
        if a == "--sheet" and args:
            sheet = args.pop(0)
        elif a == "--write" and args:
            write = args.pop(0)
        else:
            sys.stderr.write(__doc__)
            return 2
    sheet_explicit = sheet is not None
    sheet = sheet or DEFAULT_SHEET

    mismatches = []
    rows = {}
    for cls, key, name in (("A", "N_A", "PARAM-01 class A"), ("B", "N_B", "PARAM-01 class B"),
                           ("det", "N_det", "PARAM-02")):
        p, alpha = OWNER_PAIRS[cls]["p"], OWNER_PAIRS[cls]["alpha"]
        ratio, via_log = count_log(alpha, p)
        via_exact = count_exact(alpha, p)
        rows[key] = {"alpha": alpha, "count_exact": via_exact, "count_log": via_log, "p": p,
                     "parameter": name, "ratio_ln_alpha_over_ln_1_minus_p": fmt(ratio, 15)}
        if via_log != via_exact:
            mismatches.append("%s: log formula %d != exact %d" % (key, via_log, via_exact))
        if via_exact != OWNER_COUNTS[key]:
            mismatches.append("%s: recomputed %d != table %d" % (key, via_exact, OWNER_COUNTS[key]))

    u, c = OWNER_PAIRS["clean"]["u"], OWNER_PAIRS["clean"]["c"]
    one_minus_c = str(Decimal(1) - Decimal(c))
    ratio, n_log = count_log(one_minus_c, u)
    n_exact = count_exact(one_minus_c, u)
    rows["n"] = {"alpha_equivalent_1_minus_c": one_minus_c, "c": c, "count_exact": n_exact, "count_log": n_log,
                 "parameter": "PARAM-03", "ratio_ln_1_minus_c_over_ln_1_minus_u": fmt(ratio, 15), "u": u}
    if n_log != n_exact:
        mismatches.append("n: log formula %d != exact %d" % (n_log, n_exact))
    if n_exact != OWNER_COUNTS["n"]:
        mismatches.append("n: recomputed %d != table %d" % (n_exact, OWNER_COUNTS["n"]))
    n = n_exact

    b_before, b_at = bound_k0(c, n - 1), bound_k0(c, n)
    bound = {"formula": "1 - (1 - c)^(1 / n)", "k": 0, "c": c, "u": u,
             "at_n_minus_1": {"n": n - 1, "upper_bound": fmt(b_before), "at_most_u": b_before <= Decimal(u)},
             "at_n": {"n": n, "upper_bound": fmt(b_at), "at_most_u": b_at <= Decimal(u)}}
    if b_before <= Decimal(u):
        mismatches.append("k=0 bound at n-1=%d is at most u (n is not minimal)" % (n - 1))
    if b_at > Decimal(u):
        mismatches.append("k=0 bound at n=%d exceeds u" % n)

    wu = rows["N_B"]["count_exact"]
    if wu != OWNER_COUNTS["WU_DRY_RUNS"]:
        mismatches.append("WU_DRY_RUNS: N_B %d != table %d" % (wu, OWNER_COUNTS["WU_DRY_RUNS"]))

    recomputed = {"N_A": rows["N_A"]["count_exact"], "N_B": rows["N_B"]["count_exact"],
                  "N_det": rows["N_det"]["count_exact"], "n": n, "WU_DRY_RUNS": wu}

    sheet_report = {"checked": False}
    if os.path.exists(sheet) or sheet_explicit:
        try:
            values, problems = read_sheet_block(sheet)
        except OSError as exc:
            sys.stderr.write("cannot read the sheet: %s\n" % exc)
            return 2
        mismatches.extend(problems)
        expected = {"p_A": OWNER_PAIRS["A"]["p"], "alpha_A": OWNER_PAIRS["A"]["alpha"],
                    "p_B": OWNER_PAIRS["B"]["p"], "alpha_B": OWNER_PAIRS["B"]["alpha"],
                    "p_det": OWNER_PAIRS["det"]["p"], "alpha_det": OWNER_PAIRS["det"]["alpha"],
                    "u": u, "c": c}
        for k, v in recomputed.items():
            expected[k] = str(v)
        for k in SHEET_KEYS:
            if k in values and values[k] != expected[k]:
                mismatches.append("sheet value block: %s = %s but the recomputed table says %s" % (k, values[k], expected[k]))
        sheet_report = {"checked": True, "keys": len(SHEET_KEYS), "path": os.path.basename(sheet)}

    report = {
        "artifact": "I-52-ct21d-ba10-v8-params-check",
        "k0_bound": bound,
        "mismatches": mismatches,
        "owner_counts": OWNER_COUNTS,
        "owner_pairs": OWNER_PAIRS,
        "recomputed": recomputed,
        "rows": rows,
        "sheet": sheet_report,
        "verdict": "MATCH" if not mismatches else "MISMATCH",
    }
    text = json.dumps(report, indent=2, sort_keys=True, ensure_ascii=True) + "\n"
    if write:
        with open(write, "w", encoding="utf-8", newline="\n") as fh:
            fh.write(text)
    sys.stdout.write(text)
    return 0 if not mismatches else 1


if __name__ == "__main__":
    sys.exit(main(sys.argv))
