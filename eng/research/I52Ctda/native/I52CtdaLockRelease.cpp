#include "I52CtdaLockRelease.h"

bool I52AsciiEqualsIgnoreCase(const std::wstring& a, const std::wstring& b)
{
    if (a.size() != b.size()) return false;
    for (size_t i = 0; i < a.size(); ++i)
    {
        wchar_t x = a[i], y = b[i];
        if (x >= L'a' && x <= L'z') x = static_cast<wchar_t>(x - L'a' + L'A');
        if (y >= L'a' && y <= L'z') y = static_cast<wchar_t>(y - L'a' + L'A');
        if (x != y) return false;
    }
    return true;
}

std::wstring I52CommandEndWindow::open(const std::wstring& stageCommand)
{
    open_ = true;
    command_ = stageCommand;
    eligible_ = foreign_ = 0;
    inconsistent_ = stageCommand.empty() ? 1 : 0;
    bound_ = 0;
    return L"phase=WINDOW-OPEN;anchor=COMMAND-END-WINDOW;stageCommand=" + command_ + (command_.empty() ? L";inconsistent=EMPTY-STAGE-COMMAND" : L"");
}

std::wstring I52CommandEndWindow::transition(uint64_t sourceSequence, const std::wstring& globalCommand, int currentMode, int myNewMode) const
{
    return L"sourceSequence=" + std::to_wstring(sourceSequence) + L";sourceGlobalCommand=" + globalCommand + L";stageCommand=" + command_
        + L";current=" + std::to_wstring(currentMode) + L";myNew=" + std::to_wstring(myNewMode);
}

I52LockDecision I52CommandEndWindow::observe(uint64_t sourceSequence, bool scratchDocument, const std::wstring& globalCommand, int currentMode, int myNewMode, bool unlocked)
{
    I52LockDecision d{};
    if (!open_ || !scratchDocument) { d.kind = I52LockClass::NotScratch; return d; }
    const std::wstring raw = transition(sourceSequence, globalCommand, currentMode, myNewMode);
    const bool own = !command_.empty() && I52AsciiEqualsIgnoreCase(globalCommand, L"#" + command_);
    if (own && unlocked)
    {
        ++eligible_;
        if (eligible_ == 1)
        {
            bound_ = sourceSequence;
            d.kind = I52LockClass::Eligible; d.mark = true; d.token = true;
            d.payload = L"anchor=COMMAND-END-WINDOW;" + raw;
            return d;
        }
        d.kind = I52LockClass::Duplicate; d.record = true;
        d.payload = L"phase=ELIGIBLE-DUPLICATE;anchor=COMMAND-END-WINDOW;" + raw;
        return d;
    }
    if (own)
    {
        ++inconsistent_;
        d.kind = I52LockClass::NotUnlocked; d.record = true;
        d.payload = L"phase=INCONSISTENT;reason=OWN-RELEASE-NOT-UNLOCKED;anchor=COMMAND-END-WINDOW;" + raw;
        return d;
    }
    if (!command_.empty() && I52AsciiEqualsIgnoreCase(globalCommand, command_))
    {
        ++inconsistent_;
        d.kind = I52LockClass::Reacquired; d.record = true;
        d.payload = L"phase=INCONSISTENT;reason=STAGE-COMMAND-REACQUIRED;anchor=COMMAND-END-WINDOW;" + raw;
        return d;
    }
    ++foreign_;
    d.kind = I52LockClass::Foreign; d.record = true;
    d.payload = L"phase=FOREIGN;anchor=COMMAND-END-WINDOW;" + raw;
    return d;
}

std::wstring I52AppctxUnlockBracket::enter()
{
    active_ = true;
    transitions_ = 0;
    bound_ = 0;
    return L"phase=APPCTX-CALL-ENTRY;anchor=APPCTX-UNLOCK-01-CALL";
}

I52LockDecision I52AppctxUnlockBracket::observe(uint64_t sourceSequence, bool scratchDocument, const std::wstring& globalCommand, int currentMode, int myNewMode)
{
    I52LockDecision d{};
    if (!active_ || !scratchDocument) { d.kind = I52LockClass::NotScratch; return d; }
    const std::wstring raw = L"sourceSequence=" + std::to_wstring(sourceSequence) + L";sourceGlobalCommand=" + globalCommand
        + L";current=" + std::to_wstring(currentMode) + L";myNew=" + std::to_wstring(myNewMode);
    if (++transitions_ == 1)
    {
        bound_ = sourceSequence;
        d.kind = I52LockClass::Eligible; d.mark = true; d.token = true;
        d.payload = L"anchor=APPCTX-UNLOCK-01-CALL;" + raw;
        return d;
    }
    d.kind = I52LockClass::Duplicate; d.record = true;
    d.payload = L"phase=APPCTX-SECOND;anchor=APPCTX-UNLOCK-01-CALL;" + raw;
    return d;
}

std::wstring I52AppctxUnlockBracket::exit(int status)
{
    active_ = false;
    return L"phase=APPCTX-CALL-EXIT;anchor=APPCTX-UNLOCK-01-CALL;status=" + std::to_wstring(status) + L";transitions=" + std::to_wstring(transitions_)
        + L";boundSequence=" + std::to_wstring(bound_) + L";resolved=" + (transitions_ == 1 ? L"1" : L"0");
}

std::wstring I52CommandEndWindow::close(const wchar_t* reason)
{
    open_ = false;
    return std::wstring(L"phase=WINDOW-CLOSE;reason=") + reason + L";resolved=" + (eligible_ == 1 && inconsistent_ == 0 ? L"1" : L"0")
        + L";eligible=" + std::to_wstring(eligible_) + L";foreign=" + std::to_wstring(foreign_) + L";inconsistent=" + std::to_wstring(inconsistent_)
        + L";boundSequence=" + std::to_wstring(bound_) + L";stageCommand=" + command_;
}
