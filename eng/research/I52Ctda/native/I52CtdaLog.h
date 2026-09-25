#pragma once

#include "I52CtdaAbi.h"
#include "I52CtdaPlan.h"

#include <cstdint>
#include <map>
#include <string>
#include <vector>

// LOG-SEQ-01 owner inside R-NATIVE-ARX: the single process-wide log, the single sequencer, the stage activation
// table (DeliveryId), the completion-token table, the FINISH-FENCE-01 flag and the C15 arm record.
class I52Log final
{
public:
    static I52Log& instance();

    // Paths and ProbeId come from the control plane (RUN-ENV-01): I52_CTDA_EVENT_LOG, I52_CTDA_PROBE_ID.
    void configure();
    bool ready() const { return ready_; }
    const std::wstring& probeId() const { return probeId_; }
    const std::wstring& logPath() const { return path_; }

    uint64_t append(const I52CtdaRecord& record);
    int countFrom(const std::wstring& module) const;
    int countOf(const std::wstring& module, const std::wstring& event) const;
    uint64_t lastSequence() const;

    // Stage activations. open() starts a new activation (monotonic DeliveryId) and makes it current for the stage.
    uint64_t open(I52Id stage);
    uint64_t current(I52Id stage) const;
    uint64_t currentForToken(I52Id token) const;
    void setCommandIdentity(const std::wstring& command);
    std::wstring commandIdentity() const;
    I52Id stageOf(uint64_t deliveryId) const;

    // Completion tokens (17 CompletionToken ids). A token is accepted only once, only for the running ProbeId's plan
    // and only with a DeliveryId of an activation of the token's own stage (TOK-LOCK-RELEASE: a lock-release stage).
    int32_t setToken(I52Id token, uint64_t deliveryId, std::wstring& reason);
    bool tokenSet(I52Id token) const;
    size_t tokenCount() const;
    std::vector<I52Id> missingTokens(const I52RowPlan& plan) const;

    // FINISH-FENCE-01. setFence() is internal (CMD-FINISH only); fenceIsSet() backs the read export.
    int32_t fenceIsSet() const;

    // C15 handshake state written by SET-LOAD-PAYLOAD; read side-effect free by I52Ctda_QueryC15Arm.
    struct C15Arm { bool armed{}; uint64_t databaseId{}; uint64_t triggerObjectOldId{}; uint64_t retainedDataId{}; };
    void setC15Arm(const C15Arm& arm);
    int32_t queryC15Arm(I52CtdaC15Arm* answer) const;

    void setPlan(const I52RowPlan* plan) { plan_ = plan; }
    typedef void (*StageProvider)(I52Id& stage, uint64_t& delivery);
    void setStageProvider(StageProvider provider) { stageProvider_ = provider; }
    const I52RowPlan* plan() const { return plan_; }
    void setScratch(uint64_t documentId, uint64_t databaseId) { scratchDocument_ = documentId; scratchDatabase_ = databaseId; }
    uint64_t scratchDocument() const { return scratchDocument_; }
    uint64_t scratchDatabase() const { return scratchDatabase_; }

    I52CtdaManagedEntryFn managedEntry() const { return managedEntry_; }
    void setManagedEntry(I52CtdaManagedEntryFn entry) { managedEntry_ = entry; }

private:
    friend class I52FinishFence;
    I52Log();
    I52Log(const I52Log&) = delete;
    I52Log& operator=(const I52Log&) = delete;
    void setFence();

    bool ready_{};
    std::wstring path_;
    std::wstring probeId_;
    void* file_{};
    mutable void* section_{};
    volatile int64_t sequence_{};
    uint64_t deliveryCounter_{};
    std::map<I52Id, uint64_t> currentActivation_;
    std::map<uint64_t, I52Id> activationStage_;
    std::map<I52Id, uint64_t> tokens_;
    volatile long fence_{};
    C15Arm c15_{};
    const I52RowPlan* plan_{};
    std::wstring commandIdentity_{L"NONE"};
    std::map<std::wstring, int> moduleRecords_;
    std::map<std::wstring, int> moduleEventRecords_;
    uint64_t scratchDocument_{};
    uint64_t scratchDatabase_{};
    I52CtdaManagedEntryFn managedEntry_{};
    StageProvider stageProvider_{};
};

// The only holder of the fence setter; CMD-FINISH is its only caller.
class I52FinishFence final
{
public:
    static void set() { I52Log::instance().setFence(); }
};

// Native convenience: one record of R-NATIVE-ARX with the current or given activation.
struct I52Fact
{
    I52Id event;
    I52Id stage;
    uint64_t delivery;
    std::wstring payload;
};

std::wstring I52Hex(uint64_t value);

// Native-side token setter: routes through the exported I52Ctda_TokenSet so every token takes the same path.
int32_t I52Ctda_TokenSetNative(I52Id token, uint64_t deliveryId);

extern "C" __declspec(dllexport) uint64_t I52Ctda_LogAppend(const I52CtdaRecord* record);
extern "C" __declspec(dllexport) int32_t I52Ctda_TokenSet(const wchar_t* tokenId, uint64_t deliveryId);
extern "C" __declspec(dllexport) int32_t I52Ctda_QueryC15Arm(I52CtdaC15Arm* arm);
extern "C" __declspec(dllexport) int32_t I52Ctda_FinishFenceIsSet(void);
std::wstring I52Narrow(const std::string& value);
