#pragma once

#include "I52CtdaFixture.h"
#include "I52CtdaLog.h"
#include "I52CtdaPlan.h"

#include "acdocman.h"
#include "dbmain.h"

#include <map>
#include <memory>
#include <string>
#include <vector>

class AcTransaction;

// R-NATIVE-ARX governed executor: interprets exactly one immutable V35 plan per process (RUN-ENV-01). Every step
// switches over generated identifiers; anything a plan names that is not implemented is recorded and is UNKNOWN.
class I52Executor final
{
public:
    static I52Executor& instance();

    void load();
    void unload();

    // CommandResource bodies (CMD-BOOT, CMD-PROBE, CMD-FIXTURE, CMD-CANCEL, CMD-QUEUED, CMD-FINISH) and the V35 smoke.
    void boot();
    void probe();
    void fixtureCommand();
    void cancelCommand();
    void queuedCommand();
    void finish();
    void smoke();

    // Reactor entry points. `instance` is the exact retained reactor instance of the registration.
    void onDatabase(I52Id registration, const void* instance, I52Id event, const AcDbObject* object, bool erasing);
    void onObject(I52Id registration, const void* instance, I52Id event, const AcDbObject* object, AcDbObjectId closedId, bool erasing);
    void onTransaction(const void* instance, I52Id event, int numTransactions, AcDbTransactionManager* manager);
    void onEditor(const void* instance, I52Id event, const ACHAR* command);
    void onDocument(const void* instance, I52Id event, AcApDocument* document, int myCurrent, int myNew, int current, const ACHAR* globalCommand);
    void onLinker(const void* instance, I52Id event, const ACHAR* module);
    void onIdle();

    // Scheduler and context deliveries (static thunks receive the retained data slot).
    static void appDelivery(void* data);
    static void cmdDelivery(void* data);
    static void syncCallback(void* data);
    static void driverApp(void* data);
    static void cancelSync(void* data);
    static void cancelCompletion(void* data);

    struct StageFrame { I52Id stage; uint64_t delivery; };
    struct GuardState
    {
        I52Id guard{};
        std::wstring family;
        std::map<std::wstring, std::wstring> target;
        bool armed{};
        bool consumed{};
        bool handedOver{};
    };
    struct Registered
    {
        I52Id id{};
        void* instance{};
        AcDbObjectId notifier;
        bool attached{};
        bool removed{};
        bool retainedFenced{};
    };
    struct Retained
    {
        int slot{};
        I52Id stage2{};
        bool released{};
    };

private:
    I52Executor() = default;

    // Recording (LOG-RECORD-01 through LOG-SEQ-01).
    uint64_t fact(I52Id event, const std::wstring& payload = {}, I52Id stage = I52Id::None, uint64_t delivery = 0, I52Id driverOrScheduler = I52Id::None);
    I52Id currentStage() const;
    uint64_t currentDelivery() const;
    uint64_t push(I52Id stage);
    void pop(I52Id stage);
    bool recording() const { return recording_; }
    std::wstring context() const;

    // Plan helpers.
    bool rowHas(const I52Id* ids, size_t n, I52Id id) const { return I52Plan::contains(ids, n, id); }
    bool rowHasSetup(I52Id id) const;
    bool rowHasRegistration(I52Id id) const;
    bool rowHasObservation(I52Id id) const;
    bool rowHasToken(I52Id id) const;
    I52Id bodyEvent() const;
    I52Id deliveryScheduler(I52Id stage) const;
    I52Id triggerStage() const;
    std::wstring boundCommand() const;
    AcDbObjectId fixtureObject(I52Id identity) const;

    // Driver phases.
    bool registerObserver(I52Id registration);
    bool registerFinishGate();
    bool runSetup();
    bool runSetupAction(I52Id setup);
    void armGuards(bool includeAppend);
    void disarmGuards(const wchar_t* phase);
    bool runTrigger();
    void runDriverPhase(const std::wstring& phase);
    void postTriggerObligations();
    bool loadPayload();
    void armAppendGuard(const void* instancePointer);
    void recordPhase(const wchar_t* phase, const std::wstring& detail = {});

    // Guards (keySchema).
    std::map<std::wstring, std::wstring> callbackKey(I52Id guard, const std::wstring& family, I52Id event, const void* instance, AcDbObjectId target, const std::wstring& extra) const;
    GuardState* guardFor(I52Id event);
    enum class Admission { Observe, RunBody, Suppressed, Recursion, Late };
    Admission admit(I52Id registration, const void* instance, I52Id event, AcDbObjectId target, const std::wstring& extra);

    // Bodies, chains and execution contexts.
    void runBody(I52Id registration, I52Id event, const std::wstring& evidence);
    bool runExecution(I52Id exec, uint64_t execDelivery);
    Acad::ErrorStatus mutate(I52Id mutation);
    bool observeWriteLock(I52Id lockAuthority);
    AcApDocument* resolveScratchDocument(I52Id unknownIfMissing);
    void enqueueSend();
    void enqueueApplication();
    void enqueueCommand(I52Id callerStage);
    void recordLate(I52Id stage, const std::wstring& detail);
    void recordCallbackEvidence(I52Id event);
    void observationEvidence(I52Id event);
    bool removedOrRetained(I52Id registration, const void* instance, I52Id event);
    static const wchar_t* callbackMember(I52Id event);

    // Lock-release windows (LOCK-RELEASE-BIND-01).
    void openCommandWindow(I52Id stage, uint64_t delivery);
    void closeCommandWindow(const wchar_t* reason);

    // FIN-GATE-01 and CMD-FINISH.
    void issueFinish(bool drain);
    void runVerifiers();
    void runCleanup(bool drain);
    bool runCleanupStep(I52Id step, bool drain);
    bool cleanupBase();
    void removeObserver(Registered& registered);

    const I52RowPlan* plan_{};
    I52CtdaFixture fixture_;
    bool loaded_{};
    bool booted_{};
    bool bootFailed_{};
    std::wstring bootFacts_;
    std::wstring bootSnapshot_;
    bool recording_{};
    bool aborted_{};
    std::wstring abortReason_;
    AcApDocument* document_{};
    AcDbDatabase* database_{};
    std::vector<StageFrame> stages_;
    std::vector<Registered> registered_;
    std::vector<GuardState> guards_;
    bool inBody_{};
    I52Id bodyStage_{};
    std::vector<std::wstring> commands_;

    // Driver-owned transactions (T-PRIMARY, T-NESTED, T-CONTROLLED) and owned obligations.
    AcTransaction* primary_{};
    AcTransaction* nested_{};
    AcTransaction* controlled_{};
    int ownedLocks_{};
    int ownedTransactions_{};
    bool outcomeCommitted_{};
    bool execOk_{};
    AcTransaction* execution_{};
    bool triggerBegun_{};
    bool evidenceRecorded_{};

    // Scheduler state.
    Retained retained_[2]{};
    int pendingSend_{};
    int pendingApplication_{};
    int pendingCommand_{};
    int pendingInfra_{};
    uint64_t syncDelivery_{};
    uint64_t execDelivery_{};

    // Command windows.
    struct CommandStage { std::wstring command; I52Id stage; uint64_t delivery; };
    std::vector<CommandStage> commandStages_;
    struct Window { bool open{}; bool resolved{}; I52Id stage{}; uint64_t delivery{}; } window_;
    struct AppUnlock { bool active{}; bool resolved{}; I52Id stage{}; uint64_t delivery{}; } appUnlock_;

    // Document lock requests (TRG-LOCK-CYCLE / TRG-LOCK-VETO).
    int lockRequest_{};
    bool lockInFlight_{};
    bool vetoArmed_{};
    bool vetoIssued_{};
    Acad::ErrorStatus vetoStatus_{Acad::eOk};

    // Payload (R-PAYLOAD-ARX) and managed observer (R-MANAGED-OBSERVER).
    std::wstring payloadPath_;
    bool payloadLoaded_{};
    bool managedSubscribed_{};

    // FIN-GATE-01 / CMD-FINISH.
    bool gateRegistered_{};
    uintptr_t gateTimer_{};
    uint64_t gateDelivery_{};
    unsigned long long probeReturnTick_{};
    bool finishIssued_{};
    bool finishing_{};
    bool selfAction_{};
    bool drainMode_{};
    uint64_t finishDelivery_{};
    std::wstring lastGateState_{L"<INITIAL>"};
};
