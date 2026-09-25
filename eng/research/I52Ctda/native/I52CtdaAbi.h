#pragma once

// LOG-SEQ-01 C ABI of R-NATIVE-ARX (EXEC-CATALOG-V35-1, V35 freeze 43DCE809...B6DF). Shared by R-PAYLOAD-ARX
// (GetModuleHandleW + GetProcAddress) and mirrored by R-MANAGED-OBSERVER ([DllImport("I52CtdaNative.arx")]).
// Exports: I52Ctda_LogAppend, I52Ctda_TokenSet, I52Ctda_QueryC15Arm, I52Ctda_FinishFenceIsSet. The FINISH-FENCE-01
// setter is internal to R-NATIVE-ARX and is not exported.

#include <stdint.h>

#ifdef __cplusplus
extern "C" {
#endif

// DeliveryId conventions for records written by a module that does not own the stage activation table:
// I52CTDA_DELIVERY_CURRENT stamps the current activation of StageId; I52CTDA_DELIVERY_NEW opens a new activation.
#define I52CTDA_DELIVERY_CURRENT 0ull
#define I52CTDA_DELIVERY_NEW 0xFFFFFFFFFFFFFFFFull
// StageId I52CTDA_STAGE_CURRENT asks the sequencer to stamp the innermost active stage of R-NATIVE-ARX (the owner of
// the stage table) together with its current DeliveryId; used for observation records of foreign modules.
#define I52CTDA_STAGE_CURRENT L"*"
// CommandIdentity I52CTDA_COMMAND_CURRENT asks the sequencer to stamp the command identity tracked by R-NATIVE-ARX.
#define I52CTDA_COMMAND_CURRENT L"*"

// One LOG-RECORD-01 record as supplied by a module. Sequence, PID, TID and the timestamp are assigned by the
// sequencer; every other field is mandatory. Payload is "key=value" pairs separated by ';' (keys and values never
// contain ';' or '='). Every identifier field holds an EXEC-CATALOG-V35-1 identifier.
typedef struct I52CtdaRecord
{
    uint32_t size;                      // sizeof(I52CtdaRecord)
    const wchar_t* probeId;
    const wchar_t* stageId;
    uint64_t deliveryId;
    const wchar_t* driverOrSchedulerId;
    const wchar_t* moduleId;            // R-NATIVE-ARX, R-PAYLOAD-ARX or R-MANAGED-OBSERVER
    uint64_t documentId;
    uint64_t databaseId;
    const wchar_t* commandIdentity;
    const wchar_t* eventOrMarkerId;
    const wchar_t* payload;
} I52CtdaRecord;

// C15 handshake answer (I52Ctda_QueryC15Arm). Side-effect free. databaseId is an input: the answer is armed only
// when it equals the scratch database recorded by BOOT-01 and the running row is the armed C15 row.
typedef struct I52CtdaC15Arm
{
    uint32_t size;                      // sizeof(I52CtdaC15Arm)
    uint64_t databaseId;                // in: the payload's working database
    int32_t armed;                      // out: 1 only for C15N16N-SM with a matching database
    int32_t databaseMatches;            // out: databaseId equals the BOOT-01 scratch database
    uint64_t triggerObjectOldId;        // out: F-TRIGGER-MOD ObjectId (asOldId)
    uint64_t retainedDataId;            // out: retained callback data slot (SET-RETAIN-CALLBACK-DATA)
    wchar_t probeId[64];                // out
} I52CtdaC15Arm;

typedef uint64_t (*I52CtdaLogAppendFn)(const I52CtdaRecord* record);
typedef int32_t (*I52CtdaTokenSetFn)(const wchar_t* tokenId, uint64_t deliveryId);
typedef int32_t (*I52CtdaQueryC15ArmFn)(I52CtdaC15Arm* arm);
typedef int32_t (*I52CtdaFinishFenceIsSetFn)(void);

// Registration entry of R-MANAGED-OBSERVER (RR-MANAGED-CMD). Implementation helper, not part of LOG-SEQ-01: the
// managed module hands the entry to R-NATIVE-ARX when it is loaded; DRIVER-CMD-01 calls it during REGISTER
// (subscribe=1) and CLN-BASE calls it again (subscribe=0). Returns 0 on success. The R3 smoke alone also calls it with
// I52CTDA_MANAGED_SMOKE_FENCE_READ: the managed module reads I52Ctda_FinishFenceIsSet(), records the value and returns it.
typedef int32_t (__stdcall *I52CtdaManagedEntryFn)(int32_t subscribe, uint64_t documentId, const wchar_t* probeId);
#define I52CTDA_MANAGED_SMOKE_FENCE_READ 2

// Payload removal export of R-PAYLOAD-ARX (bound by R-NATIVE-ARX in CLN-BASE). Returns the Acad::ErrorStatus of
// removeReactor as int32 (0 = eOk); I52CTDA_PAYLOAD_NOTHING_REGISTERED when RR-PAYLOAD-DB was never registered.
#define I52CTDA_PAYLOAD_NOTHING_REGISTERED (-1)
typedef int32_t (*I52CtdaPayloadRemoveReactorFn)(void);

#ifdef __cplusplus
}
#endif
