#include "I52CtdaRuntime.h"

#include "rxregsvc.h"

#include <Windows.h>
#include <chrono>
#include <cstring>
#include <cstdlib>
#include <filesystem>
#include <fstream>

namespace
{
void writeEvent(unsigned long long sequence, const char* eventId, const void* notifier, int depth)
{
    wchar_t pathBuffer[32768]{};
    const DWORD length = GetEnvironmentVariableW(L"I52_CTDA_EVENT_LOG", pathBuffer, static_cast<DWORD>(_countof(pathBuffer)));
    if (length == 0 || length >= _countof(pathBuffer)) return;
    const auto micros = std::chrono::duration_cast<std::chrono::microseconds>(std::chrono::system_clock::now().time_since_epoch()).count();
    std::ofstream stream(std::filesystem::path(pathBuffer), std::ios::binary | std::ios::app);
    stream << "{\"sequence\":" << sequence << ",\"timestampUnixMicros\":" << micros
           << ",\"processId\":" << GetCurrentProcessId() << ",\"threadId\":" << GetCurrentThreadId()
           << ",\"eventId\":\"" << eventId << "\",\"notifier\":\"" << notifier
           << "\",\"transactionDepth\":" << depth << "}\n";
}
}

class I52CtdaRuntime::DatabaseReactor final : public AcDbDatabaseReactor
{
    void objectAppended(const AcDbDatabase*, const AcDbObject* o) override { instance().onEvent("N-DB-APPEND", o); }
    void objectErased(const AcDbDatabase*, const AcDbObject* o, bool) override { instance().onEvent("N-DB-ERASE", o); }
    void objectModified(const AcDbDatabase*, const AcDbObject* o) override { instance().onEvent("N-DB-MOD", o); }
    void objectOpenedForModify(const AcDbDatabase*, const AcDbObject* o) override { instance().onEvent("N-DB-OPEN", o); }
};

class I52CtdaRuntime::ObjectReactor final : public AcDbObjectReactor
{
    void cancelled(const AcDbObject* o) override { instance().onEvent("N-OBJ-CANCEL", o); }
    void objectClosed(const AcDbObjectId id) override { instance().onEvent("N-OBJ-CLOSED", reinterpret_cast<const void*>(id.asOldId())); }
    void erased(const AcDbObject* o, bool) override { instance().onEvent("N-OBJ-ERASE", o); }
    void modified(const AcDbObject* o) override { instance().onEvent("N-OBJ-MOD", o); }
    void openedForModify(const AcDbObject* o) override { instance().onEvent("N-OBJ-OPEN", o); }
    void modifyUndone(const AcDbObject* o) override { instance().onEvent("N-OBJ-UNDO", o); }
};

class I52CtdaRuntime::EntityReactor final : public AcDbEntityReactor
{
    void modifiedGraphics(const AcDbEntity* e) override { instance().onEvent("N-ENT-GFX", e); }
};

class I52CtdaRuntime::TransactionReactor final : public AcTransactionReactor
{
    void transactionAboutToStart(int& n, AcDbTransactionManager* m) override { instance().onEvent("N-TR-ABOUT-START", m, n); }
    void transactionStarted(int& n, AcDbTransactionManager* m) override { instance().onEvent("N-TR-STARTED", m, n); }
    void transactionAboutToEnd(int& n, AcDbTransactionManager* m) override { instance().onEvent("N-TR-ABOUT-END", m, n); }
    void transactionEnded(int& n, AcDbTransactionManager* m) override { instance().onEvent("N-TR-ENDED", m, n); }
    void transactionAboutToAbort(int& n, AcDbTransactionManager* m) override { instance().onEvent("N-TR-ABOUT-ABORT", m, n); }
    void transactionAborted(int& n, AcDbTransactionManager* m) override { instance().onEvent("N-TR-ABORTED", m, n); }
    void endCalledOnOutermostTransaction(int& n, AcDbTransactionManager* m) override { instance().onEvent("N-TR-OUTERMOST-END-CALLED", m, n); }
};

class I52CtdaRuntime::EditorReactor final : public AcEditorReactor
{
    void commandWillStart(const ACHAR* c) override { instance().onEvent("N-ED-WILL", c); }
    void commandEnded(const ACHAR* c) override { instance().onEvent("N-ED-END", c); }
    void commandCancelled(const ACHAR* c) override { instance().onEvent("N-ED-CANCEL", c); }
};

class I52CtdaRuntime::DocumentReactor final : public AcApDocManagerReactor
{
public:
    Acad::ErrorStatus requestVeto() { return veto(); }
private:
    void documentLockModeWillChange(AcApDocument* d, AcAp::DocLockMode, AcAp::DocLockMode, AcAp::DocLockMode, const ACHAR*) override { instance().onEvent("N-DOC-LOCK-WILL", d); }
    void documentLockModeChangeVetoed(AcApDocument* d, const ACHAR*) override { instance().onEvent("N-DOC-LOCK-VETO", d); }
    void documentLockModeChanged(AcApDocument* d, AcAp::DocLockMode, AcAp::DocLockMode, AcAp::DocLockMode, const ACHAR*) override { instance().onEvent("N-DOC-LOCK-CHANGED", d); }
};

class I52CtdaRuntime::LinkerReactor final : public AcRxDLinkerReactor
{
    void rxAppWillBeLoaded(const ACHAR* n) override { instance().onEvent("N-RX-WILL-LOAD", n); }
    void rxAppLoaded(const ACHAR* n) override { instance().onEvent("N-RX-LOADED", n); }
};

I52CtdaRuntime& I52CtdaRuntime::instance() { static I52CtdaRuntime value; return value; }
I52CtdaRuntime::I52CtdaRuntime() = default;
I52CtdaRuntime::~I52CtdaRuntime() = default;

void I52CtdaRuntime::registerReactors()
{
    AcApDocument* document = acDocManager == nullptr ? nullptr : acDocManager->curDocument();
    database_ = document == nullptr ? nullptr : document->database();
    databaseReactor_ = new DatabaseReactor(); objectReactor_ = new ObjectReactor(); entityReactor_ = new EntityReactor();
    transactionReactor_ = new TransactionReactor(); editorReactor_ = new EditorReactor(); documentReactor_ = new DocumentReactor(); linkerReactor_ = new LinkerReactor();
    if (database_ != nullptr) { database_->addReactor(databaseReactor_); if (database_->transactionManager() != nullptr) database_->transactionManager()->addReactor(transactionReactor_); }
    if (acedEditor != nullptr) acedEditor->addReactor(editorReactor_);
    if (acDocManager != nullptr) acDocManager->addReactor(documentReactor_);
    if (acrxDynamicLinker != nullptr) acrxDynamicLinker->addReactor(linkerReactor_);
}

void I52CtdaRuntime::registerFixtureObjects(AcDbObject* objectTarget, AcDbEntity* entityTarget)
{
    objectTarget_ = objectTarget; entityTarget_ = entityTarget;
    if (objectTarget_ != nullptr) objectTarget_->addReactor(objectReactor_);
    if (entityTarget_ != nullptr) entityTarget_->addReactor(entityReactor_);
}

void I52CtdaRuntime::unregisterReactors()
{
    if (entityTarget_ != nullptr) entityTarget_->removeReactor(entityReactor_);
    if (objectTarget_ != nullptr) objectTarget_->removeReactor(objectReactor_);
    if (database_ != nullptr) { database_->removeReactor(databaseReactor_); if (database_->transactionManager() != nullptr) database_->transactionManager()->removeReactor(transactionReactor_); }
    if (acedEditor != nullptr) acedEditor->removeReactor(editorReactor_);
    if (acDocManager != nullptr) acDocManager->removeReactor(documentReactor_);
    if (acrxDynamicLinker != nullptr) acrxDynamicLinker->removeReactor(linkerReactor_);
    delete linkerReactor_; delete documentReactor_; delete editorReactor_; delete transactionReactor_; delete entityReactor_; delete objectReactor_; delete databaseReactor_;
    linkerReactor_ = nullptr; documentReactor_ = nullptr; editorReactor_ = nullptr; transactionReactor_ = nullptr; entityReactor_ = nullptr; objectReactor_ = nullptr; databaseReactor_ = nullptr;
    database_ = nullptr; objectTarget_ = nullptr; entityTarget_ = nullptr;
}

void I52CtdaRuntime::onEvent(const char* eventId, const void* notifier, int depth) { writeEvent(++sequence_, eventId, notifier, depth); }

#define I52_PROBE(id, handler, authority, origin) static void handler() { I52CtdaRuntime::instance().dispatchProbe(id, authority, origin); }
#include "ProbeDispatchTable.inc"
#undef I52_PROBE

struct ProbeDispatchEntry { const char* id; void (*handler)(); const char* authority; const char* origin; };
static const ProbeDispatchEntry kProbeDispatchTable[] = {
#define I52_PROBE(id, handler, authority, origin) { id, &handler, authority, origin },
#include "ProbeDispatchTable.inc"
#undef I52_PROBE
};

void I52CtdaRuntime::dispatchProbe(const char* probeId, const char* authorityId, const char* originEventId)
{
    bool registered = false;
    for (const auto& entry : kProbeDispatchTable) if (std::strcmp(entry.id, probeId) == 0) { registered = true; break; }
    if (!registered) return;
    onEvent(authorityId, probeId);
    if (originEventId != nullptr && *originEventId != '\0' && std::string(originEventId) != "NONE") onEvent(originEventId, probeId);
}
Acad::ErrorStatus I52CtdaRuntime::scheduleSend(AcApDocument* d, const ACHAR* c) { ++queuedWork_; const auto s = acDocManager->sendStringToExecute(d, c, true, false, false); if (s != Acad::eOk) --queuedWork_; return s; }
Acad::ErrorStatus I52CtdaRuntime::scheduleCommandContext(void (*c)(void*), void* d) { ++queuedWork_; const auto s = acDocManager->beginExecuteInCommandContext(c, d); if (s != Acad::eOk) --queuedWork_; return s; }
Acad::ErrorStatus I52CtdaRuntime::scheduleApplicationContext(void (*c)(void*), void* d) { ++queuedWork_; const auto s = acDocManager->beginExecuteInApplicationContext(c, d); if (s != Acad::eOk) --queuedWork_; return s; }
void I52CtdaRuntime::executeApplicationContext(void (*c)(void*), void* d) { acDocManager->executeInApplicationContext(c, d); }
Acad::ErrorStatus I52CtdaRuntime::vetoCurrentLockChange() { return documentReactor_->requestVeto(); }
Acad::ErrorStatus I52CtdaRuntime::lockDocument(AcApDocument* d) { const auto s = acDocManager->lockDocument(d, AcAp::kWrite, L"RACKCAD_CTDA_V34", L"RACKCAD_CTDA_V34", false); if (s == Acad::eOk) ++ownedLocks_; return s; }
Acad::ErrorStatus I52CtdaRuntime::unlockDocument(AcApDocument* d) { const auto s = acDocManager->unlockDocument(d); if (s == Acad::eOk) --ownedLocks_; return s; }
AcTransactionManager* I52CtdaRuntime::resolveTransactionManager(AcApDocument* d) { return d == nullptr ? nullptr : d->transactionManager(); }
AcTransaction* I52CtdaRuntime::startTransaction(AcTransactionManager* m) { AcTransaction* t = m == nullptr ? nullptr : m->startTransaction(); if (t != nullptr) ++ownedTransactions_; return t; }
Acad::ErrorStatus I52CtdaRuntime::endTransaction(AcTransactionManager* m) { const auto s = m == nullptr ? Acad::eNullPtr : m->endTransaction(); if (s == Acad::eOk) --ownedTransactions_; return s; }
Acad::ErrorStatus I52CtdaRuntime::abortTransaction(AcTransactionManager* m) { const auto s = m == nullptr ? Acad::eNullPtr : m->abortTransaction(); if (s == Acad::eOk) --ownedTransactions_; return s; }
bool I52CtdaRuntime::armGuard(const char* id) { std::scoped_lock lock(mutex_); return guards_.insert(id).second; }
void I52CtdaRuntime::disarmGuard(const char* id) { std::scoped_lock lock(mutex_); guards_.erase(id); }
bool I52CtdaRuntime::cleanupComplete() const { std::scoped_lock lock(mutex_); return ownedTransactions_ == 0 && ownedLocks_ == 0 && queuedWork_ == 0 && guards_.empty(); }
