#include <Windows.h>

#include "aced.h"
#include "acdocman.h"
#include "dbapserv.h"
#include "dbmain.h"
#include "rxregsvc.h"
#include "../native/I52CtdaAbi.h"

#include <cwchar>
#include <cwctype>
#include <string>

// R-PAYLOAD-ARX (research only, never deployed). Loaded only with acedArxLoad(NL-ARX-PATH); never unloaded in process.
// At kInitAppMsg it binds LOG-SEQ-01, evaluates PAYLOAD-DB-BINDING and only then queries the C15 arm record; only the
// armed C15 answer registers RR-PAYLOAD-DB, whose callbacks read FINISH-FENCE-01 first and run the C15 origin body
// (CHAIN-SEND) once with RG-DB-MOD semantics.
namespace
{
I52CtdaLogAppendFn logAppend = nullptr;
I52CtdaTokenSetFn tokenSet = nullptr;
I52CtdaQueryC15ArmFn queryArm = nullptr;
I52CtdaFinishFenceIsSetFn fenceIsSet = nullptr;

std::wstring probeId;
AcDbDatabase* boundDatabase = nullptr;
uint64_t triggerObjectOldId = 0;
bool guardArmed = false;
bool guardConsumed = false;
bool inBody = false;
bool reactorRemoved = false;

std::wstring hex(uint64_t value) { wchar_t b[24]{}; std::swprintf(b, _countof(b), L"0x%llX", static_cast<unsigned long long>(value)); return b; }
std::wstring flag(bool value) { return value ? L"1" : L"0"; }

std::wstring environment(const wchar_t* name)
{
    wchar_t buffer[32768]{};
    const DWORD length = GetEnvironmentVariableW(name, buffer, static_cast<DWORD>(_countof(buffer)));
    return length == 0 || length >= _countof(buffer) ? std::wstring() : std::wstring(buffer, length);
}

std::wstring lower(std::wstring value) { for (wchar_t& c : value) c = static_cast<wchar_t>(std::towlower(c)); return value; }

uint64_t record(const wchar_t* stage, uint64_t delivery, const wchar_t* event, const std::wstring& payload)
{
    if (logAppend == nullptr) return 0;
    AcApDocument* document = acDocManager == nullptr ? nullptr : acDocManager->curDocument();
    I52CtdaRecord r{ sizeof(I52CtdaRecord), probeId.c_str(), stage, delivery, L"DRIVER-CMD-01", L"R-PAYLOAD-ARX",
        reinterpret_cast<uint64_t>(document), reinterpret_cast<uint64_t>(boundDatabase), I52CTDA_COMMAND_CURRENT, event, payload.c_str() };
    return logAppend(&r);
}

class PayloadDatabaseReactor final : public AcDbDatabaseReactor
{
public:
    void objectModified(const AcDbDatabase*, const AcDbObject* object) override { deliver(L"N-DB-MOD", object); }
    void objectAppended(const AcDbDatabase*, const AcDbObject* object) override { deliver(L"N-DB-APPEND", object); }
    void objectErased(const AcDbDatabase*, const AcDbObject* object, bool) override { deliver(L"N-DB-ERASE", object); }
    void objectOpenedForModify(const AcDbDatabase*, const AcDbObject* object) override { deliver(L"N-DB-OPEN", object); }

private:
    void deliver(const wchar_t* event, const AcDbObject* object)
    {
        // FINISH-FENCE-01 first: after the fence only one LATE-DELIVERY record, without touching the notifier.
        if (fenceIsSet != nullptr && fenceIsSet() != 0)
        {
            record(L"STG-ORIGIN", I52CTDA_DELIVERY_NEW, L"FINISH-FENCE-01", L"kind=LATE-DELIVERY;notifierAccess=NONE;registration=RR-PAYLOAD-DB;instance=" + hex(reinterpret_cast<uint64_t>(this))
                + L";removedInstance=" + flag(reactorRemoved) + L";event=" + event);
            return;
        }
        const uint64_t id = object == nullptr ? 0 : static_cast<uint64_t>(object->objectId().asOldId());
        record(I52CTDA_STAGE_CURRENT, I52CTDA_DELIVERY_CURRENT, event, L"registration=RR-PAYLOAD-DB;instance=" + hex(reinterpret_cast<uint64_t>(this)) + L";object=" + hex(id));
        const bool body = std::wcscmp(event, L"N-DB-MOD") == 0 && id == triggerObjectOldId;
        if (!body || !guardArmed) return;
        if (inBody) { record(I52CTDA_STAGE_CURRENT, I52CTDA_DELIVERY_CURRENT, L"RG-DB-MOD", L"phase=RECURSION;owner=R-PAYLOAD-ARX"); return; }
        if (guardConsumed) { record(I52CTDA_STAGE_CURRENT, I52CTDA_DELIVERY_CURRENT, L"RG-DB-MOD", L"phase=OBSERVE;later=1;owner=R-PAYLOAD-ARX"); return; }
        guardConsumed = true;
        inBody = true;
        // C15 origin body (CHAIN-SEND) in STG-ORIGIN: one NS-SEND with the retained data, then MARK-ORIGIN-RETURN.
        record(L"STG-ORIGIN", I52CTDA_DELIVERY_NEW, L"EP-CALLBACK", L"event=N-DB-MOD;registration=RR-PAYLOAD-DB;instance=" + hex(reinterpret_cast<uint64_t>(this)) + L";object=" + hex(id));
        record(L"STG-ORIGIN", I52CTDA_DELIVERY_CURRENT, L"RG-DB-MOD", L"phase=CONSUME;owner=R-PAYLOAD-ARX");
        record(L"STG-ORIGIN", I52CTDA_DELIVERY_CURRENT, L"CTX-CALLBACK", L"event=N-DB-MOD;owner=R-PAYLOAD-ARX");
        AcApDocument* document = acDocManager->curDocument();
        record(L"STG-ORIGIN", I52CTDA_DELIVERY_CURRENT, L"SP-SEND", L"retainedSlot=1");
        const Acad::ErrorStatus status = acDocManager->sendStringToExecute(document, L"I52CTDA_QUEUED ", false, false, false);
        record(L"STG-ORIGIN", I52CTDA_DELIVERY_CURRENT, L"NS-SEND", L"status=" + std::to_wstring(static_cast<int>(status)) + L";point=SP-SEND;command=I52CTDA_QUEUED");
        if (status != Acad::eOk) record(L"STG-ORIGIN", I52CTDA_DELIVERY_CURRENT, L"UNK-REJECTION", L"scheduler=NS-SEND;status=" + std::to_wstring(static_cast<int>(status)));
        if (tokenSet != nullptr) tokenSet(L"TOK-ENQUEUE-SEND", I52CTDA_DELIVERY_CURRENT);
        record(L"STG-ORIGIN", I52CTDA_DELIVERY_CURRENT, L"MARK-ORIGIN-RETURN", L"event=N-DB-MOD;owner=R-PAYLOAD-ARX");
        inBody = false;
    }
};

PayloadDatabaseReactor* reactor = nullptr;

template <typename T> bool bind(HMODULE module, const char* name, T& target)
{
    target = reinterpret_cast<T>(GetProcAddress(module, name));
    return target != nullptr;
}

void initialize()
{
    probeId = environment(L"I52_CTDA_PROBE_ID");
    HMODULE native = GetModuleHandleW(L"I52CtdaNative.arx");
    const bool bound = native != nullptr && bind(native, "I52Ctda_LogAppend", logAppend) && bind(native, "I52Ctda_TokenSet", tokenSet)
        && bind(native, "I52Ctda_QueryC15Arm", queryArm) && bind(native, "I52Ctda_FinishFenceIsSet", fenceIsSet);
    if (!bound) { logAppend = nullptr; return; }  // cannot bind LOG-SEQ-01: records nothing (UNK-LOG-BINDING)

    AcDbDatabase* working = acdbHostApplicationServices() == nullptr ? nullptr : acdbHostApplicationServices()->workingDatabase();
    AcApDocument* current = acDocManager == nullptr ? nullptr : acDocManager->curDocument();
    const std::wstring scratch = lower(environment(L"I52_CTDA_SCRATCH_DWG"));
    const std::wstring file = current == nullptr || current->fileName() == nullptr ? std::wstring() : lower(current->fileName());
    boundDatabase = working;
    record(L"STG-PAYLOAD-INIT", I52CTDA_DELIVERY_CURRENT, L"R-PAYLOAD-ARX", L"phase=INIT;logSeqBound=1;fenceIsSet=" + std::to_wstring(fenceIsSet()));

    // PAYLOAD-DB-BINDING: working database == scratch database of the exact scratch document; otherwise nothing registers.
    const bool binding = working != nullptr && current != nullptr && current->database() == working && !scratch.empty() && file == scratch;
    record(L"STG-PAYLOAD-INIT", I52CTDA_DELIVERY_CURRENT, L"PAYLOAD-DB-BINDING", L"holds=" + flag(binding) + L";workingDatabase=" + hex(reinterpret_cast<uint64_t>(working))
        + L";curDocument=" + hex(reinterpret_cast<uint64_t>(current)) + L";documentMatchesScratch=" + flag(!scratch.empty() && file == scratch));
    if (!binding) { record(L"STG-PAYLOAD-INIT", I52CTDA_DELIVERY_CURRENT, L"UNK-PAYLOAD-DB", L"step=PAYLOAD-DB-BINDING"); return; }

    I52CtdaC15Arm arm{};
    arm.size = sizeof(I52CtdaC15Arm);
    arm.databaseId = reinterpret_cast<uint64_t>(working);
    queryArm(&arm);
    record(L"STG-PAYLOAD-INIT", I52CTDA_DELIVERY_CURRENT, L"R-PAYLOAD-ARX", L"phase=C15-ARM;armed=" + std::to_wstring(arm.armed) + L";databaseMatches=" + std::to_wstring(arm.databaseMatches));
    if (arm.armed != 1) return;  // unarmed rows register no reactor
    if (arm.databaseMatches != 1) { record(L"STG-PAYLOAD-INIT", I52CTDA_DELIVERY_CURRENT, L"UNK-PAYLOAD-DB", L"step=QUERY-C15-ARM"); return; }

    triggerObjectOldId = arm.triggerObjectOldId;
    reactor = new PayloadDatabaseReactor();
    const Acad::ErrorStatus status = working->addReactor(reactor);
    guardArmed = status == Acad::eOk;
    record(L"STG-PAYLOAD-INIT", I52CTDA_DELIVERY_CURRENT, L"MARK-REGISTRATION", L"event=N-DB-MOD;object=" + hex(triggerObjectOldId) + L";database=" + hex(reinterpret_cast<uint64_t>(working))
        + L";instance=" + hex(reinterpret_cast<uint64_t>(reactor)) + L";status=" + std::to_wstring(static_cast<int>(status)));
    record(L"STG-PAYLOAD-INIT", I52CTDA_DELIVERY_CURRENT, L"RG-DB-MOD", L"phase=ARM;family=N-DB-MOD;owner=R-PAYLOAD-ARX;target=" + hex(triggerObjectOldId));
}
}

// Removal export bound by R-NATIVE-ARX in CLN-BASE: removes RR-PAYLOAD-DB while the database is live.
extern "C" __declspec(dllexport) int32_t I52CtdaPayload_RemoveReactor()
{
    if (reactor == nullptr || boundDatabase == nullptr) return I52CTDA_PAYLOAD_NOTHING_REGISTERED;
    if (reactorRemoved) return 0;
    const Acad::ErrorStatus status = boundDatabase->removeReactor(reactor);
    reactorRemoved = status == Acad::eOk;
    guardArmed = false;
    record(I52CTDA_STAGE_CURRENT, I52CTDA_DELIVERY_CURRENT, L"RG-DB-MOD", L"phase=DISARM;family=N-DB-MOD;owner=R-PAYLOAD-ARX;at=CLN-BASE");
    record(I52CTDA_STAGE_CURRENT, I52CTDA_DELIVERY_CURRENT, L"RR-PAYLOAD-DB", L"phase=REMOVE;status=" + std::to_wstring(static_cast<int>(status)) + L";instance=" + hex(reinterpret_cast<uint64_t>(reactor)));
    return static_cast<int32_t>(status);  // the instance stays allocated until process exit
}

extern "C" AcRx::AppRetCode acrxEntryPoint(AcRx::AppMsgCode message, void* packet)
{
    switch (message)
    {
    case AcRx::kInitAppMsg:
        // Not unlocked: no in-process unload; process exit is the residency fence.
        acrxRegisterAppMDIAware(packet);
        initialize();
        break;
    default:
        break;
    }
    return AcRx::kRetOK;
}
