#include "I52CtdaExecutor.h"
#include "I52CtdaAuthority.h"

#include "aced.h"
#include "acedads.h"
#include "actrans.h"
#include "dbapserv.h"
#include "dbents.h"
#include "dbobjptr.h"
#include "dbtrans.h"
#include "rxdlinkr.h"

#include <Windows.h>

#include <cwchar>
#include <cwctype>
#include <sstream>

// ------------------------------------------------------------------ reactor classes (exact retained instances)
namespace
{
I52Executor& X() { return I52Executor::instance(); }

class DatabaseReactor final : public AcDbDatabaseReactor
{
public:
    explicit DatabaseReactor(I52Id registration) : registration_(registration) {}
    void objectAppended(const AcDbDatabase*, const AcDbObject* o) override { X().onDatabase(registration_, this, I52Id::N_DB_APPEND, o, false); }
    void objectErased(const AcDbDatabase*, const AcDbObject* o, bool erasing) override { X().onDatabase(registration_, this, I52Id::N_DB_ERASE, o, erasing); }
    void objectModified(const AcDbDatabase*, const AcDbObject* o) override { X().onDatabase(registration_, this, I52Id::N_DB_MOD, o, false); }
    void objectOpenedForModify(const AcDbDatabase*, const AcDbObject* o) override { X().onDatabase(registration_, this, I52Id::N_DB_OPEN, o, false); }
private:
    I52Id registration_;
};

class ObjectReactor final : public AcDbObjectReactor
{
public:
    explicit ObjectReactor(I52Id registration) : registration_(registration) {}
    void cancelled(const AcDbObject* o) override { X().onObject(registration_, this, I52Id::N_OBJ_CANCEL, o, AcDbObjectId::kNull, false); }
    void objectClosed(const AcDbObjectId id) override { X().onObject(registration_, this, I52Id::N_OBJ_CLOSED, nullptr, id, false); }
    void erased(const AcDbObject* o, bool erasing) override { X().onObject(registration_, this, I52Id::N_OBJ_ERASE, o, AcDbObjectId::kNull, erasing); }
    void modified(const AcDbObject* o) override { X().onObject(registration_, this, I52Id::N_OBJ_MOD, o, AcDbObjectId::kNull, false); }
    void openedForModify(const AcDbObject* o) override { X().onObject(registration_, this, I52Id::N_OBJ_OPEN, o, AcDbObjectId::kNull, false); }
    void modifyUndone(const AcDbObject* o) override { X().onObject(registration_, this, I52Id::N_OBJ_UNDO, o, AcDbObjectId::kNull, false); }
private:
    I52Id registration_;
};

class EntityReactor final : public AcDbEntityReactor
{
public:
    explicit EntityReactor(I52Id registration) : registration_(registration) {}
    void modifiedGraphics(const AcDbEntity* e) override { X().onObject(registration_, this, I52Id::N_ENT_GFX, e, AcDbObjectId::kNull, false); }
    void cancelled(const AcDbObject* o) override { X().onObject(registration_, this, I52Id::N_OBJ_CANCEL, o, AcDbObjectId::kNull, false); }
    void objectClosed(const AcDbObjectId id) override { X().onObject(registration_, this, I52Id::N_OBJ_CLOSED, nullptr, id, false); }
    void erased(const AcDbObject* o, bool erasing) override { X().onObject(registration_, this, I52Id::N_OBJ_ERASE, o, AcDbObjectId::kNull, erasing); }
    void modified(const AcDbObject* o) override { X().onObject(registration_, this, I52Id::N_OBJ_MOD, o, AcDbObjectId::kNull, false); }
    void openedForModify(const AcDbObject* o) override { X().onObject(registration_, this, I52Id::N_OBJ_OPEN, o, AcDbObjectId::kNull, false); }
    void modifyUndone(const AcDbObject* o) override { X().onObject(registration_, this, I52Id::N_OBJ_UNDO, o, AcDbObjectId::kNull, false); }
private:
    I52Id registration_;
};

class TransactionReactor final : public AcTransactionReactor
{
public:
    void transactionAboutToStart(int& n, AcDbTransactionManager* m) override { X().onTransaction(this, I52Id::N_TR_ABOUT_START, n, m); }
    void transactionStarted(int& n, AcDbTransactionManager* m) override { X().onTransaction(this, I52Id::N_TR_STARTED, n, m); }
    void transactionAboutToEnd(int& n, AcDbTransactionManager* m) override { X().onTransaction(this, I52Id::N_TR_ABOUT_END, n, m); }
    void transactionEnded(int& n, AcDbTransactionManager* m) override { X().onTransaction(this, I52Id::N_TR_ENDED, n, m); }
    void transactionAboutToAbort(int& n, AcDbTransactionManager* m) override { X().onTransaction(this, I52Id::N_TR_ABOUT_ABORT, n, m); }
    void transactionAborted(int& n, AcDbTransactionManager* m) override { X().onTransaction(this, I52Id::N_TR_ABORTED, n, m); }
    void endCalledOnOutermostTransaction(int& n, AcDbTransactionManager* m) override { X().onTransaction(this, I52Id::N_TR_OUTERMOST_END_CALLED, n, m); }
};

class EditorReactor final : public AcEditorReactor
{
public:
    void commandWillStart(const ACHAR* c) override { X().onEditor(this, I52Id::N_ED_WILL, c); }
    void commandEnded(const ACHAR* c) override { X().onEditor(this, I52Id::N_ED_END, c); }
    void commandCancelled(const ACHAR* c) override { X().onEditor(this, I52Id::N_ED_CANCEL, c); }
};

class DocumentReactor final : public AcApDocManagerReactor
{
public:
    Acad::ErrorStatus requestVeto() { return veto(); }
    void documentLockModeWillChange(AcApDocument* d, AcAp::DocLockMode myCurrent, AcAp::DocLockMode myNew, AcAp::DocLockMode current, const ACHAR* global) override
    { X().onDocument(this, I52Id::N_DOC_LOCK_WILL, d, myCurrent, myNew, current, global); }
    void documentLockModeChangeVetoed(AcApDocument* d, const ACHAR* global) override
    { X().onDocument(this, I52Id::N_DOC_LOCK_VETO, d, 0, 0, 0, global); }
    void documentLockModeChanged(AcApDocument* d, AcAp::DocLockMode myPrevious, AcAp::DocLockMode myCurrent, AcAp::DocLockMode current, const ACHAR* global) override
    { X().onDocument(this, I52Id::N_DOC_LOCK_CHANGED, d, myPrevious, myCurrent, current, global); }
};

class LinkerReactor final : public AcRxDLinkerReactor
{
public:
    void rxAppWillBeLoaded(const ACHAR* n) override { X().onLinker(this, I52Id::N_RX_WILL_LOAD, n); }
    void rxAppLoaded(const ACHAR* n) override { X().onLinker(this, I52Id::N_RX_LOADED, n); }
};

std::wstring status(Acad::ErrorStatus s) { return std::to_wstring(static_cast<int>(s)); }
std::wstring ptr(const void* p) { return I52Hex(reinterpret_cast<uint64_t>(p)); }
std::wstring oid(const AcDbObjectId& id) { return I52Hex(static_cast<uint64_t>(id.asOldId())); }
std::wstring flag(bool b) { return b ? L"1" : L"0"; }
std::wstring text(const ACHAR* value) { std::wstring t = value == nullptr ? L"" : value; for (wchar_t& c : t) if (c == L';' || c == L'=') c = L'_'; return t; }
std::wstring lower(std::wstring value) { for (wchar_t& c : value) c = static_cast<wchar_t>(std::towlower(c)); return value; }

bool writeCapable(int mode) { return mode == AcAp::kWrite || mode == AcAp::kAutoWrite || mode == AcAp::kProtectedAutoWrite; }

const wchar_t* member(I52Id event)
{
    switch (event)
    {
    case I52Id::N_DB_APPEND: return L"objectAppended";
    case I52Id::N_DB_ERASE: return L"objectErased";
    case I52Id::N_DB_MOD: return L"objectModified";
    case I52Id::N_DB_OPEN: return L"objectOpenedForModify";
    case I52Id::N_OBJ_CANCEL: return L"cancelled";
    case I52Id::N_OBJ_CLOSED: return L"objectClosed";
    case I52Id::N_OBJ_ERASE: return L"erased";
    case I52Id::N_OBJ_MOD: return L"modified";
    case I52Id::N_OBJ_OPEN: return L"openedForModify";
    case I52Id::N_OBJ_UNDO: return L"modifyUndone";
    case I52Id::N_ENT_GFX: return L"modifiedGraphics";
    case I52Id::N_TR_ABOUT_START: return L"transactionAboutToStart";
    case I52Id::N_TR_STARTED: return L"transactionStarted";
    case I52Id::N_TR_ABOUT_END: return L"transactionAboutToEnd";
    case I52Id::N_TR_ENDED: return L"transactionEnded";
    case I52Id::N_TR_ABOUT_ABORT: return L"transactionAboutToAbort";
    case I52Id::N_TR_ABORTED: return L"transactionAborted";
    case I52Id::N_TR_OUTERMOST_END_CALLED: return L"endCalledOnOutermostTransaction";
    case I52Id::N_ED_WILL: return L"commandWillStart";
    case I52Id::N_ED_END: return L"commandEnded";
    case I52Id::N_ED_CANCEL: return L"commandCancelled";
    case I52Id::N_DOC_LOCK_WILL: return L"documentLockModeWillChange";
    case I52Id::N_DOC_LOCK_VETO: return L"documentLockModeChangeVetoed";
    case I52Id::N_DOC_LOCK_CHANGED: return L"documentLockModeChanged";
    case I52Id::N_RX_WILL_LOAD: return L"rxAppWillBeLoaded";
    case I52Id::N_RX_LOADED: return L"rxAppLoaded";
    default: return L"NONE";
    }
}

// Depth of the transaction a transaction-reactor callback is about, from the reactor-supplied count: the subject is
// the new transaction for about-to-start and the removed one for aborted. D-1 (09N-B canary on the qualified host):
// transactionEnded reports numTransactions = 1 while the active count is already 0, so the ended subject is n.
int subjectDepth(I52Id event, int n)
{
    switch (event)
    {
    case I52Id::N_TR_ABOUT_START: case I52Id::N_TR_ABORTED: return n + 1;
    default: return n;
    }
}
}

I52Executor& I52Executor::instance() { static I52Executor executor; return executor; }

// ------------------------------------------------------------------ recording
I52Id I52Executor::currentStage() const
{
    if (!stages_.empty()) return stages_.back().stage;
    if (finishing_) return I52Id::STG_FINISH;
    if (gateRegistered_) return I52Id::STG_FIN_GATE;
    return I52Id::STG_PROBE_CMD;
}

uint64_t I52Executor::currentDelivery() const
{
    if (!stages_.empty()) return stages_.back().delivery;
    if (finishing_) return finishDelivery_;
    if (gateRegistered_) return gateDelivery_;
    return I52Log::instance().current(I52Id::STG_PROBE_CMD);
}

uint64_t I52Executor::push(I52Id stage)
{
    const uint64_t delivery = I52Log::instance().open(stage);
    stages_.push_back({ stage, delivery });
    return delivery;
}

void I52Executor::pop(I52Id stage)
{
    for (size_t i = stages_.size(); i-- > 0;)
        if (stages_[i].stage == stage) { stages_.erase(stages_.begin() + static_cast<std::ptrdiff_t>(i)); return; }
}

I52Id I52Executor::deliveryScheduler(I52Id stage) const
{
    switch (stage)
    {
    case I52Id::STG_SEND_DELIVERY: return I52Id::NS_SEND;
    case I52Id::STG_APPCTX_DELIVERY: return I52Id::NS_BEGIN_APPCTX;
    case I52Id::STG_CMDCTX_DELIVERY: return I52Id::NS_BEGIN_CMDCTX;
    default: return plan_ == nullptr ? I52Id::DRIVER_CMD_01 : plan_->driver;
    }
}

std::wstring I52Executor::context() const
{
    const bool app = acDocManager != nullptr && acDocManager->isApplicationContext();
    AcApDocument* current = acDocManager == nullptr ? nullptr : acDocManager->curDocument();
    const int lock = document_ == nullptr ? -1 : static_cast<int>(document_->lockMode(true));
    AcDbTransactionManager* tm = database_ == nullptr ? nullptr : database_->transactionManager();
    return L"appContext=" + flag(app) + L";curDocument=" + ptr(current) + L";lockMode=" + std::to_wstring(lock)
        + L";activeTransactions=" + std::to_wstring(tm == nullptr ? -1 : tm->numActiveTransactions()) + L";top=" + ptr(tm == nullptr ? nullptr : tm->topTransaction());
}

uint64_t I52Executor::fact(I52Id event, const std::wstring& payload, I52Id stage, uint64_t delivery, I52Id driverOrScheduler)
{
    if (!recording_) return 0;
    I52Log& log = I52Log::instance();
    const I52Id s = stage == I52Id::None ? currentStage() : stage;
    const uint64_t d = delivery != 0 ? delivery : (stage == I52Id::None ? currentDelivery() : log.current(s));
    const I52Id who = driverOrScheduler != I52Id::None ? driverOrScheduler : deliveryScheduler(s);
    const std::wstring command = commands_.empty() ? L"NONE" : commands_.back();
    I52CtdaRecord record{ sizeof(I52CtdaRecord), log.probeId().c_str(), I52Plan::text(s), d, I52Plan::text(who), L"R-NATIVE-ARX",
        reinterpret_cast<uint64_t>(document_), reinterpret_cast<uint64_t>(database_), command.c_str(), I52Plan::text(event), payload.c_str() };
    return log.append(record);
}

void I52Executor::recordPhase(const wchar_t* phase, const std::wstring& detail)
{
    fact(plan_->driver, std::wstring(L"phase=") + phase + (detail.empty() ? L"" : L";" + detail));
}

// ------------------------------------------------------------------ plan helpers
bool I52Executor::rowHasSetup(I52Id id) const { return plan_ != nullptr && rowHas(plan_->setup, plan_->setupCount, id); }
bool I52Executor::rowHasRegistration(I52Id id) const { return plan_ != nullptr && rowHas(plan_->registrations, plan_->registrationCount, id); }
bool I52Executor::rowHasObservation(I52Id id) const { return plan_ != nullptr && rowHas(plan_->observations, plan_->observationCount, id); }
bool I52Executor::rowHasToken(I52Id id) const { return plan_ != nullptr && rowHas(plan_->tokens, plan_->tokenCount, id); }

// The body callback identity: the PrimaryAuthority EventId, or the ScheduleOrigin EventId for scheduled rows.
I52Id I52Executor::bodyEvent() const
{
    if (plan_ == nullptr) return I52Id::None;
    if (std::wcscmp(I52Plan::kind(plan_->primaryAuthority), L"EventId") == 0) return plan_->primaryAuthority;
    if (std::wcscmp(I52Plan::kind(plan_->scheduleOrigin), L"EventId") == 0) return plan_->scheduleOrigin;
    return I52Id::None;
}

// Innermost stage in which the trigger's own operation raises the body callback (guard key StageId).
I52Id I52Executor::triggerStage() const
{
    switch (plan_->trigger)
    {
    case I52Id::TRG_RUN_FIXTURE_CMD: return I52Id::STG_FIXTURE_CMD;
    case I52Id::TRG_CANCEL_CMDCTX: return I52Id::STG_CANCEL_CMD;
    case I52Id::TRG_LOAD_PAYLOAD: return I52Id::STG_PAYLOAD_LOAD;
    default: return I52Id::STG_TRIGGER;
    }
}

std::wstring I52Executor::boundCommand() const
{
    const I52Auth::Trigger* t = I52Auth::trigger(plan_->trigger);
    if (t == nullptr) return {};
    const std::wstring cls = t->targetClass;
    return cls.rfind(L"CMD:", 0) == 0 ? cls.substr(4) : std::wstring();
}

AcDbObjectId I52Executor::fixtureObject(I52Id identity) const
{
    const I52FixtureIds& ids = fixture_.ids();
    switch (identity)
    {
    case I52Id::F_TRIGGER_MOD: return ids.triggerModify;
    case I52Id::F_TRIGGER_ERASE_DB: return ids.triggerEraseDatabase;
    case I52Id::F_TRIGGER_ERASE_OBJ: return ids.triggerEraseObject;
    case I52Id::F_TRIGGER_XR: return ids.triggerXrecord;
    case I52Id::F_XR: return ids.semanticXrecord;
    case I52Id::F_REF_B: return ids.materialReference;
    case I52Id::F_REF_A: return ids.siblingA;
    case I52Id::F_REF_C: return ids.siblingC;
    case I52Id::F_TRIGGER_APPEND: return ids.triggerAppend;
    default: return AcDbObjectId::kNull;
    }
}

// ------------------------------------------------------------------ guards (keySchema)
std::map<std::wstring, std::wstring> I52Executor::callbackKey(I52Id guard, const std::wstring& family, I52Id event, const void* instance, AcDbObjectId target, const std::wstring& extra) const
{
    std::map<std::wstring, std::wstring> key;
    AcDbTransactionManager* tm = database_ == nullptr ? nullptr : database_->transactionManager();
    for (const std::wstring& name : I52Auth::guardKeys(guard, family))
    {
        std::wstring value;
        if (name == L"ProbeId") value = I52Log::instance().probeId();
        else if (name == L"GuardId") value = I52Plan::text(guard);
        else if (name == L"DatabaseId") value = ptr(database_);
        else if (name == L"DocumentId") value = ptr(document_);
        else if (name == L"EventId") value = I52Plan::text(event);
        else if (name == L"CallbackMember") value = member(event);
        else if (name == L"TargetObjectId") value = oid(target);
        else if (name == L"ReactorInstance") value = ptr(instance);
        else if (name == L"TransactionManagerId") value = ptr(tm);
        else if (name == L"StageId") value = I52Plan::text(currentStage());
        else value = extra;  // TransactionIdentity, CommandName, RequestId, ModulePath, TargetInstancePointer, ErasingFlag
        key[name] = value;
    }
    return key;
}

I52Executor::GuardState* I52Executor::guardFor(I52Id event)
{
    const std::wstring id = I52Plan::text(event);
    for (GuardState& g : guards_)
    {
        if (g.family == L"SA-VETO") continue;
        if (g.guard == I52Id::RG_TX ? id.rfind(L"N-TR-", 0) == 0 : g.guard == I52Id::RG_ONESHOT ? id.rfind(g.family, 0) == 0 : g.family == id)
            return &g;
    }
    return nullptr;
}

I52Executor::Admission I52Executor::admit(I52Id registration, const void* instance, I52Id event, AcDbObjectId target, const std::wstring& extra)
{
    if (plan_ == nullptr || registration != plan_->bodyObserver || event != bodyEvent()) return Admission::Observe;
    GuardState* g = guardFor(event);
    if (g == nullptr || !g->armed) return Admission::Observe;
    const std::map<std::wstring, std::wstring> key = callbackKey(g->guard, g->family, event, instance, target, extra);
    if (I52Log::instance().fenceIsSet() && !selfAction_) return Admission::Late;
    if (inBody_)
    {
        // Notifications caused by the body's own actions are logged and never re-enter the body; for the
        // DB/object/one-shot guards the same identity (ignoring StageId) re-entering is other recursion.
        if (g->guard == I52Id::RG_TX) return Admission::Suppressed;
        std::map<std::wstring, std::wstring> a = key, b = g->target;
        a.erase(L"StageId"); b.erase(L"StageId");
        return a == b ? Admission::Recursion : Admission::Suppressed;
    }
    if (key != g->target) return Admission::Observe;
    if (g->consumed) return Admission::Observe;
    g->consumed = true;
    return Admission::RunBody;
}

// ------------------------------------------------------------------ reactor entry points
void I52Executor::onDatabase(I52Id registration, const void* instance, I52Id event, const AcDbObject* object, bool erasing)
{
    if (!recording_) return;
    if (removedOrRetained(registration, instance, event)) return;
    const bool fenced = I52Log::instance().fenceIsSet() != 0;
    if (fenced || selfAction_)
    {
        if (fenced && !selfAction_ && registration == plan_->bodyObserver && event == bodyEvent())
            recordLate(plan_->scheduleOrigin == event ? I52Id::STG_ORIGIN : I52Id::STG_PRIMARY_CALLBACK, L"registration=" + std::wstring(I52Plan::text(registration)) + L";instance=" + ptr(instance));
        else fact(event, L"registration=" + std::wstring(I52Plan::text(registration)) + L";instance=" + ptr(instance) + L";notifierAccess=NONE;phase=FINISH-OBSERVATION");
        return;
    }
    const AcDbObjectId id = object == nullptr ? AcDbObjectId::kNull : object->objectId();
    std::wstring extra;
    if (event == I52Id::N_DB_APPEND) extra = ptr(object);
    else if (event == I52Id::N_DB_ERASE) extra = flag(erasing);
    fact(event, L"registration=" + std::wstring(I52Plan::text(registration)) + L";instance=" + ptr(instance) + L";object=" + oid(id)
        + L";pointer=" + ptr(object) + L";erasing=" + flag(erasing) + L";" + context());
    observationEvidence(event);
    switch (admit(registration, instance, event, id, extra))
    {
    case Admission::RunBody: runBody(registration, event, L"object=" + oid(id)); break;
    case Admission::Suppressed: fact(guardFor(event)->guard, L"phase=SUPPRESSED;event=" + std::wstring(I52Plan::text(event)) + L";object=" + oid(id)); break;
    case Admission::Recursion: fact(guardFor(event)->guard, L"phase=RECURSION;event=" + std::wstring(I52Plan::text(event)) + L";object=" + oid(id)); break;
    case Admission::Late: recordLate(I52Id::STG_PRIMARY_CALLBACK, L"registration=" + std::wstring(I52Plan::text(registration))); break;
    case Admission::Observe: break;
    }
}

void I52Executor::onObject(I52Id registration, const void* instance, I52Id event, const AcDbObject* object, AcDbObjectId closedId, bool erasing)
{
    if (!recording_) return;
    if (removedOrRetained(registration, instance, event)) return;
    const bool fenced = I52Log::instance().fenceIsSet() != 0;
    if (fenced || selfAction_)
    {
        if (fenced && !selfAction_ && registration == plan_->bodyObserver && event == bodyEvent())
            recordLate(plan_->scheduleOrigin == event ? I52Id::STG_ORIGIN : I52Id::STG_PRIMARY_CALLBACK, L"registration=" + std::wstring(I52Plan::text(registration)) + L";instance=" + ptr(instance));
        else fact(event, L"registration=" + std::wstring(I52Plan::text(registration)) + L";instance=" + ptr(instance) + L";notifierAccess=NONE;phase=FINISH-OBSERVATION");
        return;
    }
    const AcDbObjectId id = object == nullptr ? closedId : object->objectId();
    fact(event, L"registration=" + std::wstring(I52Plan::text(registration)) + L";instance=" + ptr(instance) + L";object=" + oid(id)
        + L";erasing=" + flag(erasing) + L";" + context());
    observationEvidence(event);
    switch (admit(registration, instance, event, id, flag(erasing)))
    {
    case Admission::RunBody: runBody(registration, event, L"object=" + oid(id) + L";erasing=" + flag(erasing)); break;
    case Admission::Suppressed: fact(guardFor(event)->guard, L"phase=SUPPRESSED;event=" + std::wstring(I52Plan::text(event)) + L";object=" + oid(id)); break;
    case Admission::Recursion: fact(guardFor(event)->guard, L"phase=RECURSION;event=" + std::wstring(I52Plan::text(event)) + L";object=" + oid(id)); break;
    case Admission::Late: recordLate(I52Id::STG_PRIMARY_CALLBACK, L"registration=" + std::wstring(I52Plan::text(registration))); break;
    case Admission::Observe: break;
    }
}

void I52Executor::onTransaction(const void* instance, I52Id event, int numTransactions, AcDbTransactionManager* manager)
{
    if (!recording_) return;
    if (removedOrRetained(I52Id::RR_TX, instance, event)) return;
    const bool fenced = I52Log::instance().fenceIsSet() != 0;
    if (fenced || selfAction_)
    {
        if (fenced && !selfAction_ && plan_->bodyObserver == I52Id::RR_TX && event == bodyEvent())
            recordLate(plan_->scheduleOrigin == event ? I52Id::STG_ORIGIN : I52Id::STG_PRIMARY_CALLBACK, L"registration=RR-TX;instance=" + ptr(instance));
        else fact(event, L"registration=RR-TX;instance=" + ptr(instance) + L";numTransactions=" + std::to_wstring(numTransactions) + L";notifierAccess=NONE;phase=FINISH-OBSERVATION");
        return;
    }
    const int depth = subjectDepth(event, numTransactions);
    fact(event, L"registration=RR-TX;instance=" + ptr(instance) + L";manager=" + ptr(manager) + L";numTransactions=" + std::to_wstring(numTransactions)
        + L";subjectDepth=" + std::to_wstring(depth) + L";" + context());
    observationEvidence(event);
    switch (admit(I52Id::RR_TX, instance, event, AcDbObjectId::kNull, L"depth:" + std::to_wstring(depth)))
    {
    case Admission::RunBody: runBody(I52Id::RR_TX, event, L"numTransactions=" + std::to_wstring(numTransactions)); break;
    case Admission::Suppressed: fact(I52Id::RG_TX, L"phase=SUPPRESSED;event=" + std::wstring(I52Plan::text(event)) + L";depth=" + std::to_wstring(depth)); break;
    case Admission::Recursion: fact(I52Id::RG_TX, L"phase=RECURSION;event=" + std::wstring(I52Plan::text(event))); break;
    case Admission::Late: recordLate(I52Id::STG_PRIMARY_CALLBACK, L"registration=RR-TX"); break;
    case Admission::Observe: break;
    }
}

void I52Executor::onEditor(const void* instance, I52Id event, const ACHAR* commandName)
{
    if (!recording_) return;
    if (removedOrRetained(I52Id::RR_ED, instance, event)) return;
    const std::wstring command = text(commandName);
    if (event == I52Id::N_ED_WILL)
    {
        // Strictly before the next commandWillStart: a COMMAND-END-WINDOW closes here at the latest.
        closeCommandWindow(L"NEXT-COMMAND-WILL-START");
        I52Id stage = I52Id::None;
        if (command == L"I52CTDA_QUEUED") stage = I52Id::STG_SEND_DELIVERY;
        else if (command == L"I52CTDA_FIXTURE") stage = I52Id::STG_FIXTURE_CMD;
        else if (command == L"HFV34_CANCEL") stage = I52Id::STG_CANCEL_CMD;
        else if (command == L"I52CTDA_FINISH") { stage = I52Id::STG_FINISH; }
        else if (pendingCommand_ > 0 && command != L"I52CTDA_BOOT" && command != L"I52CTDA_PROBE") stage = I52Id::STG_CMDCTX_DELIVERY;
        if (stage != I52Id::None)
        {
            const uint64_t delivery = stage == I52Id::STG_FINISH ? (finishDelivery_ = I52Log::instance().open(I52Id::STG_FINISH)) : push(stage);
            if (stage == I52Id::STG_FINISH) stages_.push_back({ stage, delivery });
            commandStages_.push_back({ command, stage, delivery });
        }
        commands_.push_back(command);
        I52Log::instance().setCommandIdentity(command);
    }
    const bool fenced = I52Log::instance().fenceIsSet() != 0;
    // I52CTDA_PROBE's own end belongs to the probe activation (N-ED-END@STG-PROBE-CMD), after the stage returned.
    const bool probeEnd = event != I52Id::N_ED_WILL && command == L"I52CTDA_PROBE";
    fact(event, L"registration=RR-ED;instance=" + ptr(instance) + L";command=" + command + L";" + context(),
        probeEnd ? I52Id::STG_PROBE_CMD : I52Id::None, probeEnd ? I52Log::instance().current(I52Id::STG_PROBE_CMD) : 0);
    if (!fenced && !selfAction_)
    {
        observationEvidence(event);
        switch (admit(I52Id::RR_ED, instance, event, AcDbObjectId::kNull, command))
        {
        case Admission::RunBody: runBody(I52Id::RR_ED, event, L"command=" + command); break;
        case Admission::Suppressed: fact(guardFor(event)->guard, L"phase=SUPPRESSED;event=" + std::wstring(I52Plan::text(event)) + L";command=" + command); break;
        case Admission::Recursion: fact(guardFor(event)->guard, L"phase=RECURSION;event=" + std::wstring(I52Plan::text(event)) + L";command=" + command); break;
        case Admission::Late: recordLate(I52Id::STG_PRIMARY_CALLBACK, L"registration=RR-ED;command=" + command); break;
        case Admission::Observe: break;
        }
    }
    else if (fenced && !selfAction_ && plan_->bodyObserver == I52Id::RR_ED && event == bodyEvent())
        recordLate(plan_->scheduleOrigin == event ? I52Id::STG_ORIGIN : I52Id::STG_PRIMARY_CALLBACK, L"registration=RR-ED;command=" + command);
    if (event == I52Id::N_ED_WILL) return;

    // commandEnded / commandCancelled close the command stage; tokens and lock-release windows follow the stage.
    if (!commandStages_.empty() && commandStages_.back().command == command)
    {
        const CommandStage ended = commandStages_.back();
        commandStages_.pop_back();
        if (event == I52Id::N_ED_END && ended.stage == I52Id::STG_SEND_DELIVERY && rowHasToken(I52Id::TOK_SEND_CMD_END)) I52Ctda_TokenSetNative(I52Id::TOK_SEND_CMD_END, ended.delivery);
        if (event == I52Id::N_ED_END && ended.stage == I52Id::STG_FIXTURE_CMD && rowHasToken(I52Id::TOK_FIXTURE_CMD_END)) I52Ctda_TokenSetNative(I52Id::TOK_FIXTURE_CMD_END, ended.delivery);
        if (event == I52Id::N_ED_CANCEL && ended.stage == I52Id::STG_CANCEL_CMD && rowHasToken(I52Id::TOK_CANCEL_OBSERVED)) I52Ctda_TokenSetNative(I52Id::TOK_CANCEL_OBSERVED, ended.delivery);
        if (ended.stage != I52Id::STG_FINISH) pop(ended.stage);
        if (!I52Auth::lockAnchor(ended.stage).empty() && I52Auth::lockAnchor(ended.stage) == L"COMMAND-END-WINDOW") openCommandWindow(ended.stage, ended.delivery);
    }
    if (!commands_.empty() && commands_.back() == command) commands_.pop_back();
    I52Log::instance().setCommandIdentity(commands_.empty() ? L"NONE" : commands_.back());
}

void I52Executor::onDocument(const void* instance, I52Id event, AcApDocument* document, int myCurrent, int myNew, int current, const ACHAR* globalCommand)
{
    if (!recording_) return;
    if (removedOrRetained(I52Id::RR_DOC, instance, event)) return;
    const std::wstring global = text(globalCommand);
    const bool ours = document == document_ && global == L"RACKCAD_CTDA_V35" && lockInFlight_;
    const std::wstring request = ours ? L"RACKCAD_CTDA_V35#" + std::to_wstring(lockRequest_) : L"FOREIGN:" + global;
    const bool fenced = I52Log::instance().fenceIsSet() != 0;
    fact(event, L"registration=RR-DOC;instance=" + ptr(instance) + L";document=" + ptr(document) + L";scratch=" + flag(document == document_)
        + L";myCurrent=" + std::to_wstring(myCurrent) + L";myNew=" + std::to_wstring(myNew) + L";current=" + std::to_wstring(current)
        + L";globalCommand=" + global + L";requestId=" + request + (fenced ? L";notifierAccess=NONE" : L""));
    if (document != document_) return;

    // LOCK-RELEASE-BIND-01 anchors.
    if (event == I52Id::N_DOC_LOCK_CHANGED)
    {
        if (appUnlock_.active && !appUnlock_.resolved)
        {
            appUnlock_.resolved = true;
            fact(I52Id::MARK_LOCK_RELEASE, L"anchor=APPCTX-UNLOCK-01-CALL;current=" + std::to_wstring(current), appUnlock_.stage, appUnlock_.delivery);
            if (rowHasToken(I52Id::TOK_LOCK_RELEASE)) I52Ctda_TokenSetNative(I52Id::TOK_LOCK_RELEASE, appUnlock_.delivery);
        }
        else if (window_.open && current == AcAp::kNotLocked)
        {
            if (!window_.resolved)
            {
                window_.resolved = true;
                fact(I52Id::MARK_LOCK_RELEASE, L"anchor=COMMAND-END-WINDOW;current=" + std::to_wstring(current), window_.stage, window_.delivery);
                if (rowHasToken(I52Id::TOK_LOCK_RELEASE)) I52Ctda_TokenSetNative(I52Id::TOK_LOCK_RELEASE, window_.delivery);
            }
            else fact(I52Id::MARK_LOCK_RELEASE, L"anchor=COMMAND-END-WINDOW;candidate=SECOND;current=" + std::to_wstring(current), window_.stage, window_.delivery);
        }
    }
    if (fenced || selfAction_)
    {
        if (fenced && !selfAction_ && plan_->bodyObserver == I52Id::RR_DOC && event == bodyEvent()) recordLate(I52Id::STG_PRIMARY_CALLBACK, L"registration=RR-DOC;instance=" + ptr(instance));
        return;
    }

    // SET-ARM-VETO: the exact TRG-LOCK-VETO request calls SA-VETO once (RG-ONESHOT, SA-VETO key).
    if (event == I52Id::N_DOC_LOCK_WILL && vetoArmed_ && ours)
    {
        for (GuardState& g : guards_)
        {
            if (g.family != L"SA-VETO" || !g.armed) continue;
            const std::map<std::wstring, std::wstring> key = callbackKey(g.guard, g.family, event, instance, AcDbObjectId::kNull, request);
            if (key != g.target || g.consumed) { fact(g.guard, L"phase=OBSERVE;family=SA-VETO;requestId=" + request); break; }
            g.consumed = true;
            fact(g.guard, L"phase=CONSUME;family=SA-VETO;requestId=" + request);
            for (Registered& r : registered_)
                if (r.id == I52Id::RR_DOC && r.instance == instance) { vetoStatus_ = static_cast<DocumentReactor*>(r.instance)->requestVeto(); vetoIssued_ = true; }
            fact(I52Id::SA_VETO, L"status=" + status(vetoStatus_) + L";requestId=" + request);
            break;
        }
    }
    observationEvidence(event);
    switch (admit(I52Id::RR_DOC, instance, event, AcDbObjectId::kNull, request))
    {
    case Admission::RunBody: runBody(I52Id::RR_DOC, event, L"requestId=" + request); break;
    case Admission::Suppressed: fact(guardFor(event)->guard, L"phase=SUPPRESSED;event=" + std::wstring(I52Plan::text(event))); break;
    case Admission::Recursion: fact(guardFor(event)->guard, L"phase=RECURSION;event=" + std::wstring(I52Plan::text(event))); break;
    case Admission::Late: recordLate(I52Id::STG_PRIMARY_CALLBACK, L"registration=RR-DOC"); break;
    case Admission::Observe: break;
    }
}

void I52Executor::onLinker(const void* instance, I52Id event, const ACHAR* module)
{
    if (!recording_) return;
    if (removedOrRetained(I52Id::RR_DLINK, instance, event)) return;
    const std::wstring path = lower(text(module));
    const bool exact = !payloadPath_.empty() && path == lower(payloadPath_);
    const bool fenced = I52Log::instance().fenceIsSet() != 0;
    fact(event, L"registration=RR-DLINK;instance=" + ptr(instance) + L";module=" + path + L";exactModule=" + flag(exact) + (fenced ? L";notifierAccess=NONE" : L""));
    if (fenced || selfAction_)
    {
        if (fenced && !selfAction_ && plan_->bodyObserver == I52Id::RR_DLINK && event == bodyEvent()) recordLate(I52Id::STG_PRIMARY_CALLBACK, L"registration=RR-DLINK;instance=" + ptr(instance));
        return;
    }
    observationEvidence(event);
    switch (admit(I52Id::RR_DLINK, instance, event, AcDbObjectId::kNull, path))
    {
    case Admission::RunBody: runBody(I52Id::RR_DLINK, event, L"module=" + path); break;
    case Admission::Suppressed: fact(guardFor(event)->guard, L"phase=SUPPRESSED;event=" + std::wstring(I52Plan::text(event))); break;
    case Admission::Recursion: fact(guardFor(event)->guard, L"phase=RECURSION;event=" + std::wstring(I52Plan::text(event))); break;
    case Admission::Late: recordLate(I52Id::STG_PRIMARY_CALLBACK, L"registration=RR-DLINK"); break;
    case Admission::Observe: break;
    }
}

// Callback-time evidence of the PrimaryAuthority callback (VER-T event part, OBS-COUNT, OBS-VISIBILITY, TOP-*).
void I52Executor::recordCallbackEvidence(I52Id event)
{
    AcDbTransactionManager* tm = database_ == nullptr ? nullptr : database_->transactionManager();
    std::wstring payload = L"event=" + std::wstring(I52Plan::text(event)) + L";" + context();
    if (tm != nullptr && tm->numActiveTransactions() > 0 && (rowHasObservation(I52Id::OBS_VISIBILITY) || rowHas(plan_->verifiers, plan_->verifierCount, I52Id::VER_T)))
    {
        std::string bytes;
        const Acad::ErrorStatus read = I52CtdaFixture::readXrecordText(tm, fixture_.ids().semanticXrecord, bytes);
        payload += L";readThroughTop=" + status(read) + L";bytes=" + I52Narrow(bytes);
    }
    payload += L";primary=" + ptr(primary_) + L";nested=" + ptr(nested_) + L";controlled=" + ptr(controlled_);
    for (size_t i = 0; i < plan_->topCount; ++i) fact(plan_->top[i], payload);
    if (rowHas(plan_->verifiers, plan_->verifierCount, I52Id::VER_T)) fact(I52Id::VER_T, L"phase=CALLBACK;" + payload);
}


// CLN-OBJ-RETAIN-FENCED: the retained reactor only logs, with NotifierAccess=NONE (FENCED-RETENTION-SAFE); it is not a
// governed delivery. A delivery to an instance whose eOk removal is recorded is the V34 section 5 contradiction
// (FP-CLEANUP-SAFETY b). Both are decided before any notifier access, for every reactor kind.
bool I52Executor::removedOrRetained(I52Id registration, const void* instance, I52Id event)
{
    for (const Registered& r : registered_)
    {
        if (r.instance != instance) continue;
        if (r.retainedFenced)
        {
            fact(I52Id::CLN_OBJ_RETAIN_FENCED, L"kind=RETAINED-NOTIFICATION;registration=" + std::wstring(I52Plan::text(registration)) + L";instance=" + ptr(instance)
                + L";event=" + I52Plan::text(event) + L";notifierAccess=NONE");
            return true;
        }
        if (r.removed)
        {
            recordLate(I52Id::STG_PRIMARY_CALLBACK, L"registration=" + std::wstring(I52Plan::text(registration)) + L";instance=" + ptr(instance) + L";event=" + I52Plan::text(event));
            return true;
        }
    }
    return false;
}

// Callback-time evidence of observation rows: the first PrimaryAuthority callback after the trigger began.
void I52Executor::observationEvidence(I52Id event)
{
    if (plan_->bodyObserver != I52Id::None || !triggerBegun_ || evidenceRecorded_ || event != plan_->primaryAuthority) return;
    evidenceRecorded_ = true;
    recordCallbackEvidence(event);
}

// ------------------------------------------------------------------ bodies
void I52Executor::runBody(I52Id registration, I52Id event, const std::wstring& evidence)
{
    const bool origin = plan_->scheduleOrigin == event && std::wcscmp(I52Plan::kind(plan_->scheduleOrigin), L"EventId") == 0;
    const I52Id stage = origin ? I52Id::STG_ORIGIN : I52Id::STG_PRIMARY_CALLBACK;
    inBody_ = true;
    bodyStage_ = stage;
    push(stage);
    GuardState* g = guardFor(event);
    fact(I52Id::EP_CALLBACK, L"event=" + std::wstring(I52Plan::text(event)) + L";registration=" + I52Plan::text(registration) + L";" + evidence + L";" + context());
    if (g != nullptr) fact(g->guard, L"phase=CONSUME;event=" + std::wstring(I52Plan::text(event)));
    fact(I52Id::CTX_CALLBACK, L"event=" + std::wstring(I52Plan::text(event)) + L";" + context());
    recordCallbackEvidence(event);
    fact(I52Id::LOCK_OBS, L"lockMode=" + std::to_wstring(document_ == nullptr ? -1 : static_cast<int>(document_->lockMode(true)))
        + L";myLockMode=" + std::to_wstring(document_ == nullptr ? -1 : static_cast<int>(document_->myLockMode())));
    if (rowHasObservation(I52Id::OBS_EWASNOTIFYING))
    {
        AcDbObject* a = nullptr;
        const Acad::ErrorStatus s = acdbOpenObject(a, fixture_.ids().siblingA, AcDb::kForWrite);
        if (s == Acad::eOk && a != nullptr) a->close();
        fact(I52Id::OBS_EWASNOTIFYING, L"acdbOpenObjectForWrite=" + status(s) + L";target=F-REF-A");
    }
    if (origin)
    {
        const std::vector<I52Id> steps = I52Auth::chainSteps(plan_->schedulerChain);
        if (!steps.empty() && steps.front() == I52Id::NS_SEND) enqueueSend();
        else if (!steps.empty() && steps.front() == I52Id::NS_BEGIN_APPCTX) enqueueApplication();
        else fact(plan_->schedulerChain, L"status=NOT-AN-ORIGIN-CHAIN");
        fact(I52Id::MARK_ORIGIN_RETURN, L"event=" + std::wstring(I52Plan::text(event)));
    }
    else if (plan_->executionContext != I52Id::EXEC_OBSERVE)
    {
        const uint64_t exec = push(I52Id::STG_EXEC);
        execDelivery_ = exec;
        execOk_ = runExecution(plan_->executionContext, exec);
        pop(I52Id::STG_EXEC);
    }
    pop(stage);
    inBody_ = false;
    bodyStage_ = I52Id::None;
}

void I52Executor::recordLate(I52Id stage, const std::wstring& detail)
{
    // FINISH-FENCE-01: the entry only appends one LATE-DELIVERY record and returns (no lock, T, write or scheduler call).
    const uint64_t delivery = I52Log::instance().open(stage);
    bool removed = false;
    for (const Registered& r : registered_) if (r.removed && detail.find(ptr(r.instance)) != std::wstring::npos) removed = true;
    fact(I52Id::FINISH_FENCE_01, L"kind=LATE-DELIVERY;notifierAccess=NONE;removedInstance=" + flag(removed) + L";" + detail, stage, delivery);
}

// ------------------------------------------------------------------ execution contexts
bool I52Executor::observeWriteLock(I52Id lockAuthority)
{
    const int mode = document_ == nullptr ? -1 : static_cast<int>(document_->lockMode(true));
    const bool ok = writeCapable(mode);
    fact(lockAuthority, L"lockMode=" + std::to_wstring(mode) + L";writeCapable=" + flag(ok));
    if (!ok) fact(I52Id::UNK_NO_WRITE_LOCK, L"authority=" + std::wstring(I52Plan::text(lockAuthority)) + L";lockMode=" + std::to_wstring(mode));
    return ok;
}

AcApDocument* I52Executor::resolveScratchDocument(I52Id unknownIfMissing)
{
    AcApDocument* current = acDocManager == nullptr ? nullptr : acDocManager->curDocument();
    AcApDocument* active = acDocManager == nullptr ? nullptr : acDocManager->mdiActiveDocument();
    const bool ok = current == document_ && active == document_ && document_ != nullptr;
    if (!ok) fact(unknownIfMissing, L"curDocument=" + ptr(current) + L";mdiActiveDocument=" + ptr(active) + L";scratch=" + ptr(document_));
    return ok ? document_ : nullptr;
}

Acad::ErrorStatus I52Executor::mutate(I52Id mutation)
{
    AcDbTransactionManager* tm = database_ == nullptr ? nullptr : database_->transactionManager();
    Acad::ErrorStatus s = Acad::eOk;
    switch (mutation)
    {
    case I52Id::MUT_NONE: s = Acad::eOk; break;
    case I52Id::MUT_S: s = fixture_.mutateSemantic(); break;
    case I52Id::MUT_M: s = fixture_.mutateMaterial(); break;
    case I52Id::MUT_SM: s = fixture_.mutateMixed(); break;
    case I52Id::MUT_ALL: s = fixture_.mutateAll(); break;
    default: s = Acad::eNotImplementedYet; break;
    }
    // OBS-T-AFFILIATION: the mutation record names the transaction it ran in.
    fact(mutation, L"status=" + status(s) + L";transaction=" + ptr(tm == nullptr ? nullptr : tm->topTransaction())
        + L";activeTransactions=" + std::to_wstring(tm == nullptr ? -1 : tm->numActiveTransactions()));
    if (s != Acad::eOk) fact(I52Id::UNK_ILLEGAL_MUTATION, L"mutation=" + std::wstring(I52Plan::text(mutation)) + L";status=" + status(s));
    return s;
}

// Runs one ExecutionContextId inside its STG-EXEC activation. Any failed step is recorded and is UNKNOWN.
bool I52Executor::runExecution(I52Id exec, uint64_t execDelivery)
{
    fact(exec, L"phase=ENTRY;" + context());
    AcDbTransactionManager* dbtm = database_ == nullptr ? nullptr : database_->transactionManager();
    bool ok = true;
    auto ownedTransaction = [&](I52Id transactionId) -> bool
    {
        AcTransactionManager* tm = document_ == nullptr ? nullptr : document_->transactionManager();
        fact(I52Id::SA_TX_RESOLVE, L"manager=" + ptr(tm) + L";status=" + flag(tm != nullptr));
        if (tm == nullptr) { fact(I52Id::UNK_ILLEGAL_DB_CONTEXT, L"step=SA-TX-RESOLVE"); return false; }
        AcTransaction* t = tm->startTransaction();
        fact(I52Id::SA_TX_START, L"transaction=" + ptr(t) + L";owner=" + I52Plan::text(transactionId) + L";activeTransactions=" + std::to_wstring(tm->numActiveTransactions()));
        if (t == nullptr) { fact(I52Id::UNK_T_START, L"owner=" + std::wstring(I52Plan::text(transactionId))); return false; }
        ++ownedTransactions_;
        execution_ = t;
        for (size_t i = 0; i < plan_->topCount; ++i) if (plan_->top[i] == I52Id::TOP_REL_OWNED) fact(I52Id::TOP_REL_OWNED, L"top=" + ptr(tm->topTransaction()) + L";owned=" + ptr(t));
        const Acad::ErrorStatus m = mutate(plan_->mutation);
        if (m == Acad::eOk)
        {
            const Acad::ErrorStatus e = tm->endTransaction();
            fact(I52Id::SA_TX_END, L"status=" + status(e) + L";owner=" + I52Plan::text(transactionId) + L";transaction=" + ptr(t));
            if (e == Acad::eOk) { --ownedTransactions_; execution_ = nullptr; } else fact(I52Id::UNK_T_END, L"owner=" + std::wstring(I52Plan::text(transactionId)));
            return e == Acad::eOk;
        }
        const Acad::ErrorStatus a = tm->abortTransaction();
        fact(I52Id::SA_TX_ABORT, L"status=" + status(a) + L";owner=" + I52Plan::text(transactionId) + L";transaction=" + ptr(t));
        if (a == Acad::eOk) { --ownedTransactions_; execution_ = nullptr; }
        return false;
    };

    switch (exec)
    {
    case I52Id::EXEC_OBSERVE:
        break;
    case I52Id::CB_PRIMARY_01:
    {
        ok = observeWriteLock(I52Id::CB_LOCK_01);
        const bool topIsPrimary = dbtm != nullptr && primary_ != nullptr && dbtm->topTransaction() == primary_;
        fact(I52Id::T_PRIMARY, L"topIsPrimary=" + flag(topIsPrimary) + L";primary=" + ptr(primary_));
        if (!topIsPrimary) { fact(I52Id::UNK_ILLEGAL_CONTEXT, L"step=topTransaction==T-PRIMARY"); ok = false; }
        if (ok) ok = mutate(plan_->mutation) == Acad::eOk;
        break;  // T-PRIMARY ends by SET-OUTCOME-COMMIT; TOK-EXEC-DONE follows it.
    }
    case I52Id::CB_EXEC_01:
    {
        ok = observeWriteLock(I52Id::CB_LOCK_01);
        if (ok && plan_->primaryTransaction == I52Id::T_ABORTING && dbtm != nullptr && dbtm->numActiveTransactions() > 0)
        {
            fact(I52Id::UNK_NESTED_IN_ABORT, L"activeTransactions=" + std::to_wstring(dbtm->numActiveTransactions()));
            ok = false;
        }
        if (ok) ok = ownedTransaction(I52Id::T_CB);
        break;
    }
    case I52Id::EDC_EXEC_01:
    {
        ok = resolveScratchDocument(I52Id::UNK_ILLEGAL_DB_CONTEXT) != nullptr && observeWriteLock(I52Id::EDC_LOCK_01);
        if (ok) ok = ownedTransaction(I52Id::T_CANCEL);
        if (!ok) fact(I52Id::UNK_T_CANCEL_FAILURE, L"step=EDC-EXEC-01");
        break;
    }
    case I52Id::SEND_EXEC_01:
    {
        ok = resolveScratchDocument(I52Id::UNK_ILLEGAL_DB_CONTEXT) != nullptr && observeWriteLock(I52Id::SEND_LOCK_01);
        if (ok) ok = ownedTransaction(I52Id::T_SEND);
        break;
    }
    case I52Id::CMDCTX_EXEC_01:
    {
        ok = resolveScratchDocument(I52Id::UNK_ILLEGAL_DB_CONTEXT) != nullptr && observeWriteLock(I52Id::CMDCTX_LOCK_01);
        if (ok) ok = ownedTransaction(I52Id::T_CMD);
        if (!ok) fact(I52Id::UNK_T_CMD_FAILURE, L"step=CMDCTX-EXEC-01");
        break;
    }
    case I52Id::APPCTX_EXEC_01:
    {
        AcApDocument* d = acDocManager == nullptr ? nullptr : acDocManager->curDocument();
        if (d != document_) { fact(I52Id::UNK_ILLEGAL_DB_CONTEXT, L"step=APPCTX-EXEC-01 resolve D;curDocument=" + ptr(d)); ok = false; break; }
        // APPCTX-LOCK-01 (SA-LOCK): only eOk creates the unlock obligation; no mutation precedes success.
        const Acad::ErrorStatus lock = acDocManager->lockDocument(document_, AcAp::kWrite, L"RACKCAD_CTDA_V34", L"RACKCAD_CTDA_V34", false);
        fact(I52Id::SA_LOCK, L"status=" + status(lock) + L";authority=APPCTX-LOCK-01");
        fact(I52Id::APPCTX_LOCK_01, L"status=" + status(lock));
        if (lock != Acad::eOk) { fact(I52Id::UNK_LOCK_DOC_T_LEAK, L"step=APPCTX-LOCK-01;status=" + status(lock)); ok = false; break; }
        ++ownedLocks_;
        fact(I52Id::APPCTX_TX_01, L"phase=ENTRY");
        ok = ownedTransaction(I52Id::T_APP);
        // APPCTX-UNLOCK-01: the lock-mode change emitted inside this call anchors MARK-LOCK-RELEASE for the stage.
        const I52Id deliveryStage = stages_.size() >= 2 ? stages_[stages_.size() - 2].stage : I52Id::None;
        const uint64_t deliveryId = stages_.size() >= 2 ? stages_[stages_.size() - 2].delivery : 0;
        appUnlock_ = { true, false, deliveryStage, deliveryId };
        const Acad::ErrorStatus unlock = acDocManager->unlockDocument(document_);
        appUnlock_.active = false;
        fact(I52Id::SA_UNLOCK, L"status=" + status(unlock) + L";authority=APPCTX-UNLOCK-01");
        fact(I52Id::APPCTX_UNLOCK_01, L"status=" + status(unlock) + L";markerResolved=" + flag(appUnlock_.resolved));
        if (unlock == Acad::eOk) --ownedLocks_; else { fact(I52Id::UNK_LOCK_DOC_T_LEAK, L"step=APPCTX-UNLOCK-01"); ok = false; }
        break;
    }
    default:
        fact(I52Id::UNKNOWN_COMMON, L"unimplemented=" + std::wstring(I52Plan::text(exec)));
        ok = false;
        break;
    }
    fact(exec, L"phase=EXIT;ok=" + flag(ok));
    if (ok && exec != I52Id::CB_PRIMARY_01 && exec != I52Id::EXEC_OBSERVE && rowHasToken(I52Id::TOK_EXEC_DONE)) I52Ctda_TokenSetNative(I52Id::TOK_EXEC_DONE, execDelivery);
    return ok;
}

// ------------------------------------------------------------------ scheduler chains
void I52Executor::enqueueSend()
{
    fact(I52Id::SP_SEND, L"retainedSlot=" + std::to_wstring(retained_[0].slot));
    const Acad::ErrorStatus s = acDocManager->sendStringToExecute(document_, L"I52CTDA_QUEUED ", false, false, false);
    fact(I52Id::NS_SEND, L"status=" + status(s) + L";point=SP-SEND;command=I52CTDA_QUEUED");
    if (s == Acad::eOk) ++pendingSend_; else fact(I52Id::UNK_REJECTION, L"scheduler=NS-SEND;status=" + status(s));
    if (rowHasToken(I52Id::TOK_ENQUEUE_SEND)) I52Ctda_TokenSetNative(I52Id::TOK_ENQUEUE_SEND, currentDelivery());
}

void I52Executor::enqueueApplication()
{
    fact(I52Id::SP_APPCTX, L"retainedSlot=" + std::to_wstring(retained_[0].slot));
    const Acad::ErrorStatus s = acDocManager->beginExecuteInApplicationContext(&I52Executor::appDelivery, &retained_[0]);
    fact(I52Id::NS_BEGIN_APPCTX, L"status=" + status(s) + L";point=SP-APPCTX");
    if (s == Acad::eOk) ++pendingApplication_; else fact(I52Id::UNK_REJECTION, L"scheduler=NS-BEGIN-APPCTX;status=" + status(s));
    if (rowHasToken(I52Id::TOK_ENQUEUE_APPCTX)) I52Ctda_TokenSetNative(I52Id::TOK_ENQUEUE_APPCTX, currentDelivery());
}

void I52Executor::enqueueCommand(I52Id callerStage)
{
    const bool app = acDocManager->isApplicationContext();
    fact(I52Id::SP_CMDCTX, L"isApplicationContext=" + flag(app));
    if (!app) fact(I52Id::UNK_ILLEGAL_ENQUEUE, L"scheduler=NS-BEGIN-CMDCTX;isApplicationContext=0");
    const Acad::ErrorStatus s = acDocManager->beginExecuteInCommandContext(&I52Executor::cmdDelivery, &retained_[1]);
    fact(I52Id::NS_BEGIN_CMDCTX, L"status=" + status(s) + L";point=SP-CMDCTX");
    if (s == Acad::eOk) ++pendingCommand_; else fact(I52Id::UNK_REJECTION, L"scheduler=NS-BEGIN-CMDCTX;status=" + status(s));
    if (rowHasToken(I52Id::TOK_ENQUEUE_CMDCTX)) I52Ctda_TokenSetNative(I52Id::TOK_ENQUEUE_CMDCTX, I52Log::instance().current(callerStage));
}

void I52Executor::appDelivery(void* data)
{
    I52Executor& x = instance();
    const uint64_t delivery = x.push(I52Id::STG_APPCTX_DELIVERY);
    if (I52Log::instance().fenceIsSet())
    {
        x.pop(I52Id::STG_APPCTX_DELIVERY);
        x.recordLate(I52Id::STG_APPCTX_DELIVERY, L"delivery=NS-BEGIN-APPCTX;data=" + ptr(data));
        return;
    }
    --x.pendingApplication_;
    x.fact(I52Id::EP_APPCTX, L"data=" + ptr(data) + L";" + x.context());
    x.fact(I52Id::MARK_APPCTX_DELIVERY, L"isApplicationContext=" + flag(acDocManager->isApplicationContext()));
    x.fact(I52Id::CTX_APPLICATION, L"isApplicationContext=" + flag(acDocManager->isApplicationContext()));
    if (!acDocManager->isApplicationContext()) x.fact(I52Id::UNK_INVALID_APP_CONTEXT, L"stage=STG-APPCTX-DELIVERY");
    if (x.plan_->schedulerChain == I52Id::CHAIN_APPCTX_CMDCTX)
    {
        x.enqueueCommand(I52Id::STG_APPCTX_DELIVERY);
        x.fact(I52Id::MARK_APPCTX_CALLBACK_RETURN, L"stage2=NS-BEGIN-CMDCTX");
    }
    else
    {
        const uint64_t exec = x.push(I52Id::STG_EXEC);
        x.runExecution(x.plan_->executionContext, exec);
        x.pop(I52Id::STG_EXEC);
    }
    if (x.rowHasToken(I52Id::TOK_APPCTX_DELIVERY_DONE)) I52Ctda_TokenSetNative(I52Id::TOK_APPCTX_DELIVERY_DONE, delivery);
    x.pop(I52Id::STG_APPCTX_DELIVERY);
}

void I52Executor::cmdDelivery(void* data)
{
    I52Executor& x = instance();
    // The command enclosing EP-CMDCTX opened STG-CMDCTX-DELIVERY at its commandWillStart when RR-ED observes it.
    const bool opened = !x.commandStages_.empty() && x.commandStages_.back().stage == I52Id::STG_CMDCTX_DELIVERY;
    const uint64_t delivery = opened ? x.commandStages_.back().delivery : x.push(I52Id::STG_CMDCTX_DELIVERY);
    if (I52Log::instance().fenceIsSet())
    {
        if (!opened) x.pop(I52Id::STG_CMDCTX_DELIVERY);
        x.recordLate(I52Id::STG_CMDCTX_DELIVERY, L"delivery=NS-BEGIN-CMDCTX;data=" + ptr(data));
        return;
    }
    --x.pendingCommand_;
    x.fact(I52Id::EP_CMDCTX, L"data=" + ptr(data) + L";enclosingCommandObserved=" + flag(opened) + L";" + x.context());
    x.fact(I52Id::CTX_COMMAND, L"isApplicationContext=" + flag(acDocManager->isApplicationContext()));
    const uint64_t exec = x.push(I52Id::STG_EXEC);
    x.runExecution(x.plan_->executionContext, exec);
    x.pop(I52Id::STG_EXEC);
    if (x.rowHasToken(I52Id::TOK_CMDCTX_DELIVERY_DONE)) I52Ctda_TokenSetNative(I52Id::TOK_CMDCTX_DELIVERY_DONE, delivery);
    if (!opened) x.pop(I52Id::STG_CMDCTX_DELIVERY);
}

void I52Executor::syncCallback(void* data)
{
    I52Executor& x = instance();
    x.fact(I52Id::MARK_APPCTX_ENTRY, L"data=" + ptr(data) + L";isApplicationContext=" + flag(acDocManager->isApplicationContext()), I52Id::STG_SYNC_APPCTX, x.syncDelivery_);
    x.fact(I52Id::CTX_CALLBACK, L"callback=NX-APPCTX-SYNC;" + x.context(), I52Id::STG_SYNC_APPCTX, x.syncDelivery_);
    x.fact(I52Id::CTX_APPLICATION, L"isApplicationContext=" + flag(acDocManager->isApplicationContext()), I52Id::STG_SYNC_APPCTX, x.syncDelivery_);
    if (!acDocManager->isApplicationContext()) x.fact(I52Id::UNK_INVALID_APP_CONTEXT, L"stage=STG-SYNC-APPCTX");
    if (x.plan_->trigger == I52Id::TRG_APPCTX_SYNC)
    {
        const uint64_t exec = x.push(I52Id::STG_EXEC);
        x.runExecution(x.plan_->executionContext, exec);
        x.pop(I52Id::STG_EXEC);
    }
    else x.enqueueCommand(I52Id::STG_SYNC_APPCTX);
}

void I52Executor::cancelSync(void* data)
{
    I52Executor& x = instance();
    x.fact(I52Id::MARK_APPCTX_ENTRY, L"data=" + ptr(data) + L";isApplicationContext=" + flag(acDocManager->isApplicationContext()), I52Id::STG_SYNC_APPCTX, x.syncDelivery_);
    x.fact(I52Id::SP_CMDCTX, L"use=TRIGGER-INFRA;isApplicationContext=" + flag(acDocManager->isApplicationContext()));
    const Acad::ErrorStatus s = acDocManager->beginExecuteInCommandContext(&I52Executor::cancelCompletion, nullptr);
    x.fact(I52Id::NS_BEGIN_CMDCTX, L"status=" + status(s) + L";schedulerUse=TRIGGER-INFRA;credit=NONE");
    if (s == Acad::eOk) ++x.pendingInfra_; else x.fact(I52Id::UNK_REJECTION, L"scheduler=NS-BEGIN-CMDCTX;use=TRIGGER-INFRA;status=" + status(s));
}

void I52Executor::cancelCompletion(void* data)
{
    I52Executor& x = instance();
    const uint64_t delivery = I52Log::instance().current(I52Id::STG_CANCEL_CMD);
    if (I52Log::instance().fenceIsSet()) { x.recordLate(I52Id::STG_CANCEL_CMD, L"delivery=CANCEL-CMDCTX-01;data=" + ptr(data)); return; }
    --x.pendingInfra_;
    x.fact(I52Id::CANCEL_CMDCTX_01, L"phase=COMPLETION;" + x.context(), I52Id::STG_CANCEL_CMD, delivery);
    if (x.rowHasToken(I52Id::TOK_CANCEL_CMDCTX_DONE)) I52Ctda_TokenSetNative(I52Id::TOK_CANCEL_CMDCTX_DONE, delivery);
}

// ------------------------------------------------------------------ lock-release windows
void I52Executor::openCommandWindow(I52Id stage, uint64_t delivery)
{
    window_ = { true, false, stage, delivery };
    fact(I52Id::LOCK_RELEASE_BIND_01, L"phase=WINDOW-OPEN;anchor=COMMAND-END-WINDOW;stage=" + std::wstring(I52Plan::text(stage)), stage, delivery);
}

void I52Executor::closeCommandWindow(const wchar_t* reason)
{
    if (!window_.open) return;
    fact(I52Id::LOCK_RELEASE_BIND_01, std::wstring(L"phase=WINDOW-CLOSE;reason=") + reason + L";resolved=" + flag(window_.resolved), window_.stage, window_.delivery);
    window_.open = false;
}

// ------------------------------------------------------------------ observer registrations (exact retained instances)
bool I52Executor::registerObserver(I52Id registration)
{
    Registered r{};
    r.id = registration;
    Acad::ErrorStatus s = Acad::eOk;
    const I52Auth::Registration* info = I52Auth::registration(registration);
    switch (registration)
    {
    case I52Id::OR_XR: case I52Id::OR_TXR: case I52Id::OR_REF_A: case I52Id::OR_REF_B: case I52Id::OR_TEO: case I52Id::ER_REF_A:
    {
        r.notifier = fixtureObject(info == nullptr ? I52Id::None : I52Plan::parse(info->notifier));
        AcDbObject* notifier = nullptr;
        s = acdbOpenObject(notifier, r.notifier, AcDb::kForRead);
        if (s == Acad::eOk)
        {
            if (registration == I52Id::ER_REF_A) { auto* reactor = new EntityReactor(registration); r.instance = reactor; s = notifier->addReactor(reactor); }
            else { auto* reactor = new ObjectReactor(registration); r.instance = reactor; s = notifier->addReactor(reactor); }
            notifier->close();
        }
        break;
    }
    case I52Id::RR_DB: { auto* reactor = new DatabaseReactor(registration); r.instance = reactor; s = database_->addReactor(reactor); break; }
    case I52Id::RR_TX:
    {
        auto* reactor = new TransactionReactor();
        r.instance = reactor;
        AcDbTransactionManager* tm = database_->transactionManager();
        if (tm == nullptr) s = Acad::eNullPtr; else tm->addReactor(reactor);
        break;
    }
    case I52Id::RR_ED: { auto* reactor = new EditorReactor(); r.instance = reactor; acedEditor->addReactor(reactor); break; }
    case I52Id::RR_DOC: { auto* reactor = new DocumentReactor(); r.instance = reactor; acDocManager->addReactor(reactor); break; }
    case I52Id::RR_DLINK: { auto* reactor = new LinkerReactor(); r.instance = reactor; acrxDynamicLinker->addReactor(reactor); break; }
    case I52Id::RR_MANAGED_CMD:
    {
        I52CtdaManagedEntryFn entry = I52Log::instance().managedEntry();
        const int32_t result = entry == nullptr ? -1 : entry(1, reinterpret_cast<uint64_t>(document_), I52Log::instance().probeId().c_str());
        s = result == 0 ? Acad::eOk : Acad::eNotHandled;
        managedSubscribed_ = result == 0;
        fact(registration, L"phase=REGISTER;entryBound=" + flag(entry != nullptr) + L";entryResult=" + std::to_wstring(result));
        break;
    }
    case I52Id::RR_PAYLOAD_DB:
        fact(registration, L"phase=REGISTER;owner=R-PAYLOAD-ARX;registeredAt=SET-LOAD-PAYLOAD");
        return true;
    default:
        s = Acad::eNotImplementedYet;
        break;
    }
    r.attached = s == Acad::eOk;
    registered_.push_back(r);
    fact(registration, L"phase=REGISTER;status=" + status(s) + L";instance=" + ptr(r.instance) + L";notifier=" + oid(r.notifier)
        + L";reactorClass=" + (info == nullptr ? L"NONE" : info->reactorClass));
    if (s != Acad::eOk) fact(I52Id::UNKNOWN_COMMON, L"registration=" + std::wstring(I52Plan::text(registration)) + L";status=" + status(s));
    return s == Acad::eOk;
}

void I52Executor::removeObserver(Registered& r)
{
    if (r.removed || r.retainedFenced || !r.attached) return;
    Acad::ErrorStatus s = Acad::eOk;
    switch (r.id)
    {
    case I52Id::OR_XR: case I52Id::OR_TXR: case I52Id::OR_REF_A: case I52Id::OR_REF_B: case I52Id::OR_TEO: case I52Id::ER_REF_A:
    {
        // CLN-OBJ-REMOVE: open the notifier while live; only the disposable erased F-TRIGGER-ERASE-OBJ opens erased.
        const bool erased = r.notifier.isErased();
        const bool disposableEraseTrigger = r.notifier == fixture_.ids().triggerEraseObject;
        if (erased && !disposableEraseTrigger) { s = Acad::eWasErased; fact(I52Id::CLN_OBJ_REMOVE, L"registration=" + std::wstring(I52Plan::text(r.id)) + L";skipped=NOTIFIER-ERASED;neverReopened=1"); break; }
        AcDbObject* notifier = nullptr;
        s = acdbOpenObject(notifier, r.notifier, AcDb::kForRead, erased && disposableEraseTrigger);
        if (s == Acad::eOk)
        {
            s = notifier->removeReactor(static_cast<AcDbObjectReactor*>(r.instance));
            notifier->close();
        }
        break;
    }
    case I52Id::RR_DB: s = database_->removeReactor(static_cast<DatabaseReactor*>(r.instance)); break;
    case I52Id::RR_TX: { AcDbTransactionManager* tm = database_->transactionManager(); if (tm != nullptr) tm->removeReactor(static_cast<TransactionReactor*>(r.instance)); break; }
    case I52Id::RR_ED: acedEditor->removeReactor(static_cast<EditorReactor*>(r.instance)); break;
    case I52Id::RR_DOC: acDocManager->removeReactor(static_cast<DocumentReactor*>(r.instance)); break;
    case I52Id::RR_DLINK: acrxDynamicLinker->removeReactor(static_cast<LinkerReactor*>(r.instance)); break;
    case I52Id::RR_MANAGED_CMD:
    {
        I52CtdaManagedEntryFn entry = I52Log::instance().managedEntry();
        const int32_t result = entry == nullptr ? -1 : entry(0, reinterpret_cast<uint64_t>(document_), I52Log::instance().probeId().c_str());
        s = result == 0 ? Acad::eOk : Acad::eNotHandled;
        managedSubscribed_ = false;
        break;
    }
    default: s = Acad::eNotImplementedYet; break;
    }
    r.removed = s == Acad::eOk;
    // The instance is retained unreleased until process exit (the host may still hold a reference).
    fact(r.id, L"phase=REMOVE;status=" + status(s) + L";instance=" + ptr(r.instance) + L";removed=" + flag(r.removed));
}

// Called by CMD-FINISH through the veto path owner: returns the retained RR-DOC instance for SA-VETO.

const wchar_t* I52Executor::callbackMember(I52Id event) { return member(event); }
