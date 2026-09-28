// D-4 native regression (V35-A3 LOCK-RELEASE-BIND-01, COMMAND-END-WINDOW): the real I52CommandEndWindow classification
// used by R-NATIVE-ARX, driven without ObjectARX. Run as: I52CtdaNativeTests.exe lockrelease
#include "I52CtdaLockRelease.h"

#include <cstdio>
#include <string>
#include <vector>

namespace
{
int lockFailures = 0;

void expect(bool condition, const char* message)
{
    if (condition) return;
    ++lockFailures;
    std::printf("FAIL lock-release: %s\n", message);
}

bool has(const std::wstring& text, const wchar_t* part) { return text.find(part) != std::wstring::npos; }

constexpr int kNotLocked = 2, kWrite = 4;
const std::wstring C = L"I52CTDA_QUEUED";

void eligibleOwnRelease()
{
    I52CommandEndWindow w;
    expect(has(w.open(C), L"stageCommand=I52CTDA_QUEUED") && !w.emptyCommand(), "window opens on the stage command");
    const I52LockDecision d = w.observe(69, true, L"#I52CTDA_QUEUED", kNotLocked, kNotLocked, true);
    expect(d.kind == I52LockClass::Eligible && d.mark && d.token && !d.record, "own #C unlock is the eligible candidate");
    expect(has(d.payload, L"sourceSequence=69") && has(d.payload, L"sourceGlobalCommand=#I52CTDA_QUEUED") && has(d.payload, L"stageCommand=I52CTDA_QUEUED"),
        "MARK carries the source Sequence and global command");
}

void foreignTransitions()
{
    I52CommandEndWindow w;
    w.open(C);
    struct Case { const wchar_t* global; int current; bool unlocked; const char* name; };
    const std::vector<Case> cases = {
        { L"", kWrite, false, "foreign empty command (acquisition)" },
        { L"", kNotLocked, true, "foreign empty command (unlock)" },
        { L"#", kNotLocked, true, "foreign '#' alone" },
        { L"I52CTDA_FINISH", kWrite, false, "next-command acquisition (FINISH)" },
        { L"#I52CTDA_QUEUEDX", kNotLocked, true, "different command sharing the prefix" },
        { L"#REGEN", kNotLocked, true, "another command's release" } };
    uint64_t sequence = 70;
    for (const Case& c : cases)
    {
        const I52LockDecision d = w.observe(sequence, true, c.global, c.current, c.current, c.unlocked);
        expect(d.kind == I52LockClass::Foreign && !d.mark && !d.token && d.record && has(d.payload, L"phase=FOREIGN")
            && has(d.payload, (L"sourceSequence=" + std::to_wstring(sequence)).c_str()), c.name);
        ++sequence;
    }
    expect(w.eligible() == 0 && w.foreign() == 6 && w.inconsistent() == 0, "foreign transitions are neither candidates nor inconsistencies");
    expect(has(w.close(L"NEXT-COMMAND-WILL-START"), L"resolved=0;eligible=0;foreign=6"), "no eligible candidate: marker absence");
}

void duplicateOwnRelease()
{
    I52CommandEndWindow w;
    w.open(C);
    const I52LockDecision first = w.observe(69, true, L"#I52CTDA_QUEUED", kNotLocked, kNotLocked, true);
    const I52LockDecision second = w.observe(75, true, L"#I52CTDA_QUEUED", kNotLocked, kNotLocked, true);
    expect(first.mark && first.token, "first eligible marks and sets the token");
    expect(second.kind == I52LockClass::Duplicate && !second.mark && !second.token && has(second.payload, L"phase=ELIGIBLE-DUPLICATE;"), "a second eligible release is recorded, never marked");
    expect(w.eligible() == 2 && has(w.close(L"NEXT-COMMAND-WILL-START"), L"resolved=0;eligible=2"), "two eligible candidates do not resolve");
}

void emptyStageCommand()
{
    I52CommandEndWindow w;
    expect(has(w.open(L""), L"inconsistent=EMPTY-STAGE-COMMAND") && w.emptyCommand(), "empty C is identity-inconsistent (a)");
    const I52LockDecision d = w.observe(69, true, L"#", kNotLocked, kNotLocked, true);
    expect(d.kind == I52LockClass::Foreign && !d.mark && !d.token, "'#' alone is never eligible");
    expect(w.eligible() == 0 && w.inconsistent() == 1, "empty C: nothing eligible, window inconsistent");
}

void reacquisitionAndNotUnlocked()
{
    I52CommandEndWindow w;
    w.open(C);
    const I52LockDecision re = w.observe(72, true, L"I52CTDA_QUEUED", kWrite, kWrite, false);
    expect(re.kind == I52LockClass::Reacquired && !re.mark && !re.token && has(re.payload, L"reason=STAGE-COMMAND-REACQUIRED"), "C re-acquires the lock (b)");
    const I52LockDecision locked = w.observe(73, true, L"#I52CTDA_QUEUED", kWrite, kWrite, false);
    expect(locked.kind == I52LockClass::NotUnlocked && !locked.mark && !locked.token && has(locked.payload, L"reason=OWN-RELEASE-NOT-UNLOCKED"), "#C not ending unlocked (c)");
    expect(w.inconsistent() == 2 && has(w.close(L"NEXT-COMMAND-WILL-START"), L"resolved=0"), "inconsistent window does not resolve");
}

void wrongDocumentAndCase()
{
    I52CommandEndWindow w;
    w.open(C);
    const I52LockDecision other = w.observe(60, false, L"#I52CTDA_QUEUED", kNotLocked, kNotLocked, true);
    expect(other.kind == I52LockClass::NotScratch && !other.mark && !other.token && !other.record, "own release on another document is not considered");
    const I52LockDecision lower = w.observe(69, true, L"#i52ctda_Queued", kNotLocked, kNotLocked, true);
    expect(lower.kind == I52LockClass::Eligible && lower.mark && lower.token, "ASCII case-insensitive own command is eligible");
    expect(I52AsciiEqualsIgnoreCase(L"#I52CTDA_QUEUED", L"#i52ctda_queued") && !I52AsciiEqualsIgnoreCase(L"#I52CTDA_QUEUED", L"#I52CTDA_QUEUE")
        && !I52AsciiEqualsIgnoreCase(L"É", L"é"), "ASCII-only case folding");
}

// The clean 16N-S re-run (e864a093), transition by transition under V35-A3: one own release binds, five foreign cycles and
// FINISH's acquisition are recorded as foreign; exactly one MARK and one token.
void cleanSixteenNS()
{
    I52CommandEndWindow w;
    w.open(C);
    struct T { uint64_t seq; const wchar_t* global; int current; };
    const std::vector<T> run = { { 69, L"#I52CTDA_QUEUED", 2 }, { 73, L"", 4 }, { 75, L"#", 2 }, { 78, L"", 4 }, { 80, L"#", 2 }, { 85, L"I52CTDA_FINISH", 4 } };
    int marks = 0, tokens = 0;
    for (const T& t : run)
    {
        const I52LockDecision d = w.observe(t.seq, true, t.global, t.current, t.current, t.current == kNotLocked);
        marks += d.mark ? 1 : 0;
        tokens += d.token ? 1 : 0;
    }
    expect(marks == 1 && tokens == 1 && w.boundSequence() == 69, "one MARK and one token, bound to the own release");
    expect(has(w.close(L"NEXT-COMMAND-WILL-START"), L"resolved=1;eligible=1;foreign=5;inconsistent=0;boundSequence=69"), "clean 16N-S window resolves");
}

// m-3: APPCTX-UNLOCK-01-CALL bracket. One transition inside the unlockDocument call marks (with source Sequence and global
// command, not necessarily unlocked for 13A-SM); a second fails closed; none leaves the bracket unresolved; transitions
// before ENTRY or after EXIT (merely inside the APPCTX stage) and on another document are never candidates.
void appctxBracket()
{
    I52AppctxUnlockBracket outside;
    expect(outside.observe(40, true, L"", kWrite, kWrite).kind == I52LockClass::NotScratch, "cycle before the bracket is not a candidate");
    I52AppctxUnlockBracket one;
    expect(has(one.enter(), L"phase=APPCTX-CALL-ENTRY"), "bracket entry record");
    expect(one.observe(41, false, L"", kNotLocked, kNotLocked).kind == I52LockClass::NotScratch, "other document inside the bracket is ignored");
    const I52LockDecision first = one.observe(42, true, L"#I52CTDA_PROBE", kNotLocked, kNotLocked);
    expect(first.mark && first.token && has(first.payload, L"anchor=APPCTX-UNLOCK-01-CALL;sourceSequence=42;sourceGlobalCommand=#I52CTDA_PROBE"), "one transition marks with its source");
    expect(has(one.exit(0), L"transitions=1;boundSequence=42;resolved=1") && one.resolved(), "bracket resolves on one transition");
    expect(one.observe(44, true, L"#", kNotLocked, kNotLocked).kind == I52LockClass::NotScratch, "cycle after the bracket is not a candidate");

    I52AppctxUnlockBracket two;
    two.enter();
    two.observe(50, true, L"#I52CTDA_PROBE", kNotLocked, kNotLocked);
    const I52LockDecision second = two.observe(51, true, L"", kWrite, kWrite);
    expect(second.kind == I52LockClass::Duplicate && !second.mark && !second.token && has(second.payload, L"phase=APPCTX-SECOND;"), "second transition in the bracket is recorded, never marked");
    expect(has(two.exit(0), L"transitions=2;boundSequence=50;resolved=0") && !two.resolved(), "two transitions do not resolve");

    I52AppctxUnlockBracket none;
    none.enter();
    expect(has(none.exit(0), L"transitions=0;boundSequence=0;resolved=0") && !none.resolved(), "no transition: marker absence");

    I52AppctxUnlockBracket sync;
    sync.enter();
    const I52LockDecision locked = sync.observe(60, true, L"#I52CTDA_PROBE", kWrite, kWrite);
    expect(locked.mark && locked.token && has(locked.payload, L"current=4"), "13A-SM: the candidate need not end unlocked");
}
}

int runLockReleaseTests()
{
    eligibleOwnRelease();
    foreignTransitions();
    duplicateOwnRelease();
    emptyStageCommand();
    reacquisitionAndNotUnlocked();
    wrongDocumentAndCase();
    cleanSixteenNS();
    appctxBracket();
    std::printf("%s D-4/m-3 LOCK-RELEASE-BIND-01 COMMAND-END-WINDOW and APPCTX bracket: 8 groups, %d failures\n", lockFailures == 0 ? "PASS" : "FAIL", lockFailures);
    return lockFailures == 0 ? 0 : 1;
}
