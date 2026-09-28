#pragma once

// LOCK-RELEASE-BIND-01, COMMAND-END-WINDOW anchor (V35-A3, amendedBy A3-D4). Pure classification, independent of
// ObjectARX, so the build-machine native tests drive it directly; I52Executor feeds it the recorded RR-DOC transitions.
//
// The stage command C is the global command name that commandWillStart reported for the stage's own activation. The
// window runs from commandEnded of C to strictly before the next commandWillStart (FINISH closes it at the latest). An
// eligible candidate is an unlocked documentLockModeChanged of the scratch document whose global command name is `#` + C
// (ASCII case-insensitive); every other transition is foreign: recorded, never a candidate, never blocking. A window is
// identity-inconsistent when C is empty, when C re-acquires the lock, or when `#` + C does not end unlocked. The native
// module marks only the first eligible candidate (with its source Sequence and global command name) and records every
// further eligible one as a duplicate: exactly one eligible candidate binds, and CONTROL-PLANE-RESULT-01 recomputes it.

#include <cstdint>
#include <string>

enum class I52LockClass { NotScratch, Eligible, Duplicate, Foreign, Reacquired, NotUnlocked };

struct I52LockDecision
{
    I52LockClass kind{};
    bool mark{};      // emit MARK-LOCK-RELEASE with `payload`
    bool token{};     // set TOK-LOCK-RELEASE for the window's activation
    bool record{};    // emit LOCK-RELEASE-BIND-01 with `payload`
    std::wstring payload;
};

bool I52AsciiEqualsIgnoreCase(const std::wstring& a, const std::wstring& b);

class I52CommandEndWindow
{
public:
    // Opens the window for stage command C; returns the LOCK-RELEASE-BIND-01 payload of the WINDOW-OPEN record, and the
    // identity inconsistency (a) when C is empty.
    std::wstring open(const std::wstring& stageCommand);
    bool isOpen() const { return open_; }
    bool emptyCommand() const { return open_ && command_.empty(); }
    // One recorded documentLockModeChanged inside the open window.
    I52LockDecision observe(uint64_t sourceSequence, bool scratchDocument, const std::wstring& globalCommand, int currentMode, int myNewMode, bool unlocked);
    // Payload of the WINDOW-CLOSE record; closes the window.
    std::wstring close(const wchar_t* reason);
    int eligible() const { return eligible_; }
    int foreign() const { return foreign_; }
    int inconsistent() const { return inconsistent_; }
    uint64_t boundSequence() const { return bound_; }

private:
    std::wstring transition(uint64_t sourceSequence, const std::wstring& globalCommand, int currentMode, int myNewMode) const;

    bool open_{};
    std::wstring command_;
    int eligible_{}, foreign_{}, inconsistent_{};
    uint64_t bound_{};
};
