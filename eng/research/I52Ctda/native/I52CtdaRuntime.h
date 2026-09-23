#pragma once

#include "aced.h"
#include "acdocman.h"
#include "actrans.h"
#include "dbmain.h"
#include "dbtrans.h"
#include "rxdlinkr.h"

#include <atomic>
#include <mutex>
#include <string>
#include <unordered_set>

class I52CtdaRuntime final
{
public:
    static I52CtdaRuntime& instance();
    void registerReactors();
    void unregisterReactors();
    void registerFixtureObjects(AcDbObject* objectTarget, AcDbEntity* entityTarget);
    void onEvent(const char* eventId, const void* notifier, int transactionDepth = -1);
    void dispatchProbe(const char* probeId, const char* authorityId, const char* originEventId);

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

private:
    I52CtdaRuntime();
    ~I52CtdaRuntime();
    I52CtdaRuntime(const I52CtdaRuntime&) = delete;
    I52CtdaRuntime& operator=(const I52CtdaRuntime&) = delete;

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
    AcDbObject* objectTarget_{};
    AcDbEntity* entityTarget_{};
    std::atomic<unsigned long long> sequence_{0};
    mutable std::mutex mutex_;
    std::unordered_set<std::string> guards_;
    int ownedTransactions_{};
    int ownedLocks_{};
    int queuedWork_{};
};
