#pragma once

#include "aced.h"
#include "acdocman.h"
#include "actrans.h"
#include "dbmain.h"
#include "dbtrans.h"
#include "rxdlinkr.h"

#include <atomic>
#include <filesystem>
#include <mutex>
#include <string>
#include <unordered_set>

class I52CtdaRuntime final
{
public:
    // Appended to I52_CTDA_OUTPUT when I52_CTDA_EVENT_LOG is not set; the harness derives the same path.
    static constexpr const wchar_t* kEventLogSuffix = L".events.jsonl";

    static I52CtdaRuntime& instance();
    void registerReactors();
    void unregisterReactors();
    Acad::ErrorStatus registerFixtureObjects(AcDbObjectId objectTarget, AcDbObjectId entityTarget);
    Acad::ErrorStatus unregisterFixtureObjects();
    bool fixtureReactorsAttached() const { return objectReactorAttached_ || entityReactorAttached_; }
    void onEvent(const char* eventId, const void* notifier, int transactionDepth = -1);
    void mark(const char* markerId, const std::string& detail);
    void dispatchProbe(const char* probeId, const char* authorityId, const char* originEventId);

    std::filesystem::path eventLogPath() const;
    bool loggerReady() const;
    void setCommandIdentity(const std::string& command);
    unsigned long long lastSequence() const { return sequence_.load(); }
    int governedDispatches() const { return governedDispatches_.load(); }

    Acad::ErrorStatus scheduleSend(AcApDocument* document, const ACHAR* command);
    Acad::ErrorStatus scheduleCommandContext(void (*callback)(void*), void* data);
    Acad::ErrorStatus scheduleApplicationContext(void (*callback)(void*), void* data);
    void executeApplicationContext(void (*callback)(void*), void* data);

    Acad::ErrorStatus vetoCurrentLockChange();
    Acad::ErrorStatus lockDocument(AcApDocument* document);
    Acad::ErrorStatus unlockDocument(AcApDocument* document);
    AcTransactionManager* resolveTransactionManager(AcApDocument* document);
    AcTransaction* startTransaction(AcTransactionManager* manager);
    Acad::ErrorStatus endTransaction(AcTransactionManager* manager);
    Acad::ErrorStatus abortTransaction(AcTransactionManager* manager);

    bool armGuard(const char* guardId);
    void disarmGuard(const char* guardId);
    bool cleanupComplete() const;
    int ownedTransactions() const { return ownedTransactions_; }
    int ownedLocks() const { return ownedLocks_; }
    int queuedWork() const { return queuedWork_; }
    size_t activeGuards() const;

private:
    I52CtdaRuntime();
    ~I52CtdaRuntime();
    I52CtdaRuntime(const I52CtdaRuntime&) = delete;
    I52CtdaRuntime& operator=(const I52CtdaRuntime&) = delete;
    void writeRecord(const char* eventId, const std::string& notifier, int depth, const std::string& detail);

    class DatabaseReactor;
    class ObjectReactor;
    class EntityReactor;
    class TransactionReactor;
    class EditorReactor;
    class DocumentReactor;
    class LinkerReactor;

    DatabaseReactor* databaseReactor_{};
    ObjectReactor* objectReactor_{};
    EntityReactor* entityReactor_{};
    TransactionReactor* transactionReactor_{};
    EditorReactor* editorReactor_{};
    DocumentReactor* documentReactor_{};
    LinkerReactor* linkerReactor_{};
    AcDbDatabase* database_{};
    AcDbObjectId objectTarget_;
    AcDbObjectId entityTarget_;
    bool objectReactorAttached_{};
    bool entityReactorAttached_{};
    std::atomic<unsigned long long> sequence_{0};
    std::atomic<int> governedDispatches_{0};
    mutable std::mutex mutex_;
    mutable std::mutex logMutex_;
    std::string commandIdentity_;
    std::unordered_set<std::string> guards_;
    int ownedTransactions_{};
    int ownedLocks_{};
    int queuedWork_{};
};
