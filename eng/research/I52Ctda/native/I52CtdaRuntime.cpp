#include "I52CtdaRuntime.h"

#include "rxregsvc.h"
#include "dbobjptr.h"

#include <Windows.h>
#include <chrono>
#include <cstring>
#include <cstdlib>
#include <fstream>
#include <iomanip>
#include <sstream>
#include <system_error>

namespace
{
std::filesystem::path environmentPath(const wchar_t* name)
{
    wchar_t buffer[32768]{};
    const DWORD length = GetEnvironmentVariableW(name, buffer, static_cast<DWORD>(_countof(buffer)));
    if (length == 0 || length >= _countof(buffer)) return {};
    return std::filesystem::path(buffer);
}

std::string jsonEscape(const std::string& value)
{
    std::ostringstream escaped;
    for (const unsigned char ch : value)
    {
        if (ch == '\\' || ch == '"') escaped << '\\' << ch;
        else if (ch < 0x20) escaped << "\\u" << std::hex << std::setw(4) << std::setfill('0') << static_cast<int>(ch) << std::dec;
        else escaped << ch;
    }
    return escaped.str();
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

// Transient reactors are attached by ObjectId with the target open for read, so removal never touches a closed pointer.
Acad::ErrorStatus I52CtdaRuntime::registerFixtureObjects(AcDbObjectId objectTarget, AcDbObjectId entityTarget)
{
    if (objectReactor_ == nullptr || entityReactor_ == nullptr) return Acad::eNullPtr;
    if (fixtureReactorsAttached()) return Acad::eInvalidInput;
    AcDbObjectPointer<AcDbObject> object(objectTarget, AcDb::kForRead);
    Acad::ErrorStatus status = object.openStatus();
    if (status == Acad::eOk) status = object->addReactor(objectReactor_);
    if (status != Acad::eOk) return status;
    objectTarget_ = objectTarget; objectReactorAttached_ = true;
    AcDbObjectPointer<AcDbEntity> entity(entityTarget, AcDb::kForRead);
    status = entity.openStatus();
    if (status == Acad::eOk) status = entity->addReactor(entityReactor_);
    if (status != Acad::eOk) return status;
    entityTarget_ = entityTarget; entityReactorAttached_ = true;
    return Acad::eOk;
}

Acad::ErrorStatus I52CtdaRuntime::unregisterFixtureObjects()
{
    Acad::ErrorStatus result = Acad::eOk;
    if (entityReactorAttached_)
    {
        AcDbObjectPointer<AcDbObject> entity(entityTarget_, AcDb::kForRead, true);
        Acad::ErrorStatus status = entity.openStatus();
        if (status == Acad::eOk) status = entity->removeReactor(entityReactor_);
        if (status == Acad::eOk) entityReactorAttached_ = false; else result = status;
    }
    if (objectReactorAttached_)
    {
        AcDbObjectPointer<AcDbObject> object(objectTarget_, AcDb::kForRead, true);
        Acad::ErrorStatus status = object.openStatus();
        if (status == Acad::eOk) status = object->removeReactor(objectReactor_);
        if (status == Acad::eOk) objectReactorAttached_ = false; else result = status;
    }
    return result;
}

void I52CtdaRuntime::unregisterReactors()
{
    unregisterFixtureObjects();
    if (database_ != nullptr) { database_->removeReactor(databaseReactor_); if (database_->transactionManager() != nullptr) database_->transactionManager()->removeReactor(transactionReactor_); }
    if (acedEditor != nullptr) acedEditor->removeReactor(editorReactor_);
    if (acDocManager != nullptr) acDocManager->removeReactor(documentReactor_);
    if (acrxDynamicLinker != nullptr) acrxDynamicLinker->removeReactor(linkerReactor_);
    // A fixture reactor that could not be detached is leaked rather than deleted: the host still references it.
    const bool fixtureDetached = !fixtureReactorsAttached();
    mark("MARK-UNLOAD-REACTORS", fixtureDetached ? "global reactors removed; fixture reactors detached" : "global reactors removed; fixture reactor detach FAILED, retained until process exit");
    delete linkerReactor_; delete documentReactor_; delete editorReactor_; delete transactionReactor_; delete databaseReactor_;
    if (fixtureDetached) { delete entityReactor_; delete objectReactor_; }
    linkerReactor_ = nullptr; documentReactor_ = nullptr; editorReactor_ = nullptr; transactionReactor_ = nullptr; entityReactor_ = nullptr; objectReactor_ = nullptr; databaseReactor_ = nullptr;
    database_ = nullptr; objectTarget_ = AcDbObjectId::kNull; entityTarget_ = AcDbObjectId::kNull;
}

std::filesystem::path I52CtdaRuntime::eventLogPath() const
{
    const auto explicitPath = environmentPath(L"I52_CTDA_EVENT_LOG");
    if (!explicitPath.empty()) return explicitPath;
    const auto output = environmentPath(L"I52_CTDA_OUTPUT");
    if (output.empty()) return {};
    return std::filesystem::path(output.native() + kEventLogSuffix);
}

bool I52CtdaRuntime::loggerReady() const
{
    const auto path = eventLogPath();
    if (path.empty()) return false;
    std::error_code error;
    if (path.has_parent_path()) std::filesystem::create_directories(path.parent_path(), error);
    std::ofstream stream(path, std::ios::binary | std::ios::app);
    return stream.good();
}

void I52CtdaRuntime::setCommandIdentity(const std::string& command) { std::scoped_lock lock(logMutex_); commandIdentity_ = command; }

void I52CtdaRuntime::writeRecord(const char* eventId, const std::string& notifier, int depth, const std::string& detail)
{
    const auto path = eventLogPath();
    if (path.empty()) return;
    AcApDocument* document = acDocManager == nullptr ? nullptr : acDocManager->curDocument();
    AcDbDatabase* database = document == nullptr ? nullptr : document->database();
    const bool applicationContext = acDocManager != nullptr && acDocManager->isApplicationContext();
    std::scoped_lock lock(logMutex_);
    const unsigned long long sequence = ++sequence_;
    const auto micros = std::chrono::duration_cast<std::chrono::microseconds>(std::chrono::system_clock::now().time_since_epoch()).count();
    std::ofstream stream(path, std::ios::binary | std::ios::app);
    stream << "{\"sequence\":" << sequence << ",\"timestampUnixMicros\":" << micros
           << ",\"processId\":" << GetCurrentProcessId() << ",\"threadId\":" << GetCurrentThreadId()
           << ",\"eventId\":\"" << jsonEscape(eventId) << "\",\"notifier\":\"" << jsonEscape(notifier)
           << "\",\"transactionDepth\":" << depth
           << ",\"documentIdentity\":\"" << static_cast<const void*>(document) << "\",\"databaseIdentity\":\"" << static_cast<const void*>(database)
           << "\",\"applicationContext\":" << (applicationContext ? "true" : "false")
           << ",\"commandIdentity\":\"" << jsonEscape(commandIdentity_) << "\",\"detail\":\"" << jsonEscape(detail) << "\"}\n";
}

void I52CtdaRuntime::onEvent(const char* eventId, const void* notifier, int depth)
{
    std::ostringstream text;
    text << notifier;
    writeRecord(eventId, text.str(), depth, {});
}

void I52CtdaRuntime::mark(const char* markerId, const std::string& detail) { writeRecord(markerId, {}, -1, detail); }

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
    ++governedDispatches_;
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
size_t I52CtdaRuntime::activeGuards() const { std::scoped_lock lock(mutex_); return guards_.size(); }
