#include <Windows.h>

#include "I52CtdaExecutor.h"
#include "I52CtdaAuthority.h"

#include "aced.h"
#include "acedads.h"
#include "actrans.h"
#include "adscodes.h"
#include "AcCoreDefs.h"
#include "dbents.h"
#include "dbobjptr.h"
#include "dbtrans.h"
#include "dbxrecrd.h"
#include "rxdlinkr.h"

#include <bcrypt.h>

#include <cwchar>
#include <cwctype>
#include <fstream>
#include <sstream>

// FIN-GATE-01 idle hook (core_rxmfcapi.h, exact AcCore declarations; that header needs MFC, so only these two
// members are declared here with the same signatures and linkage).
typedef void (*AcedOnIdleMsgFn)();
ACCORE_PORT bool acedRegisterOnIdleWinMsg(const AcedOnIdleMsgFn pfn);
ACCORE_PORT bool acedRemoveOnIdleWinMsg(const AcedOnIdleMsgFn pfn);

namespace
{
std::wstring status(Acad::ErrorStatus s) { return std::to_wstring(static_cast<int>(s)); }
std::wstring ptr(const void* p) { return I52Hex(reinterpret_cast<uint64_t>(p)); }
std::wstring oid(const AcDbObjectId& id) { return I52Hex(static_cast<uint64_t>(id.asOldId())); }
std::wstring flag(bool b) { return b ? L"1" : L"0"; }
std::wstring lower(std::wstring value) { for (wchar_t& c : value) c = static_cast<wchar_t>(std::towlower(c)); return value; }

std::wstring environment(const wchar_t* name)
{
    wchar_t buffer[32768]{};
    const DWORD length = GetEnvironmentVariableW(name, buffer, static_cast<DWORD>(_countof(buffer)));
    if (length == 0 || length >= _countof(buffer)) return {};
    return std::wstring(buffer, length);
}

// Directory of this module (<RunDir>): NL-ARX-PATH is <RunDir>\I52CtdaPayload.arx.
std::wstring runDirectory()
{
    HMODULE self = nullptr;
    GetModuleHandleExW(GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS | GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT, reinterpret_cast<LPCWSTR>(&runDirectory), &self);
    wchar_t path[MAX_PATH * 4]{};
    const DWORD length = GetModuleFileNameW(self, path, static_cast<DWORD>(_countof(path)));
    std::wstring value(path, length);
    const size_t slash = value.find_last_of(L"\\/");
    return slash == std::wstring::npos ? std::wstring() : value.substr(0, slash);
}

std::wstring sha256File(const std::wstring& path)
{
    std::ifstream stream(path, std::ios::binary);
    if (!stream) return {};
    std::string bytes((std::istreambuf_iterator<char>(stream)), std::istreambuf_iterator<char>());
    BCRYPT_ALG_HANDLE algorithm = nullptr;
    if (BCryptOpenAlgorithmProvider(&algorithm, BCRYPT_SHA256_ALGORITHM, nullptr, 0) != 0) return {};
    unsigned char digest[32]{};
    const NTSTATUS hashed = BCryptHash(algorithm, nullptr, 0, reinterpret_cast<PUCHAR>(bytes.data()), static_cast<ULONG>(bytes.size()), digest, sizeof(digest));
    BCryptCloseAlgorithmProvider(algorithm, 0);
    if (hashed != 0) return {};
    std::wstring hex;
    wchar_t buffer[3]{};
    for (unsigned char b : digest) { std::swprintf(buffer, 3, L"%02X", b); hex += buffer; }
    return hex;
}

bool moduleLoaded(const std::wstring& path)
{
    resbuf* apps = acedArxLoaded();
    bool found = false;
    const std::wstring name = lower(path.substr(path.find_last_of(L"\\/") + 1));
    for (resbuf* item = apps; item != nullptr; item = item->rbnext)
        if (item->restype == RTSTR && item->resval.rstring != nullptr && lower(item->resval.rstring).find(name.substr(0, name.find_last_of(L'.'))) != std::wstring::npos) found = true;
    if (apps != nullptr) acutRelRb(apps);
    return found;
}

std::wstring smPayload(const I52SmFacts& f)
{
    return L"keyPresent=" + flag(f.link.keyPresent) + L";identityMatches=" + flag(f.link.identityMatches) + L";erased=" + flag(f.link.erased)
        + L";isXrecord=" + flag(f.link.isXrecord) + L";resbufCount=" + std::to_wstring(f.link.resbufCount) + L";isText=" + flag(f.link.isText)
        + L";carriersFound=" + std::to_wstring(f.link.carriersFound) + L";value=" + I52Narrow(f.link.value) + L";anchorALive=" + flag(f.anchorALive)
        + L";bRead=" + flag(f.bRead) + L";bLive=" + flag(f.bLive) + L";foreignReferenceInserts=" + std::to_wstring(f.foreignReferenceInserts)
        + L";cBound=" + flag(f.cBound) + L";cFound=" + flag(f.cFound) + L";cLive=" + flag(f.cLive) + L";cAtCreationPosition=" + flag(f.cAtCreationPosition);
}

std::wstring materialPayload(const I52MaterialFacts& m)
{
    return L"live=" + flag(m.live) + L";x=" + std::to_wstring(m.x) + L";y=" + std::to_wstring(m.y) + L";z=" + std::to_wstring(m.z) + L";layer=" + I52Narrow(m.layer);
}

void idleThunk() { I52Executor::instance().onIdle(); }
void CALLBACK timerThunk(HWND, UINT, UINT_PTR, DWORD) {}  // keeps idle notifications arriving (FIN-GATE-01 mechanism)

std::string utf8(const std::wstring& value)
{
    if (value.empty()) return {};
    const int size = WideCharToMultiByte(CP_UTF8, 0, value.c_str(), static_cast<int>(value.size()), nullptr, 0, nullptr, nullptr);
    std::string result(static_cast<size_t>(size), '\0');
    WideCharToMultiByte(CP_UTF8, 0, value.c_str(), static_cast<int>(value.size()), result.data(), size, nullptr, nullptr);
    return result;
}

std::string jsonEscape(const std::string& value)
{
    std::string out;
    for (const unsigned char ch : value)
    {
        if (ch == '\\' || ch == '"') { out.push_back('\\'); out.push_back(static_cast<char>(ch)); }
        else if (ch == '\n') out += "\\n";
        else if (ch < 0x20) { char b[8]; std::snprintf(b, sizeof(b), "\\u%04x", ch); out += b; }
        else out.push_back(static_cast<char>(ch));
    }
    return out;
}
}

void I52Executor::load()
{
    loaded_ = true;
    I52Log::instance().configure();
    I52Log::instance().setStageProvider([](I52Id& stage, uint64_t& delivery) { stage = instance().currentStage(); delivery = instance().currentDelivery(); });
}

void I52Executor::unload()
{
    if (gateRegistered_ && !finishIssued_) { acedRemoveOnIdleWinMsg(idleThunk); if (gateTimer_ != 0) KillTimer(nullptr, gateTimer_); }
    for (Registered& r : registered_) removeObserver(r);
}

// ------------------------------------------------------------------ BOOT-01 (nothing is logged before I52CTDA_PROBE)
void I52Executor::boot()
{
    document_ = acDocManager == nullptr ? nullptr : acDocManager->curDocument();
    database_ = document_ == nullptr ? nullptr : document_->database();
    std::wostringstream facts;
    if (booted_) { bootFailed_ = true; bootFacts_ = L"status=DUPLICATE-BOOT"; return; }
    booted_ = true;
    AcTransactionManager* tm = document_ == nullptr ? nullptr : document_->transactionManager();
    AcTransaction* t = tm == nullptr ? nullptr : tm->startTransaction();
    Acad::ErrorStatus materialized = Acad::eNullPtr, closed = Acad::eNullPtr;
    I52FixtureIds ids{};
    if (t != nullptr)
    {
        materialized = I52CtdaFixture::materialize(database_, tm, ids);
        closed = materialized == Acad::eOk ? tm->endTransaction() : tm->abortTransaction();
    }
    I52FixtureResolution resolution{};
    if (materialized == Acad::eOk && closed == Acad::eOk)
    {
        fixture_.bind(ids);
        resolution = fixture_.resolve();
    }
    bootFailed_ = !(t != nullptr && materialized == Acad::eOk && closed == Acad::eOk && resolution.resolved == I52CtdaFixture::kDeclaredIdentities && resolution.bindingResolved);
    I52Log::instance().setScratch(reinterpret_cast<uint64_t>(document_), reinterpret_cast<uint64_t>(database_));
    facts << L"status=" << (bootFailed_ ? L"FAILED" : L"OK") << L";transaction=" << ptr(t) << L";materialize=" << static_cast<int>(materialized)
          << L";transactionClose=" << static_cast<int>(closed) << L";declared=" << I52CtdaFixture::kDeclaredIdentities << L";resolved=" << resolution.resolved
          << L";smLinkBinding=" << (resolution.bindingResolved ? L"RESOLVED" : L"MISMATCH") << L";document=" << ptr(document_) << L";database=" << ptr(database_)
          << L";observerRegistrations=0";
    bootFacts_ = facts.str();
    bootSnapshot_.assign(resolution.snapshot.begin(), resolution.snapshot.end());
    acutPrintf(bootFailed_ ? L"\nI52 CT-DA BOOT-01 failed.\n" : L"\nI52 CT-DA BOOT-01 complete.\n");
}

// ------------------------------------------------------------------ CMD-PROBE / DRIVER-CMD-01 / DRIVER-APP-01
void I52Executor::probe()
{
    wchar_t argument[128]{};
    if (acedGetString(0, L"\nProbeId: ", argument) != RTNORM) argument[0] = L'\0';
    I52Log& log = I52Log::instance();
    log.configure();
    const I52RowPlan* plan = I52Plan::find(argument);
    if (!log.ready() || plan == nullptr || log.probeId() != argument || plan_ != nullptr)
    {
        // Unknown or mismatched ProbeId: rejected before any governed dispatch (the control plane rejects it before launch).
        acutPrintf(L"\nI52 CT-DA: ProbeId '%ls' rejected (plan=%ls, logger=%ls).\n", argument, plan == nullptr ? L"none" : L"found", log.ready() ? L"ready" : L"unavailable");
        return;
    }
    plan_ = plan;
    log.setPlan(plan_);
    recording_ = true;
    commands_.push_back(L"I52CTDA_PROBE");
    log.setCommandIdentity(L"I52CTDA_PROBE");
    push(I52Id::STG_PROBE_CMD);
    fact(I52Id::RUN_ENV_01, L"pid=" + std::to_wstring(GetCurrentProcessId()) + L";runDirectory=" + runDirectory() + L";eventLog=" + log.logPath()
        + L";probeEnvironment=" + log.probeId() + L";payloadSha256Expected=" + environment(L"I52_CTDA_PAYLOAD_SHA256"));
    fact(I52Id::BOOT_01, bootFacts_.empty() ? L"status=NOT-RUN" : bootFacts_);
    fact(I52Id::CMD_PROBE, L"phase=ENTRY;probe=" + std::wstring(argument) + L";rowApprovalHash=" + plan_->rowApprovalHash
        + L";freezePackageHash=43DCE809AA5B124E73B67E2B8B76EB78DC921FCE0BE961B2906BC21BE9D3B6DF");
    if (!booted_ || bootFailed_) { aborted_ = true; abortReason_ = L"BOOT-01"; fact(I52Id::UNKNOWN_COMMON, L"step=BOOT-01"); }

    // DRIVER-CMD-01 entry: a write-capable host command lock must be observed, otherwise nothing is executed.
    const int mode = document_ == nullptr ? -1 : static_cast<int>(document_->lockMode(true));
    const bool writable = mode == AcAp::kWrite || mode == AcAp::kAutoWrite || mode == AcAp::kProtectedAutoWrite;
    recordPhase(L"ENTRY", L"lockMode=" + std::to_wstring(mode) + L";writeCapable=" + flag(writable));
    if (!writable && !aborted_) { aborted_ = true; abortReason_ = L"HOST-COMMAND-LOCK"; fact(I52Id::UNK_NO_WRITE_LOCK, L"step=DRIVER-CMD-01 entry"); }

    const std::vector<std::wstring> phases = I52Auth::phases(plan_->driver);
    if (plan_->driver == I52Id::DRIVER_CMD_01)
    {
        for (const std::wstring& phase : phases) runDriverPhase(phase);
    }
    else
    {
        // DRIVER-APP-01: I52CTDA_PROBE registers observers, runs setup and enqueues the driver delivery once.
        runDriverPhase(L"REGISTER");
        runDriverPhase(L"SETUP");
        for (const std::wstring& phase : phases) { if (phase == L"DELIVERY") break; runDriverPhase(phase); }
    }
    fact(I52Id::CMD_PROBE, L"phase=RETURN;aborted=" + flag(aborted_) + (aborted_ ? L";reason=" + abortReason_ : L""));
    if (rowHasToken(I52Id::TOK_PROBE_RETURN)) I52Ctda_TokenSetNative(I52Id::TOK_PROBE_RETURN, log.current(I52Id::STG_PROBE_CMD));
    probeReturnTick_ = GetTickCount64();
    pop(I52Id::STG_PROBE_CMD);
    commands_.pop_back();
    log.setCommandIdentity(commands_.empty() ? L"NONE" : commands_.back());
}

void I52Executor::runDriverPhase(const std::wstring& phase)
{
    const bool ok = !aborted_;
    if (phase == L"REGISTER")
    {
        recordPhase(L"REGISTER");
        if (ok) for (size_t i = 0; i < plan_->registrationCount; ++i)
            if (!registerObserver(plan_->registrations[i])) { aborted_ = true; abortReason_ = L"REGISTER"; break; }
        registerFinishGate();  // always, so the row reaches CMD-FINISH (DRAIN on failure)
    }
    else if (phase == L"SETUP") { recordPhase(L"SETUP"); if (ok && !runSetup()) { aborted_ = true; abortReason_ = L"SETUP"; } }
    else if (phase == L"ARM") { recordPhase(L"ARM"); if (ok) armGuards(false); }
    else if (phase == L"TRIGGER") { recordPhase(L"TRIGGER"); if (ok && !runTrigger()) { aborted_ = true; abortReason_ = L"TRIGGER"; } }
    else if (phase == L"CALLBACK-WINDOW") recordPhase(L"CALLBACK-WINDOW", L"synchronousCallbacksReturned=1");
    else if (phase == L"POST-TRIGGER-OBLIGATIONS") postTriggerObligations();
    else if (phase == L"OUTCOME-RECORD")
        recordPhase(L"OUTCOME-RECORD", L"outcomeCommitted=" + flag(outcomeCommitted_) + L";aborted=" + flag(aborted_) + L";primary=" + ptr(primary_) + L";nested=" + ptr(nested_) + L";controlled=" + ptr(controlled_));
    else if (phase == L"DISARM") disarmGuards(L"DISARM");
    else if (phase == L"TOKEN")
    {
        recordPhase(L"TOKEN");
        if (rowHasToken(I52Id::TOK_OUTCOME) && primary_ == nullptr && nested_ == nullptr && controlled_ == nullptr)
            I52Ctda_TokenSetNative(I52Id::TOK_OUTCOME, I52Log::instance().current(I52Id::STG_PROBE_CMD));
        if (plan_->driver == I52Id::DRIVER_APP_01 && rowHasToken(I52Id::TOK_DRIVER_APP_DONE))
            I52Ctda_TokenSetNative(I52Id::TOK_DRIVER_APP_DONE, I52Log::instance().current(I52Id::STG_DRIVER_APP));
    }
    else if (phase == L"ENQUEUE")
    {
        if (!ok) { recordPhase(L"ENQUEUE", L"skipped=ABORTED"); return; }
        const Acad::ErrorStatus s = acDocManager->beginExecuteInApplicationContext(&I52Executor::driverApp, nullptr);
        recordPhase(L"ENQUEUE", L"status=" + status(s) + L";schedulerUse=DRIVER-INFRA;credit=NONE");
        if (s != Acad::eOk) { aborted_ = true; abortReason_ = L"DRIVER-APP-ENQUEUE"; fact(I52Id::UNK_REJECTION, L"scheduler=DRIVER-INFRA;status=" + status(s)); }
        else ++pendingInfra_;
    }
    else if (phase == L"DELIVERY") recordPhase(L"DELIVERY");
    else fact(I52Id::UNKNOWN_COMMON, L"unimplementedPhase=" + phase);
}

void I52Executor::driverApp(void*)
{
    I52Executor& x = instance();
    const uint64_t delivery = x.push(I52Id::STG_DRIVER_APP);
    --x.pendingInfra_;
    x.fact(I52Id::MARK_DRIVER_APP_ENTRY, L"isApplicationContext=" + flag(acDocManager->isApplicationContext()) + L";" + x.context());
    if (!acDocManager->isApplicationContext() && !x.aborted_) { x.aborted_ = true; x.abortReason_ = L"DRIVER-APP-CONTEXT"; x.fact(I52Id::UNK_INVALID_APP_CONTEXT, L"stage=STG-DRIVER-APP"); }
    if (acDocManager->curDocument() != x.document_ && !x.aborted_) { x.aborted_ = true; x.abortReason_ = L"DRIVER-APP-DOCUMENT"; x.fact(I52Id::UNK_ILLEGAL_DB_CONTEXT, L"stage=STG-DRIVER-APP"); }
    bool started = false;
    for (const std::wstring& phase : I52Auth::phases(x.plan_->driver))
    {
        if (phase == L"DELIVERY") started = true;
        if (started) x.runDriverPhase(phase);
    }
    x.fact(I52Id::MARK_DRIVER_APP_RETURN, L"aborted=" + flag(x.aborted_));
    (void)delivery;
    x.pop(I52Id::STG_DRIVER_APP);
}

bool I52Executor::registerFinishGate()
{
    if (gateRegistered_) return true;
    const bool hooked = acedRegisterOnIdleWinMsg(idleThunk);
    gateTimer_ = SetTimer(nullptr, 0, 1000, timerThunk);
    gateDelivery_ = I52Log::instance().open(I52Id::STG_FIN_GATE);
    gateRegistered_ = hooked;
    fact(I52Id::FIN_GATE_01, L"phase=REGISTERED;idleHook=" + flag(hooked) + L";timer=" + std::to_wstring(gateTimer_) + L";timeoutSeconds=" + std::to_wstring(I52Auth::finishGateTimeoutSeconds())
        + L";schedulerUse=FINISH-INFRA", I52Id::STG_FIN_GATE, gateDelivery_);
    return hooked;
}

// ------------------------------------------------------------------ SETUP
bool I52Executor::runSetup()
{
    int last = -1;
    for (size_t i = 0; i < plan_->setupCount; ++i)
    {
        const int order = I52Auth::setupOrderIndex(plan_->setup[i]);
        if (order <= last) { fact(I52Id::UNKNOWN_COMMON, L"setupOrder=VIOLATED;action=" + std::wstring(I52Plan::text(plan_->setup[i]))); return false; }
        last = order;
        if (!runSetupAction(plan_->setup[i])) return false;
    }
    return true;
}

bool I52Executor::runSetupAction(I52Id setup)
{
    AcTransactionManager* tm = document_ == nullptr ? nullptr : document_->transactionManager();
    switch (setup)
    {
    case I52Id::SET_OPEN_PRIMARY_T:
    {
        fact(I52Id::SA_TX_RESOLVE, L"manager=" + ptr(tm) + L";status=" + flag(tm != nullptr));
        if (tm == nullptr) return false;
        primary_ = tm->startTransaction();
        if (primary_ != nullptr) ++ownedTransactions_;
        const int count = tm->numActiveTransactions();
        fact(I52Id::SA_TX_START, L"transaction=" + ptr(primary_) + L";owner=T-PRIMARY;activeTransactions=" + std::to_wstring(count));
        fact(setup, L"status=" + flag(primary_ != nullptr && count == 1) + L";activeTransactions=" + std::to_wstring(count));
        return primary_ != nullptr && count == 1;
    }
    case I52Id::SET_OPEN_NESTED_T:
    {
        nested_ = tm == nullptr ? nullptr : tm->startTransaction();
        if (nested_ != nullptr) ++ownedTransactions_;
        const int count = tm == nullptr ? -1 : tm->numActiveTransactions();
        fact(I52Id::SA_TX_START, L"transaction=" + ptr(nested_) + L";owner=T-NESTED;activeTransactions=" + std::to_wstring(count));
        fact(setup, L"status=" + flag(nested_ != nullptr && count == 2) + L";activeTransactions=" + std::to_wstring(count));
        return nested_ != nullptr && count == 2;
    }
    case I52Id::SET_STAGE_S_IN_PRIMARY:
    {
        AcDbTransactionManager* dbtm = database_->transactionManager();
        const bool topIsPrimary = primary_ != nullptr && dbtm->topTransaction() == primary_;
        const Acad::ErrorStatus w = topIsPrimary ? I52CtdaFixture::writeXrecordText(dbtm, fixture_.ids().semanticXrecord, L"HFV30:S:1") : Acad::eNoActiveTransactions;
        std::string readBack;
        const Acad::ErrorStatus r = w == Acad::eOk ? I52CtdaFixture::readXrecordText(dbtm, fixture_.ids().semanticXrecord, readBack) : w;
        const bool staged = w == Acad::eOk && r == Acad::eOk && readBack == "HFV30:S:1";
        fact(setup, L"write=" + status(w) + L";read=" + status(r) + L";readBack=" + I52Narrow(readBack) + L";staged=" + flag(staged) + L";topIsPrimary=" + flag(topIsPrimary));
        if (!staged) fact(I52Id::UNK_STAGING, L"write=" + status(w) + L";read=" + status(r));
        return staged;
    }
    case I52Id::SET_OUTCOME_COMMIT:
        fact(setup, L"probeOutcomeMode=COMMIT;fixedBeforeTrigger=1");
        return true;
    case I52Id::SET_RETAIN_CALLBACK_DATA:
        retained_[0] = { 1, I52Id::None, false };
        fact(setup, L"slot=1;probe=" + I52Log::instance().probeId() + L";mutation=" + I52Plan::text(plan_->mutation) + L";sendCommand=I52CTDA_QUEUED");
        return true;
    case I52Id::SET_RETAIN_TWO_STAGE_DATA:
        retained_[0] = { 1, I52Id::NS_BEGIN_CMDCTX, false };
        retained_[1] = { 2, I52Id::CMDCTX_EXEC_01, false };
        fact(setup, L"slot1=APPCTX-NO-MUTATION;slot2=CMDCTX-EXEC-01;mutation=" + std::wstring(I52Plan::text(plan_->mutation)));
        return true;
    case I52Id::SET_PAYLOAD_PATH:
    {
        payloadPath_ = runDirectory() + L"\\I52CtdaPayload.arx";
        const DWORD attributes = GetFileAttributesW(payloadPath_.c_str());
        const bool exists = attributes != INVALID_FILE_ATTRIBUTES && (attributes & FILE_ATTRIBUTE_DIRECTORY) == 0;
        const std::wstring actual = exists ? sha256File(payloadPath_) : std::wstring();
        const std::wstring expected = environment(L"I52_CTDA_PAYLOAD_SHA256");
        const bool ok = exists && !expected.empty() && actual == expected;
        fact(setup, L"path=" + lower(payloadPath_) + L";exists=" + flag(exists) + L";sha256=" + actual + L";expected=" + expected + L";matches=" + flag(ok));
        if (!ok) fact(I52Id::UNK_LOAD, L"step=SET-PAYLOAD-PATH");
        return ok;
    }
    case I52Id::SET_LOAD_PAYLOAD:
    {
        // C15 only: the arm record is exported before the load so the payload's kInitAppMsg can query it.
        I52Log::C15Arm arm{};
        arm.armed = rowHasRegistration(I52Id::RR_PAYLOAD_DB);
        arm.databaseId = reinterpret_cast<uint64_t>(database_);
        arm.triggerObjectOldId = static_cast<uint64_t>(fixture_.ids().triggerModify.asOldId());
        arm.retainedDataId = 1;
        I52Log::instance().setC15Arm(arm);
        fact(setup, L"phase=ARM-RECORD;armed=" + flag(arm.armed) + L";triggerObject=" + oid(fixture_.ids().triggerModify));
        const bool loaded = loadPayload();
        const int registrations = I52Log::instance().countOf(L"R-PAYLOAD-ARX", L"MARK-REGISTRATION");
        const bool ok = loaded && registrations == 1;
        fact(setup, L"phase=VERIFY;loaded=" + flag(loaded) + L";markRegistration=" + std::to_wstring(registrations) + L";ok=" + flag(ok));
        if (!loaded) fact(I52Id::UNK_LOAD, L"step=SET-LOAD-PAYLOAD");
        if (registrations != 1) fact(I52Id::UNK_REGISTRATION, L"markRegistration=" + std::to_wstring(registrations));
        return ok;
    }
    case I52Id::SET_ARM_VETO:
        vetoArmed_ = true;
        fact(setup, L"guard=RG-ONESHOT;family=SA-VETO;request=RACKCAD_CTDA_V35#1");
        return true;
    default:
        fact(I52Id::UNKNOWN_COMMON, L"unimplementedSetup=" + std::wstring(I52Plan::text(setup)));
        return false;
    }
}

// acedArxLoad(NL-ARX-PATH) in STG-PAYLOAD-LOAD; STG-PAYLOAD-INIT is opened for the payload's kInitAppMsg records.
bool I52Executor::loadPayload()
{
    const uint64_t load = push(I52Id::STG_PAYLOAD_LOAD);
    I52Log::instance().open(I52Id::STG_PAYLOAD_INIT);
    const int result = acedArxLoad(payloadPath_.c_str());
    const bool loaded = moduleLoaded(payloadPath_);
    payloadLoaded_ = result == RTNORM && loaded;
    fact(I52Id::MARK_LOAD_RESULT, L"result=" + std::to_wstring(result) + L";rtnorm=" + flag(result == RTNORM) + L";loaded=" + flag(loaded) + L";module=" + lower(payloadPath_));
    if (rowHasToken(I52Id::TOK_PAYLOAD_LOAD_RESULT)) I52Ctda_TokenSetNative(I52Id::TOK_PAYLOAD_LOAD_RESULT, load);
    pop(I52Id::STG_PAYLOAD_LOAD);
    return payloadLoaded_;
}

// ------------------------------------------------------------------ guards
void I52Executor::armGuards(bool includeAppend)
{
    const I52Id event = bodyEvent();
    std::wstring family;
    auto target = [&](I52Id guard, const std::wstring& fam, I52Id ev, const std::wstring& extra, AcDbObjectId object) -> std::map<std::wstring, std::wstring>
    {
        std::map<std::wstring, std::wstring> key;
        AcDbTransactionManager* tm = database_ == nullptr ? nullptr : database_->transactionManager();
        const void* instance = nullptr;
        for (const Registered& r : registered_) if (r.id == plan_->bodyObserver) instance = r.instance;
        for (const std::wstring& name : I52Auth::guardKeys(guard, fam))
        {
            std::wstring value;
            if (name == L"ProbeId") value = I52Log::instance().probeId();
            else if (name == L"GuardId") value = I52Plan::text(guard);
            else if (name == L"DatabaseId") value = ptr(database_);
            else if (name == L"DocumentId") value = ptr(document_);
            else if (name == L"EventId") value = I52Plan::text(ev);
            else if (name == L"CallbackMember") value = callbackMember(ev);
            else if (name == L"TargetObjectId") value = oid(object);
            else if (name == L"ReactorInstance") value = ptr(instance);
            else if (name == L"TransactionManagerId") value = ptr(tm);
            else if (name == L"StageId") value = I52Plan::text(fam == L"SA-VETO" ? I52Id::STG_TRIGGER : triggerStage());
            else value = extra;
            key[name] = value;
        }
        return key;
    };
    for (size_t i = 0; i < plan_->guardCount; ++i)
    {
        const I52Id guard = plan_->guards[i];
        const I52Auth::Guard* info = I52Auth::guard(guard);
        if (guard == I52Id::RG_DB_APPEND && !includeAppend) { fact(guard, L"phase=ARM-DEFERRED;armedBy=TRIGGER"); continue; }
        if (guard == I52Id::RG_ONESHOT)
        {
            if (rowHasSetup(I52Id::SET_ARM_VETO))
            {
                GuardState veto{ guard, L"SA-VETO", target(guard, L"SA-VETO", I52Id::N_DOC_LOCK_WILL, L"RACKCAD_CTDA_V35#1", AcDbObjectId::kNull), true, false, false };
                guards_.push_back(veto);
                fact(guard, L"phase=ARM;family=SA-VETO;requestId=RACKCAD_CTDA_V35#1");
            }
            if (plan_->bodyObserver == I52Id::None || event == I52Id::None) continue;
            const std::wstring id = I52Plan::text(event);
            for (const wchar_t* fam : { L"N-OBJ-", L"N-ENT-", L"N-ED-", L"N-DOC-", L"N-RX-", L"N-DB-" }) if (id.rfind(fam, 0) == 0) family = fam;
            std::wstring extra;
            AcDbObjectId object;
            if (family == L"N-ED-") extra = boundCommand();
            else if (family == L"N-DOC-") extra = L"RACKCAD_CTDA_V35#1";
            else if (family == L"N-RX-") extra = lower(payloadPath_);
            else
            {
                const I52Auth::Registration* reg = I52Auth::registration(plan_->bodyObserver);
                object = fixtureObject(reg == nullptr ? I52Id::None : I52Plan::parse(reg->notifier));
            }
            guards_.push_back({ guard, family, target(guard, family, event, extra, object), true, false, false });
            fact(guard, L"phase=ARM;family=" + family + L";event=" + id + L";key=" + extra + oid(object));
            continue;
        }
        if (info == nullptr) { fact(I52Id::UNKNOWN_COMMON, L"unknownGuard=" + std::wstring(I52Plan::text(guard))); continue; }
        std::wstring extra;
        AcDbObjectId object;
        if (guard == I52Id::RG_TX)
        {
            family = L"N-TR-";
            extra = L"depth:" + std::to_wstring(plan_->trigger == I52Id::TRG_END_NESTED_T ? 2 : 1);
        }
        else
        {
            family = info->family;
            const std::wstring armTarget = info->armTargetText;
            object = armTarget == L"TRIGGER-TARGET" ? fixtureObject(I52Auth::trigger(plan_->trigger)->target) : fixtureObject(info->armTarget);
            if (guard == I52Id::RG_DB_ERASE) extra = L"1";
        }
        guards_.push_back({ guard, family, target(guard, family, event, extra, object), true, false, false });
        fact(guard, L"phase=ARM;family=" + family + L";target=" + oid(object) + L";extra=" + extra);
    }
}

void I52Executor::armAppendGuard(const void* instancePointer)
{
    for (size_t i = 0; i < plan_->guardCount; ++i)
    {
        if (plan_->guards[i] != I52Id::RG_DB_APPEND) continue;
        const std::wstring family = L"N-DB-APPEND";
        std::map<std::wstring, std::wstring> key;
        const void* instance = nullptr;
        for (const Registered& r : registered_) if (r.id == plan_->bodyObserver) instance = r.instance;
        for (const std::wstring& name : I52Auth::guardKeys(I52Id::RG_DB_APPEND, family))
        {
            if (name == L"ProbeId") key[name] = I52Log::instance().probeId();
            else if (name == L"GuardId") key[name] = I52Plan::text(I52Id::RG_DB_APPEND);
            else if (name == L"DatabaseId") key[name] = ptr(database_);
            else if (name == L"EventId") key[name] = I52Plan::text(I52Id::N_DB_APPEND);
            else if (name == L"CallbackMember") key[name] = callbackMember(I52Id::N_DB_APPEND);
            else if (name == L"StageId") key[name] = I52Plan::text(I52Id::STG_TRIGGER);
            else key[name] = ptr(instancePointer);  // TargetInstancePointer
        }
        (void)instance;
        guards_.push_back({ I52Id::RG_DB_APPEND, family, key, true, false, false });
        fact(I52Id::RG_DB_APPEND, L"phase=ARM;family=" + family + L";targetInstancePointer=" + ptr(instancePointer));
    }
}

void I52Executor::disarmGuards(const wchar_t* phase)
{
    const bool scriptIssued = plan_->trigger == I52Id::TRG_RUN_FIXTURE_CMD || plan_->trigger == I52Id::TRG_CANCEL_CMDCTX;
    const std::wstring p = phase;
    for (GuardState& g : guards_)
    {
        if (!g.armed) continue;
        if (p == L"DISARM" && scriptIssued) { g.handedOver = true; fact(g.guard, L"phase=HANDOVER;to=CMD-FINISH;family=" + g.family); continue; }
        g.armed = false;
        fact(g.guard, L"phase=DISARM;at=" + p + L";family=" + g.family + L";consumed=" + flag(g.consumed));
    }
}

// ------------------------------------------------------------------ TRIGGER
bool I52Executor::runTrigger()
{
    const uint64_t trigger = push(I52Id::STG_TRIGGER);
    (void)trigger;
    bool ok = true;
    AcTransactionManager* tm = document_->transactionManager();
    AcDbTransactionManager* dbtm = database_->transactionManager();
    const I52FixtureIds& ids = fixture_.ids();
    fact(plan_->trigger, L"phase=BEGIN;" + context());
    triggerBegun_ = true;
    auto openWrite = [&](AcDbObjectId id, AcDbObject*& object) { const Acad::ErrorStatus s = acdbOpenObject(object, id, AcDb::kForWrite); fact(plan_->trigger, L"step=acdbOpenObject;status=" + status(s) + L";object=" + oid(id)); return s; };
    switch (plan_->trigger)
    {
    case I52Id::TRG_MODIFY_TRIGGER_MOD:
    {
        AcDbObject* o = nullptr;
        if ((ok = openWrite(ids.triggerModify, o) == Acad::eOk))
        {
            AcDbPoint* point = AcDbPoint::cast(o);
            const Acad::ErrorStatus s = point == nullptr ? Acad::eWrongObjectType : point->setPosition(I52CtdaFixture::kTriggerModifiedPosition);
            const Acad::ErrorStatus c = o->close();
            fact(plan_->trigger, L"step=setPosition;status=" + status(s) + L";close=" + status(c));
            ok = s == Acad::eOk && c == Acad::eOk;
        }
        break;
    }
    case I52Id::TRG_ASSERT_WRITE_TRIGGER_XR:
    {
        AcDbObject* o = nullptr;
        if ((ok = openWrite(ids.triggerXrecord, o) == Acad::eOk))
        {
            o->assertWriteEnabled();
            AcDbXrecord* record = AcDbXrecord::cast(o);
            resbuf value{}; value.restype = AcDb::kDxfText; value.resval.rstring = const_cast<ACHAR*>(I52CtdaFixture::kTriggerXrWritten);
            const Acad::ErrorStatus s = record == nullptr ? Acad::eWrongObjectType : record->setFromRbChain(value);
            const Acad::ErrorStatus c = o->close();
            fact(plan_->trigger, L"step=write;status=" + status(s) + L";close=" + status(c));
            ok = s == Acad::eOk && c == Acad::eOk;
        }
        break;
    }
    case I52Id::TRG_ERASE_TRIGGER_DB: case I52Id::TRG_ERASE_TRIGGER_OBJ: case I52Id::TRG_ERASE_REF_B:
    {
        const AcDbObjectId id = plan_->trigger == I52Id::TRG_ERASE_TRIGGER_DB ? ids.triggerEraseDatabase : plan_->trigger == I52Id::TRG_ERASE_TRIGGER_OBJ ? ids.triggerEraseObject : ids.materialReference;
        AcDbObject* o = nullptr;
        if ((ok = openWrite(id, o) == Acad::eOk))
        {
            const Acad::ErrorStatus s = o->erase(true);
            const Acad::ErrorStatus c = o->close();
            fact(plan_->trigger, L"step=erase;status=" + status(s) + L";close=" + status(c));
            ok = s == Acad::eOk && c == Acad::eOk;
        }
        break;
    }
    case I52Id::TRG_APPEND_TRIGGER:
    {
        auto* point = new AcDbPoint(I52CtdaFixture::kTriggerAppendPosition);
        point->setDatabaseDefaults(database_);
        point->setLayer(L"0");
        armAppendGuard(point);
        AcDbObject* ms = nullptr;
        Acad::ErrorStatus s = acdbOpenObject(ms, ids.modelSpace, AcDb::kForWrite);
        AcDbObjectId appended;
        if (s == Acad::eOk)
        {
            s = AcDbBlockTableRecord::cast(ms)->appendAcDbEntity(appended, point);
            if (s == Acad::eOk) { point->close(); fixture_.setTriggerAppend(appended); } else delete point;
            ms->close();
        }
        else delete point;
        fact(plan_->trigger, L"step=append;status=" + status(s) + L";object=" + oid(appended));
        ok = s == Acad::eOk;
        break;
    }
    case I52Id::TRG_APPEND_TRIGGER_IN_PRIMARY:
    {
        // 02NAPP-SM: Model Space is owned for write by T-PRIMARY only; the body appends F-REF-C through the same T.
        const bool topIsPrimary = primary_ != nullptr && dbtm->topTransaction() == primary_;
        auto* point = new AcDbPoint(I52CtdaFixture::kTriggerAppendPosition);
        point->setDatabaseDefaults(database_);
        point->setLayer(L"0");
        armAppendGuard(point);
        AcDbObject* ms = nullptr;
        Acad::ErrorStatus s = topIsPrimary ? dbtm->getObject(ms, ids.modelSpace, AcDb::kForWrite) : Acad::eNoActiveTransactions;
        AcDbObjectId appended;
        if (s == Acad::eOk) s = AcDbBlockTableRecord::cast(ms)->appendAcDbEntity(appended, point);
        if (s == Acad::eOk) { s = dbtm->addNewlyCreatedDBRObject(point); fixture_.setTriggerAppend(appended); }
        else delete point;
        fact(plan_->trigger, L"step=append-in-primary;status=" + status(s) + L";topIsPrimary=" + flag(topIsPrimary) + L";object=" + oid(appended));
        ok = s == Acad::eOk;
        break;
    }
    case I52Id::TRG_OPEN_FOR_MODIFY_XR:
    {
        AcDbObject* o = nullptr;
        if ((ok = openWrite(ids.semanticXrecord, o) == Acad::eOk)) { o->assertWriteEnabled(); const Acad::ErrorStatus c = o->close(); fact(plan_->trigger, L"step=assertWriteEnabled;close=" + status(c)); ok = c == Acad::eOk; }
        break;
    }
    case I52Id::TRG_MODIFY_CLOSE_REF_A:
    {
        AcDbObject* o = nullptr;
        if ((ok = openWrite(ids.siblingA, o) == Acad::eOk)) { o->assertWriteEnabled(); const Acad::ErrorStatus c = o->close(); fact(plan_->trigger, L"step=assertWriteEnabled;close=" + status(c)); ok = c == Acad::eOk; }
        break;
    }
    case I52Id::TRG_TRANSFORM_CLOSE_REF_A:
    {
        AcDbObject* o = nullptr;
        if ((ok = openWrite(ids.siblingA, o) == Acad::eOk))
        {
            AcDbEntity* e = AcDbEntity::cast(o);
            const Acad::ErrorStatus s = e == nullptr ? Acad::eWrongObjectType : e->transformBy(AcGeMatrix3d::kIdentity);
            const Acad::ErrorStatus c = o->close();
            fact(plan_->trigger, L"step=transformBy;status=" + status(s) + L";close=" + status(c));
            ok = s == Acad::eOk && c == Acad::eOk;
        }
        break;
    }
    case I52Id::TRG_CANCEL_OPEN_XR:
    {
        AcDbObject* o = nullptr;
        if ((ok = openWrite(ids.semanticXrecord, o) == Acad::eOk))
        {
            o->assertWriteEnabled();
            AcDbXrecord* record = AcDbXrecord::cast(o);
            resbuf value{}; value.restype = AcDb::kDxfText; value.resval.rstring = const_cast<ACHAR*>(I52CtdaFixture::kCancelStagedBytes);
            const Acad::ErrorStatus w = record == nullptr ? Acad::eWrongObjectType : record->setFromRbChain(value);
            fact(plan_->trigger, L"step=stage;status=" + status(w) + L";stagedBytes=HFV35:XR:CANCEL-STAGED");
            const Acad::ErrorStatus c = o->cancel();
            fact(plan_->trigger, L"step=cancel;status=" + status(c));
            // OBS-CANCEL-RESTORED: read F-XR back before the origin body's scheduled delivery can run.
            AcDbObject* r = nullptr;
            const Acad::ErrorStatus rs = acdbOpenObject(r, ids.semanticXrecord, AcDb::kForRead);
            std::string bytes = "<UNAVAILABLE>";
            if (rs == Acad::eOk)
            {
                resbuf* chain = nullptr;
                if (AcDbXrecord::cast(r) != nullptr && AcDbXrecord::cast(r)->rbChain(&chain) == Acad::eOk && chain != nullptr && chain->restype == AcDb::kDxfText && chain->rbnext == nullptr)
                { bytes.clear(); for (const ACHAR* ch = chain->resval.rstring; ch != nullptr && *ch; ++ch) bytes.push_back(static_cast<char>(*ch < 0x80 ? *ch : '?')); }
                if (chain != nullptr) acutRelRb(chain);
                r->close();
            }
            fact(I52Id::OBS_CANCEL_RESTORED, L"read=" + status(rs) + L";bytes=" + I52Narrow(bytes));
            if (w != Acad::eOk || rs != Acad::eOk) fact(I52Id::UNK_CANCEL_STAGING, L"write=" + status(w) + L";read=" + status(rs));
            ok = w == Acad::eOk && c == Acad::eOk && rs == Acad::eOk;
        }
        break;
    }
    case I52Id::TRG_START_OUTER_T:
    {
        const int before = tm->numActiveTransactions();
        if (before != 0) { fact(I52Id::UNKNOWN_COMMON, L"step=TRG-START-OUTER-T;activeTransactions=" + std::to_wstring(before)); ok = false; break; }
        controlled_ = tm->startTransaction();
        fact(I52Id::SA_TX_START, L"transaction=" + ptr(controlled_) + L";owner=T-CONTROLLED;activeTransactions=" + std::to_wstring(tm->numActiveTransactions()));
        if (controlled_ == nullptr) { fact(I52Id::UNK_T_START, L"owner=T-CONTROLLED"); ok = false; break; }
        ++ownedTransactions_;
        const Acad::ErrorStatus e = tm->endTransaction();
        fact(I52Id::SA_TX_END, L"status=" + status(e) + L";owner=T-CONTROLLED");
        if (e == Acad::eOk) { controlled_ = nullptr; --ownedTransactions_; } else { fact(I52Id::UNK_T_END, L"owner=T-CONTROLLED"); ok = false; }
        break;
    }
    case I52Id::TRG_ABORT_PRIMARY_T:
    {
        const bool topIsPrimary = primary_ != nullptr && dbtm->topTransaction() == primary_;
        const Acad::ErrorStatus a = topIsPrimary ? tm->abortTransaction() : Acad::eNoActiveTransactions;
        fact(I52Id::SA_TX_ABORT, L"status=" + status(a) + L";owner=T-PRIMARY;trigger=1");
        if (a == Acad::eOk) { primary_ = nullptr; --ownedTransactions_; } else ok = false;
        break;
    }
    case I52Id::TRG_END_NESTED_T:
    {
        const bool topIsNested = nested_ != nullptr && dbtm->topTransaction() == nested_ && tm->numActiveTransactions() == 2;
        const Acad::ErrorStatus e = topIsNested ? tm->endTransaction() : Acad::eNoActiveTransactions;
        fact(I52Id::SA_TX_END, L"status=" + status(e) + L";owner=T-NESTED;activeTransactions=" + std::to_wstring(tm->numActiveTransactions()));
        if (e == Acad::eOk) { nested_ = nullptr; --ownedTransactions_; } else ok = false;
        break;
    }
    case I52Id::TRG_END_PRIMARY_T:
    {
        const bool topIsPrimary = primary_ != nullptr && dbtm->topTransaction() == primary_ && tm->numActiveTransactions() == 1;
        const Acad::ErrorStatus e = topIsPrimary ? tm->endTransaction() : Acad::eNoActiveTransactions;
        fact(I52Id::SA_TX_END, L"status=" + status(e) + L";owner=T-PRIMARY");
        if (e == Acad::eOk) { primary_ = nullptr; --ownedTransactions_; } else ok = false;
        break;
    }
    case I52Id::TRG_RUN_FIXTURE_CMD: case I52Id::TRG_CANCEL_CMDCTX:
        fact(plan_->trigger, L"phase=SCRIPT-ISSUED;command=" + boundCommand() + L";guardHandover=CMD-FINISH");
        break;
    case I52Id::TRG_LOCK_CYCLE: case I52Id::TRG_LOCK_VETO:
    {
        // Architect B11: SA-LOCK in the DRIVER-APP-01 delivery, then an explicit SA-UNLOCK (DRIVER-LOCK-01).
        ++lockRequest_;
        lockInFlight_ = true;
        const Acad::ErrorStatus lock = acDocManager->lockDocument(document_, AcAp::kWrite, L"RACKCAD_CTDA_V35", L"RACKCAD_CTDA_V35", false);
        lockInFlight_ = false;
        fact(I52Id::SA_LOCK, L"status=" + status(lock) + L";authority=DRIVER-LOCK-01;requestId=RACKCAD_CTDA_V35#" + std::to_wstring(lockRequest_)
            + L";vetoIssued=" + flag(vetoIssued_) + L";vetoStatus=" + status(vetoStatus_));
        if (lock == Acad::eOk)
        {
            ++ownedLocks_;
            fact(I52Id::DRIVER_LOCK_01, L"obligation=CREATED");
            const Acad::ErrorStatus unlock = acDocManager->unlockDocument(document_);
            fact(I52Id::SA_UNLOCK, L"status=" + status(unlock) + L";authority=DRIVER-LOCK-01");
            if (unlock == Acad::eOk) --ownedLocks_; else { fact(I52Id::UNK_LOCK_DOC_T_LEAK, L"step=DRIVER-LOCK-01 unlock"); ok = false; }
        }
        else if (plan_->trigger == I52Id::TRG_LOCK_CYCLE) { fact(I52Id::UNK_REJECTION, L"support=SA-LOCK;status=" + status(lock)); ok = false; }
        break;
    }
    case I52Id::TRG_APPCTX_SYNC: case I52Id::TRG_APPCTX_SYNC_BEGIN_CMDCTX:
    {
        syncDelivery_ = push(I52Id::STG_SYNC_APPCTX);
        acDocManager->executeInApplicationContext(&I52Executor::syncCallback, &retained_[0]);
        fact(I52Id::MARK_APPCTX_RETURN, L"callbackCompleted=1", I52Id::STG_SYNC_APPCTX, syncDelivery_);
        if (rowHasToken(I52Id::TOK_SYNC_RETURN)) I52Ctda_TokenSetNative(I52Id::TOK_SYNC_RETURN, syncDelivery_);
        pop(I52Id::STG_SYNC_APPCTX);
        break;
    }
    case I52Id::TRG_LOAD_PAYLOAD:
    {
        I52Log::C15Arm arm{};
        arm.databaseId = reinterpret_cast<uint64_t>(database_);
        I52Log::instance().setC15Arm(arm);
        ok = loadPayload();
        if (!ok) fact(I52Id::UNK_LOAD, L"step=TRG-LOAD-PAYLOAD");
        break;
    }
    default:
        fact(I52Id::UNKNOWN_COMMON, L"unimplementedTrigger=" + std::wstring(I52Plan::text(plan_->trigger)));
        ok = false;
        break;
    }
    fact(plan_->trigger, L"phase=END;ok=" + flag(ok));
    pop(I52Id::STG_TRIGGER);
    return ok;
}

// ------------------------------------------------------------------ POST-TRIGGER-OBLIGATIONS
void I52Executor::postTriggerObligations()
{
    AcTransactionManager* tm = document_->transactionManager();
    AcDbTransactionManager* dbtm = database_->transactionManager();
    if (rowHasSetup(I52Id::SET_OUTCOME_COMMIT) && primary_ != nullptr)
    {
        const bool topIsPrimary = dbtm->topTransaction() == primary_ && nested_ == nullptr;
        if (!aborted_ && topIsPrimary)
        {
            const Acad::ErrorStatus e = tm->endTransaction();
            fact(I52Id::SA_TX_END, L"status=" + status(e) + L";owner=T-PRIMARY;outcome=COMMIT");
            fact(I52Id::SET_OUTCOME_COMMIT, L"status=" + status(e));
            if (e == Acad::eOk) { primary_ = nullptr; --ownedTransactions_; outcomeCommitted_ = true; }
            else fact(I52Id::UNK_T_END, L"owner=T-PRIMARY");
        }
    }
    // Unwind: any driver-owned transaction still open is aborted (no abort outcome can PASS).
    for (AcTransaction** t : { &nested_, &primary_, &controlled_ })
    {
        if (*t == nullptr) continue;
        const Acad::ErrorStatus a = dbtm->topTransaction() == *t ? tm->abortTransaction() : Acad::eNotTopTransaction;
        fact(I52Id::SA_TX_ABORT, L"status=" + status(a) + L";owner=" + (t == &nested_ ? L"T-NESTED" : t == &primary_ ? L"T-PRIMARY" : L"T-CONTROLLED") + L";unwind=1");
        fact(I52Id::UNK_LOCK_DOC_T_LEAK, L"step=POST-TRIGGER unwind");
        if (a == Acad::eOk) { *t = nullptr; --ownedTransactions_; }
    }
    recordPhase(L"POST-TRIGGER-OBLIGATIONS", L"outcomeCommitted=" + flag(outcomeCommitted_) + L";ownedTransactions=" + std::to_wstring(ownedTransactions_));
    if (plan_->executionContext == I52Id::CB_PRIMARY_01 && outcomeCommitted_ && execOk_ && execDelivery_ != 0 && rowHasToken(I52Id::TOK_EXEC_DONE))
        I52Ctda_TokenSetNative(I52Id::TOK_EXEC_DONE, execDelivery_);
}

// ------------------------------------------------------------------ script-issued commands and the NS-SEND delivery
void I52Executor::fixtureCommand()
{
    if (!recording_) return;
    const bool opened = !commandStages_.empty() && commandStages_.back().command == L"I52CTDA_FIXTURE";
    if (!opened) { push(I52Id::STG_FIXTURE_CMD); commands_.push_back(L"I52CTDA_FIXTURE"); }
    fact(I52Id::CMD_FIXTURE, L"phase=ENTRY;observedByEditor=" + flag(opened) + L";databaseWrites=0");
    fact(I52Id::CMD_FIXTURE, L"phase=RETURN");
    if (!opened) { pop(I52Id::STG_FIXTURE_CMD); commands_.pop_back(); }
}

void I52Executor::cancelCommand()
{
    if (!recording_) return;
    const bool opened = !commandStages_.empty() && commandStages_.back().command == L"HFV34_CANCEL";
    if (!opened) { push(I52Id::STG_CANCEL_CMD); commands_.push_back(L"HFV34_CANCEL"); }
    fact(I52Id::CMD_CANCEL, L"phase=ENTRY;observedByEditor=" + flag(opened));
    fact(I52Id::CANCEL_CMDCTX_01, L"phase=SYNC-ENTER");
    syncDelivery_ = push(I52Id::STG_SYNC_APPCTX);
    acDocManager->executeInApplicationContext(&I52Executor::cancelSync, nullptr);
    fact(I52Id::MARK_APPCTX_RETURN, L"callbackCompleted=1;owner=CANCEL-CMDCTX-01", I52Id::STG_SYNC_APPCTX, syncDelivery_);
    pop(I52Id::STG_SYNC_APPCTX);
    fact(I52Id::CMD_CANCEL, L"phase=RETURN");
    if (!opened) { pop(I52Id::STG_CANCEL_CMD); commands_.pop_back(); }
}

void I52Executor::queuedCommand()
{
    if (!recording_) return;
    const bool opened = !commandStages_.empty() && commandStages_.back().command == L"I52CTDA_QUEUED";
    const uint64_t delivery = opened ? commandStages_.back().delivery : push(I52Id::STG_SEND_DELIVERY);
    if (!opened) commands_.push_back(L"I52CTDA_QUEUED");
    if (I52Log::instance().fenceIsSet()) recordLate(I52Id::STG_SEND_DELIVERY, L"delivery=NS-SEND;command=I52CTDA_QUEUED");
    else
    {
        --pendingSend_;
        fact(I52Id::EP_COMMAND, L"command=I52CTDA_QUEUED;retainedSlot=" + std::to_wstring(retained_[0].slot) + L";" + context(), I52Id::STG_SEND_DELIVERY, delivery);
        fact(I52Id::CTX_COMMAND, L"isApplicationContext=" + flag(acDocManager->isApplicationContext()), I52Id::STG_SEND_DELIVERY, delivery);
        const uint64_t exec = push(I52Id::STG_EXEC);
        runExecution(plan_->executionContext, exec);
        pop(I52Id::STG_EXEC);
    }
    if (!opened) { pop(I52Id::STG_SEND_DELIVERY); commands_.pop_back(); }
}

// ------------------------------------------------------------------ FIN-GATE-01
void I52Executor::onIdle()
{
    if (!gateRegistered_ || finishIssued_ || plan_ == nullptr) return;
    const std::vector<I52Id> missing = I52Log::instance().missingTokens(*plan_);
    const bool quiescent = document_ != nullptr && document_->isQuiescent();
    std::wstring missingText;
    for (I52Id t : missing) missingText += (missingText.empty() ? L"" : L",") + std::wstring(I52Plan::text(t));
    if (missingText != lastGateState_)
    {
        lastGateState_ = missingText;
        fact(I52Id::FIN_GATE_01, L"phase=CHECK;missing=" + (missingText.empty() ? std::wstring(L"NONE") : missingText) + L";quiescent=" + flag(quiescent)
            + L";isApplicationContext=" + flag(acDocManager->isApplicationContext()), I52Id::STG_FIN_GATE, gateDelivery_);
    }
    if (missing.empty() && quiescent) { issueFinish(false); return; }
    const unsigned long long elapsed = probeReturnTick_ == 0 ? 0 : GetTickCount64() - probeReturnTick_;
    if (probeReturnTick_ != 0 && elapsed >= static_cast<unsigned long long>(I52Auth::finishGateTimeoutSeconds()) * 1000ull && quiescent) issueFinish(true);
}

void I52Executor::issueFinish(bool drain)
{
    acedRemoveOnIdleWinMsg(idleThunk);
    if (gateTimer_ != 0) { KillTimer(nullptr, gateTimer_); gateTimer_ = 0; }
    drainMode_ = drain;
    const std::wstring command = L"I52CTDA_FINISH " + I52Log::instance().probeId() + L"\n";
    const Acad::ErrorStatus s = acDocManager->sendStringToExecute(document_, command.c_str(), false, false, false);
    finishIssued_ = s == Acad::eOk;
    fact(I52Id::FIN_GATE_01, L"phase=ISSUE;mode=" + std::wstring(drain ? L"FINISH-MODE-DRAIN" : L"FINISH-MODE-NORMAL") + L";status=" + status(s)
        + L";schedulerUse=FINISH-INFRA;credit=NONE", I52Id::STG_FIN_GATE, gateDelivery_);
    if (drain) fact(I52Id::UNK_FINISH_TIMEOUT, L"source=FIN-GATE-01;timeoutSeconds=" + std::to_wstring(I52Auth::finishGateTimeoutSeconds()), I52Id::STG_FIN_GATE, gateDelivery_);
}

// ------------------------------------------------------------------ CMD-FINISH
void I52Executor::finish()
{
    wchar_t argument[128]{};
    if (acedGetString(0, L"\nProbeId: ", argument) != RTNORM) argument[0] = L'\0';
    // First action: FINISH-FENCE-01 (internal setter; never exported).
    I52FinishFence::set();
    if (!recording_ || plan_ == nullptr) return;
    finishing_ = true;
    const bool opened = !commandStages_.empty() && commandStages_.back().command == L"I52CTDA_FINISH";
    if (!opened) { finishDelivery_ = I52Log::instance().open(I52Id::STG_FINISH); stages_.push_back({ I52Id::STG_FINISH, finishDelivery_ }); commands_.push_back(L"I52CTDA_FINISH"); }
    I52Log::instance().setCommandIdentity(L"I52CTDA_FINISH");
    fact(I52Id::FINISH_FENCE_01, L"phase=SET;fenceIsSet=" + std::to_wstring(I52Log::instance().fenceIsSet()));
    disarmGuards(L"FINISH-ENTRY");
    const std::vector<I52Id> missing = I52Log::instance().missingTokens(*plan_);
    std::wstring missingText;
    for (I52Id t : missing) missingText += (missingText.empty() ? L"" : L",") + std::wstring(I52Plan::text(t));
    const bool early = !missing.empty() || !finishIssued_;
    const bool drain = drainMode_ || early;
    fact(I52Id::CMD_FINISH, L"phase=ENTRY;mode=" + std::wstring(drain ? L"FINISH-MODE-DRAIN" : L"FINISH-MODE-NORMAL") + L";finishEarly=" + flag(early)
        + L";enteredThroughGate=" + flag(finishIssued_) + L";missing=" + (missingText.empty() ? std::wstring(L"NONE") : missingText)
        + L";probeArgument=" + argument + L";probeMatches=" + flag(I52Log::instance().probeId() == argument));
    if (drain) fact(I52Id::UNK_FINISH_TIMEOUT, L"source=CMD-FINISH;finishEarly=" + flag(early));
    selfAction_ = true;
    if (!drain) runVerifiers();
    runCleanup(drain);
    selfAction_ = false;
    fact(I52Id::CMD_FINISH, L"phase=FLUSH;lastSequence=" + std::to_wstring(I52Log::instance().lastSequence()));
    const Acad::ErrorStatus s = acDocManager->sendStringToExecute(document_, L"_.QUIT\n_Y\n", false, false, false);
    fact(I52Id::CMD_FINISH, L"phase=EXIT-QUEUED;exitAction=QUIT-DISCARD;status=" + status(s) + L";schedulerUse=FINISH-INFRA");
}

void I52Executor::runVerifiers()
{
    for (size_t i = 0; i < plan_->verifierCount; ++i)
    {
        const I52Id v = plan_->verifiers[i];
        switch (v)
        {
        case I52Id::VER_S: fact(v, L"phase=FRESH;bytes=" + I52Narrow(fixture_.readSemanticFresh())); break;
        case I52Id::VER_M: fact(v, L"phase=FRESH;" + materialPayload(fixture_.readMaterialFresh())); break;
        case I52Id::VER_SM: fact(v, L"phase=FRESH;" + smPayload(fixture_.captureSm(false))); break;
        case I52Id::VER_T:
        {
            AcDbTransactionManager* tm = database_->transactionManager();
            fact(v, L"phase=FRESH;activeTransactions=" + std::to_wstring(tm->numActiveTransactions()) + L";reread=" + I52Narrow(fixture_.readSemanticFresh())
                + L";outcomeCommitted=" + flag(outcomeCommitted_));
            break;
        }
        case I52Id::VER_ALL:
            fact(v, L"phase=FRESH;part=VER-S;bytes=" + I52Narrow(fixture_.readSemanticFresh()));
            fact(v, L"phase=FRESH;part=VER-M;" + materialPayload(fixture_.readMaterialFresh()));
            fact(v, L"phase=FRESH;part=VER-SM;" + smPayload(fixture_.captureSm(false)));
            break;
        default: fact(I52Id::UNKNOWN_COMMON, L"unimplementedVerifier=" + std::wstring(I52Plan::text(v))); break;
        }
    }
    if (rowHasObservation(I52Id::OBS_NOTIFIER_WRITE)) fact(I52Id::F_TRIGGER_XR, L"phase=FRESH;bytes=" + I52Narrow(fixture_.readTriggerXrFresh()));
}

// ------------------------------------------------------------------ cleanup (CleanupActionId steps, one terminal CLN-BASE)
void I52Executor::runCleanup(bool drain)
{
    fact(plan_->cleanup, L"phase=START;mode=" + std::wstring(drain ? L"DRAIN" : L"NORMAL"));
    bool all = true;
    for (size_t i = 0; i < plan_->cleanupStepCount; ++i)
    {
        const I52Id step = plan_->cleanupSteps[i];
        fact(step, L"phase=START");
        const bool ok = runCleanupStep(step, drain);
        fact(step, L"phase=END;ok=" + flag(ok));
        all = all && ok;
    }
    fact(plan_->cleanup, L"phase=END;ok=" + flag(all) + L";fence=" + I52Plan::text(plan_->fence));
}

bool I52Executor::runCleanupStep(I52Id step, bool drain)
{
    switch (step)
    {
    case I52Id::CLN_OBJ_REMOVE:
    {
        bool ok = true;
        for (Registered& r : registered_)
        {
            const I52Auth::Registration* info = I52Auth::registration(r.id);
            if (info == nullptr || !info->objectReactor || r.retainedFenced || r.removed) continue;
            removeObserver(r);
            ok = ok && r.removed;
        }
        return ok;
    }
    case I52Id::CLN_OBJ_RETAIN_FENCED:
    {
        for (Registered& r : registered_)
            if (r.id == I52Id::OR_REF_B) { r.retainedFenced = true; fact(step, L"registration=OR-REF-B;instance=" + ptr(r.instance) + L";retainedUntilProcessExit=1;notifierAccess=NONE"); }
        return true;
    }
    case I52Id::CLN_DOC_REMOVE:
    {
        bool ok = true;
        for (Registered& r : registered_) if (r.id == I52Id::RR_DOC && !r.removed) { removeObserver(r); ok = ok && r.removed; }
        return ok;
    }
    case I52Id::CLN_DEFER_DRAIN:
    {
        bool ok = true;
        for (Registered& r : registered_) if (r.id == plan_->bodyObserver && !r.removed && !r.retainedFenced) { removeObserver(r); ok = ok && r.removed; }
        const bool drained = pendingSend_ == 0 && pendingApplication_ == 0 && pendingCommand_ == 0 && pendingInfra_ == 0;
        fact(step, L"pendingSend=" + std::to_wstring(pendingSend_) + L";pendingApplication=" + std::to_wstring(pendingApplication_) + L";pendingCommand=" + std::to_wstring(pendingCommand_)
            + L";pendingInfra=" + std::to_wstring(pendingInfra_) + L";drained=" + flag(drained) + L";mode=" + (drain ? L"DRAIN" : L"NORMAL"));
        if (!drained) fact(I52Id::UNK_UNDRAINED, L"recordedNotWaited=" + flag(drain));
        return ok && drained;
    }
    case I52Id::CLN_BASE: return cleanupBase();
    case I52Id::CLN_PROCESS_EXIT:
        fact(step, L"executor=CONTROL-PLANE;inProcess=0");
        return true;
    default:
        fact(I52Id::UNKNOWN_COMMON, L"unimplementedCleanupStep=" + std::wstring(I52Plan::text(step)));
        return false;
    }
}

bool I52Executor::cleanupBase()
{
    bool ok = true;
    AcTransactionManager* tm = document_->transactionManager();
    AcDbTransactionManager* dbtm = database_->transactionManager();
    // 1. abort any owned open T (driver transactions and a leaked execution transaction)
    for (AcTransaction** t : { &execution_, &nested_, &primary_, &controlled_ })
    {
        if (*t == nullptr) continue;
        const Acad::ErrorStatus a = dbtm->topTransaction() == *t ? tm->abortTransaction() : Acad::eNotTopTransaction;
        fact(I52Id::SA_TX_ABORT, L"status=" + status(a) + L";step=CLN-BASE");
        if (a == Acad::eOk) { *t = nullptr; --ownedTransactions_; } else ok = false;
    }
    fact(I52Id::CLN_BASE, L"step=OWNED-T;ownedTransactions=" + std::to_wstring(ownedTransactions_));
    // 2. no outstanding driver or APPCTX lock obligation
    fact(I52Id::CLN_BASE, L"step=LOCK-OBLIGATIONS;ownedLocks=" + std::to_wstring(ownedLocks_));
    if (ownedLocks_ != 0) { fact(I52Id::UNK_LOCK_DOC_T_LEAK, L"step=CLN-BASE;ownedLocks=" + std::to_wstring(ownedLocks_)); ok = false; }
    // 3. payload removal export when the payload registered RR-PAYLOAD-DB
    if (payloadLoaded_)
    {
        HMODULE payload = GetModuleHandleW(L"I52CtdaPayload.arx");
        auto remove = payload == nullptr ? nullptr : reinterpret_cast<I52CtdaPayloadRemoveReactorFn>(GetProcAddress(payload, "I52CtdaPayload_RemoveReactor"));
        const int32_t result = remove == nullptr ? -2 : remove();
        fact(I52Id::CLN_BASE, L"step=PAYLOAD-REMOVE;exportBound=" + flag(remove != nullptr) + L";status=" + std::to_wstring(result));
        if (result != 0 && result != I52CTDA_PAYLOAD_NOTHING_REGISTERED) ok = false;
    }
    // 4. unregister every remaining observer except CLN-OBJ-RETAIN-FENCED
    for (Registered& r : registered_) { removeObserver(r); if (!r.removed && !r.retainedFenced && r.attached) ok = false; }
    // 5. erase each live fixture resource (never reopening an erased object) and remove R-SM-LINK
    AcTransaction* t = tm->startTransaction();
    int erased = 0, skipped = 0;
    Acad::ErrorStatus removed = t == nullptr ? Acad::eNullPtr : fixture_.removeFixtureResources(tm, erased, skipped);
    const Acad::ErrorStatus closed = t == nullptr ? Acad::eNullPtr : removed == Acad::eOk ? tm->endTransaction() : tm->abortTransaction();
    const bool carrierRemoved = fixture_.linkCarrierRemoved();
    fact(I52Id::CLN_BASE, L"step=FIXTURE-RESOURCES;status=" + status(removed) + L";transactionClose=" + status(closed) + L";erased=" + std::to_wstring(erased)
        + L";skippedErased=" + std::to_wstring(skipped));
    fact(I52Id::R_SM_LINK, L"linkCarrierRemoved=" + flag(carrierRemoved));
    ok = ok && removed == Acad::eOk && closed == Acad::eOk && carrierRemoved;
    // 6. disarm any guard still armed
    for (GuardState& g : guards_) if (g.armed) { g.armed = false; fact(g.guard, L"phase=DISARM;at=CLN-BASE"); }
    // 7. flush (every record is flushed on append) and verify: no owned T and an active-T count of zero
    const int active = dbtm->numActiveTransactions();
    if (ownedTransactions_ != 0 || active != 0) { fact(I52Id::UNK_LOCK_DOC_T_LEAK, L"step=CLN-BASE;ownedTransactions=" + std::to_wstring(ownedTransactions_) + L";activeTransactions=" + std::to_wstring(active)); ok = false; }
    fact(I52Id::CLN_BASE, L"step=VERIFY;ok=" + flag(ok) + L";activeTransactions=" + std::to_wstring(active) + L";closesDocument=0");
    return ok;
}

// ------------------------------------------------------------------ R3 host smoke (zero ProbeIds)
void I52Executor::smoke()
{
    I52Log& log = I52Log::instance();
    log.configure();
    const std::wstring output = environment(L"I52_CTDA_OUTPUT");
    std::vector<std::wstring> failures;
    if (!log.ready()) failures.push_back(L"LOGGER_UNAVAILABLE");
    if (output.empty()) failures.push_back(L"OUTPUT_MISSING");
    recording_ = log.ready();
    commands_.push_back(L"I52CTDA_SMOKE");
    log.setCommandIdentity(L"I52CTDA_SMOKE");
    push(I52Id::STG_PROBE_CMD);
    const uint64_t first = log.lastSequence() + 1;
    fact(I52Id::RUN_ENV_01, L"smoke=1;pid=" + std::to_wstring(GetCurrentProcessId()) + L";runDirectory=" + runDirectory());

    // BOOT-01 (8 identities, R-SM-LINK) in its own transaction.
    boot();
    fact(I52Id::BOOT_01, bootFacts_);
    if (bootFailed_) failures.push_back(L"BOOT_FAILED");

    // FIN-GATE-01 initialization: idle hook, 1000 ms timer and the STG-FIN-GATE activation. With no ProbeId there is no
    // plan, so onIdle() returns before any check and the gate cannot issue CMD-FINISH; the smoke removes it below.
    const bool gateHooked = registerFinishGate();
    const uintptr_t gateTimer = gateTimer_;
    const uint64_t gateDelivery = gateDelivery_;
    const int gateRecords = log.countOf(L"R-NATIVE-ARX", L"FIN-GATE-01");
    if (!gateHooked || gateTimer == 0 || gateDelivery == 0 || gateRecords != 1) failures.push_back(L"FIN_GATE_NOT_INITIALIZED");

    // Fence read ABI from the export table; the setter must not be exported.
    HMODULE self = GetModuleHandleW(L"I52CtdaNative.arx");
    auto fenceRead = self == nullptr ? nullptr : reinterpret_cast<I52CtdaFinishFenceIsSetFn>(GetProcAddress(self, "I52Ctda_FinishFenceIsSet"));
    const bool setterExported = self != nullptr && GetProcAddress(self, "I52Ctda_FinishFenceSet") != nullptr;
    const int32_t fenceBefore = fenceRead == nullptr ? -1 : fenceRead();
    if (fenceRead == nullptr) failures.push_back(L"FENCE_READ_EXPORT_MISSING");
    if (setterExported) failures.push_back(L"FENCE_SETTER_EXPORTED");
    if (fenceBefore != 0) failures.push_back(L"FENCE_SET_BEFORE_FINISH");

    // Payload lifecycle: path + identity, load (unarmed C15 answer), no registration, removal export.
    payloadPath_ = runDirectory() + L"\\I52CtdaPayload.arx";
    const std::wstring payloadSha = sha256File(payloadPath_);
    const std::wstring expectedSha = environment(L"I52_CTDA_PAYLOAD_SHA256");
    if (payloadSha.empty() || payloadSha != expectedSha) failures.push_back(L"PAYLOAD_IDENTITY");
    I52Log::C15Arm arm{};
    arm.databaseId = reinterpret_cast<uint64_t>(database_);
    log.setC15Arm(arm);
    const bool payloadOk = !payloadSha.empty() && loadPayload();
    const int payloadRecords = log.countFrom(L"R-PAYLOAD-ARX");
    const int payloadBinding = log.countOf(L"R-PAYLOAD-ARX", L"PAYLOAD-DB-BINDING");
    const int payloadRegistrations = log.countOf(L"R-PAYLOAD-ARX", L"MARK-REGISTRATION");
    if (!payloadOk) failures.push_back(L"PAYLOAD_LOAD");
    if (payloadRecords == 0 || payloadBinding != 1) failures.push_back(L"PAYLOAD_SEQUENCER_BINDING");
    if (payloadRegistrations != 0) failures.push_back(L"PAYLOAD_REGISTERED_WHEN_UNARMED");
    HMODULE payload = GetModuleHandleW(L"I52CtdaPayload.arx");
    auto remove = payload == nullptr ? nullptr : reinterpret_cast<I52CtdaPayloadRemoveReactorFn>(GetProcAddress(payload, "I52CtdaPayload_RemoveReactor"));
    const int32_t removeResult = remove == nullptr ? -2 : remove();
    if (removeResult != I52CTDA_PAYLOAD_NOTHING_REGISTERED) failures.push_back(L"PAYLOAD_REMOVE_EXPORT");

    // Managed observer lifecycle (NETLOADed by the smoke script): subscribe and unsubscribe the registration entry.
    I52CtdaManagedEntryFn entry = log.managedEntry();
    const int32_t subscribed = entry == nullptr ? -1 : entry(1, reinterpret_cast<uint64_t>(document_), log.probeId().c_str());
    const int32_t unsubscribed = entry == nullptr ? -1 : entry(0, reinterpret_cast<uint64_t>(document_), log.probeId().c_str());
    const int managedRecords = log.countFrom(L"R-MANAGED-OBSERVER");
    if (entry == nullptr) failures.push_back(L"MANAGED_ENTRY_UNBOUND");
    if (subscribed != 0 || unsubscribed != 0) failures.push_back(L"MANAGED_LIFECYCLE");
    if (managedRecords < 2) failures.push_back(L"MANAGED_SEQUENCER_BINDING");
    // Smoke-only managed fence read (unset here); read again after the fence is set.
    const int32_t managedFenceBefore = entry == nullptr ? -1 : entry(I52CTDA_MANAGED_SMOKE_FENCE_READ, reinterpret_cast<uint64_t>(document_), log.probeId().c_str());

    // Cleanup: every fixture resource and R-SM-LINK removed in its own transaction.
    AcTransactionManager* tm = document_ == nullptr ? nullptr : document_->transactionManager();
    AcTransaction* t = tm == nullptr ? nullptr : tm->startTransaction();
    int erased = 0, skipped = 0;
    const Acad::ErrorStatus removed = t == nullptr || !fixture_.isBound() ? Acad::eNullPtr : fixture_.removeFixtureResources(tm, erased, skipped);
    const Acad::ErrorStatus closed = t == nullptr ? Acad::eNullPtr : removed == Acad::eOk ? tm->endTransaction() : tm->abortTransaction();
    const bool carrierRemoved = fixture_.linkCarrierRemoved();
    fact(I52Id::CLN_BASE, L"smoke=1;status=" + status(removed) + L";transactionClose=" + status(closed) + L";erased=" + std::to_wstring(erased) + L";linkCarrierRemoved=" + flag(carrierRemoved));
    if (removed != Acad::eOk || closed != Acad::eOk || !carrierRemoved) failures.push_back(L"CLEANUP");

    // FIN-GATE-01 teardown: the gate issued no CMD-FINISH and no completion token was accepted.
    const bool finishIssued = finishIssued_;
    const int finishRecords = log.countOf(L"R-NATIVE-ARX", L"CMD-FINISH");
    const size_t tokensAccepted = log.tokenCount();
    const bool hookRemoved = gateRegistered_ && acedRemoveOnIdleWinMsg(idleThunk);
    const bool timerKilled = gateTimer_ != 0 && KillTimer(nullptr, gateTimer_) != FALSE;
    gateTimer_ = 0;
    gateRegistered_ = false;
    fact(I52Id::FIN_GATE_01, L"smoke=1;phase=UNREGISTERED;hookRemoved=" + flag(hookRemoved) + L";timerKilled=" + flag(timerKilled) + L";finishIssued=" + flag(finishIssued)
        + L";tokensAccepted=" + std::to_wstring(tokensAccepted), I52Id::STG_FIN_GATE, gateDelivery);
    if (finishIssued || finishRecords != 0) failures.push_back(L"SPONTANEOUS_FINISH");
    if (tokensAccepted != 0) failures.push_back(L"TOKEN_FABRICATED");
    if (!hookRemoved || !timerKilled) failures.push_back(L"FIN_GATE_TEARDOWN");

    // Fence: set through the internal setter, read back through the export and by the managed observer.
    I52FinishFence::set();
    const int32_t fenceAfter = fenceRead == nullptr ? -1 : fenceRead();
    if (fenceAfter != 1) failures.push_back(L"FENCE_READ_AFTER_SET");
    const int32_t managedFenceAfter = entry == nullptr ? -1 : entry(I52CTDA_MANAGED_SMOKE_FENCE_READ, reinterpret_cast<uint64_t>(document_), log.probeId().c_str());
    const int managedFenceRecords = log.countOf(L"R-MANAGED-OBSERVER", L"FINISH-FENCE-01");
    if (managedFenceBefore != 0 || managedFenceAfter != 1 || managedFenceRecords != 2) failures.push_back(L"MANAGED_FENCE_READ");
    fact(I52Id::FINISH_FENCE_01, L"smoke=1;before=" + std::to_wstring(fenceBefore) + L";after=" + std::to_wstring(fenceAfter) + L";setterExported=" + flag(setterExported));
    const uint64_t last = log.lastSequence();
    pop(I52Id::STG_PROBE_CMD);

    std::ostringstream failureList;
    for (size_t i = 0; i < failures.size(); ++i) failureList << (i == 0 ? "" : ",") << '"' << utf8(failures[i]) << '"';
    std::ofstream stream(output, std::ios::binary | std::ios::trunc);
    stream << "{\n  \"schemaVersion\": 5,\n  \"instrument\": \"I52CtdaNative\",\n  \"stage\": \"R3_SMOKE\",\n  \"commandIdentity\": \"I52CTDA_SMOKE\",\n"
           << "  \"result\": \"" << (failures.empty() ? "PASS" : "FAIL") << "\",\n  \"failures\": [" << failureList.str() << "],\n"
           << "  \"processId\": " << GetCurrentProcessId() << ",\n  \"loggerReady\": " << (log.ready() ? "true" : "false") << ",\n"
           << "  \"sequenceFirst\": " << first << ",\n  \"sequenceLast\": " << last << ",\n"
           << "  \"fixture\": { \"declared\": " << I52CtdaFixture::kDeclaredIdentities << ", \"bootFacts\": \"" << jsonEscape(utf8(bootFacts_)) << "\", \"snapshot\": \"" << jsonEscape(utf8(bootSnapshot_)) << "\" },\n"
           << "  \"payload\": { \"path\": \"" << jsonEscape(utf8(payloadPath_)) << "\", \"sha256\": \"" << utf8(payloadSha) << "\", \"loaded\": " << (payloadOk ? "true" : "false")
           << ", \"records\": " << payloadRecords << ", \"bindingRecords\": " << payloadBinding << ", \"registrations\": " << payloadRegistrations << ", \"removeResult\": " << removeResult << " },\n"
           << "  \"managed\": { \"entryBound\": " << (entry != nullptr ? "true" : "false") << ", \"subscribe\": " << subscribed << ", \"unsubscribe\": " << unsubscribed << ", \"records\": " << managedRecords
           << ", \"fenceReadBefore\": " << managedFenceBefore << ", \"fenceReadAfter\": " << managedFenceAfter << ", \"fenceReadRecords\": " << managedFenceRecords << " },\n"
           << "  \"finGate\": { \"idleHook\": " << (gateHooked ? "true" : "false") << ", \"timer\": " << gateTimer << ", \"stageDelivery\": " << gateDelivery
           << ", \"registeredRecords\": " << gateRecords << ", \"finishIssued\": " << (finishIssued ? "true" : "false") << ", \"finishRecords\": " << finishRecords
           << ", \"tokensAccepted\": " << tokensAccepted << ", \"hookRemoved\": " << (hookRemoved ? "true" : "false") << ", \"timerKilled\": " << (timerKilled ? "true" : "false") << " },\n"
           << "  \"fence\": { \"readExport\": " << (fenceRead != nullptr ? "true" : "false") << ", \"setterExported\": " << (setterExported ? "true" : "false")
           << ", \"before\": " << fenceBefore << ", \"after\": " << fenceAfter << " },\n"
           << "  \"cleanup\": { \"erased\": " << erased << ", \"linkCarrierRemoved\": " << (carrierRemoved ? "true" : "false") << " },\n"
           << "  \"governedProbesDispatched\": 0\n}\n";
    commands_.pop_back();
    acutPrintf(failures.empty() ? L"\nI52 CT-DA R3 smoke PASS.\n" : L"\nI52 CT-DA R3 smoke FAIL.\n");
}
