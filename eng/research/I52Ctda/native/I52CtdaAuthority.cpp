#include "I52CtdaAuthority.h"

namespace
{
struct Pair { I52Id a; I52Id b; };
struct IdText { I52Id id; const wchar_t* text; };
struct GuardKey { I52Id guard; const wchar_t* family; const wchar_t* key; };
struct ChainStep { I52Id chain; I52Id step; int index; };
struct PhaseEntry { I52Id driver; const wchar_t* phase; };

const Pair kTokenStages[] = {
#define I52_TOKEN_STAGE(token, stage) { token, stage },
#define I52_TOKEN_BOUND(token, marker)
#include "V35Authority.inc"
#undef I52_TOKEN_STAGE
#undef I52_TOKEN_BOUND
};
const Pair kTokenBound[] = {
#define I52_TOKEN_STAGE(token, stage)
#define I52_TOKEN_BOUND(token, marker) { token, marker },
#include "V35Authority.inc"
#undef I52_TOKEN_STAGE
#undef I52_TOKEN_BOUND
};
const IdText kStageNamespaces[] = {
#define I52_STAGE_NAMESPACE(stage, ns) { stage, ns },
#include "V35Authority.inc"
#undef I52_STAGE_NAMESPACE
};
const Pair kMarkerStages[] = {
#define I52_MARKER_STAGE(marker, stage) { marker, stage },
#include "V35Authority.inc"
#undef I52_MARKER_STAGE
};
const IdText kLockAnchors[] = {
#define I52_LOCK_ANCHOR(stage, anchor) { stage, anchor },
#include "V35Authority.inc"
#undef I52_LOCK_ANCHOR
};
const I52Auth::Guard kGuards[] = {
#define I52_GUARD(id, family, target, targetText) { id, family, target, targetText },
#define I52_GUARD_KEY(guard, family, key)
#include "V35Authority.inc"
#undef I52_GUARD
#undef I52_GUARD_KEY
};
const GuardKey kGuardKeys[] = {
#define I52_GUARD(id, family, target, targetText)
#define I52_GUARD_KEY(guard, family, key) { guard, family, key },
#include "V35Authority.inc"
#undef I52_GUARD
#undef I52_GUARD_KEY
};
const I52Auth::Trigger kTriggers[] = {
#define I52_TRIGGER(id, target, openMode, targetClass, driver, modelSpaceWrite) { id, target, openMode, targetClass, driver, modelSpaceWrite },
#include "V35Authority.inc"
#undef I52_TRIGGER
};
const ChainStep kChainSteps[] = {
#define I52_CHAIN_STEP(chain, step, index) { chain, step, index },
#include "V35Authority.inc"
#undef I52_CHAIN_STEP
};
const Pair kExecTransactions[] = {
#define I52_EXEC(exec, transaction) { exec, transaction },
#define I52_EXEC_LOCK(exec, lock)
#include "V35Authority.inc"
#undef I52_EXEC
#undef I52_EXEC_LOCK
};
const Pair kExecLocks[] = {
#define I52_EXEC(exec, transaction)
#define I52_EXEC_LOCK(exec, lock) { exec, lock },
#include "V35Authority.inc"
#undef I52_EXEC
#undef I52_EXEC_LOCK
};
const I52Auth::Registration kRegistrations[] = {
#define I52_REGISTRATION(id, reactorClass, notifier, objectReactor) { id, reactorClass, notifier, objectReactor },
#include "V35Authority.inc"
#undef I52_REGISTRATION
};
const Pair kMutationWrites[] = {
#define I52_MUTATION_WRITE(mutation, target) { mutation, target },
#include "V35Authority.inc"
#undef I52_MUTATION_WRITE
};
const Pair kCleanupSteps[] = {
#define I52_CLEANUP_STEP(action, step) { action, step },
#include "V35Authority.inc"
#undef I52_CLEANUP_STEP
};
const I52Id kSetupOrder[] = {
#define I52_SETUP_ORDER(setup) setup,
#include "V35Authority.inc"
#undef I52_SETUP_ORDER
};
const PhaseEntry kPhases[] = {
#define I52_PHASE(driver, phase) { driver, phase },
#include "V35Authority.inc"
#undef I52_PHASE
};
struct GateNumbers { int timeout; int external; int postFinish; };
const GateNumbers kGate = {
#define I52_FINISH_GATE(timeout, external, postFinish) timeout, external, postFinish
#include "V35Authority.inc"
#undef I52_FINISH_GATE
};
}

bool I52Auth::tokenStageAllowed(I52Id token, I52Id stage)
{
    for (const Pair& p : kTokenStages) if (p.a == token && p.b == stage) return true;
    // TOK-LOCK-RELEASE follows the stage bound to MARK-LOCK-RELEASE (LOCK-RELEASE-BIND-01 anchors).
    for (const Pair& p : kTokenBound) if (p.a == token && !lockAnchor(stage).empty()) return true;
    return false;
}

bool I52Auth::tokenIsBound(I52Id token)
{
    for (const Pair& p : kTokenBound) if (p.a == token) return true;
    return false;
}

std::wstring I52Auth::stageNamespace(I52Id stage)
{
    for (const IdText& p : kStageNamespaces) if (p.id == stage) return p.text;
    return {};
}

bool I52Auth::markerStageAllowed(I52Id marker, I52Id stage)
{
    for (const Pair& p : kMarkerStages) if (p.a == marker && p.b == stage) return true;
    return false;
}

std::wstring I52Auth::lockAnchor(I52Id stage)
{
    for (const IdText& p : kLockAnchors) if (p.id == stage) return p.text;
    return {};
}

const I52Auth::Guard* I52Auth::guard(I52Id id)
{
    for (const Guard& g : kGuards) if (g.id == id) return &g;
    return nullptr;
}

std::vector<std::wstring> I52Auth::guardKeys(I52Id guardId, const std::wstring& family)
{
    std::vector<std::wstring> keys;
    for (const GuardKey& k : kGuardKeys) if (k.guard == guardId && family == k.family) keys.emplace_back(k.key);
    return keys;
}

const I52Auth::Trigger* I52Auth::trigger(I52Id id)
{
    for (const Trigger& t : kTriggers) if (t.id == id) return &t;
    return nullptr;
}

std::vector<I52Id> I52Auth::chainSteps(I52Id chain)
{
    std::vector<I52Id> steps;
    for (const ChainStep& s : kChainSteps) if (s.chain == chain) steps.push_back(s.step);
    return steps;
}

I52Id I52Auth::execTransaction(I52Id exec)
{
    for (const Pair& p : kExecTransactions) if (p.a == exec) return p.b;
    return I52Id::None;
}

std::vector<I52Id> I52Auth::execLocks(I52Id exec)
{
    std::vector<I52Id> locks;
    for (const Pair& p : kExecLocks) if (p.a == exec) locks.push_back(p.b);
    return locks;
}

const I52Auth::Registration* I52Auth::registration(I52Id id)
{
    for (const Registration& r : kRegistrations) if (r.id == id) return &r;
    return nullptr;
}

std::vector<I52Id> I52Auth::mutationWrites(I52Id mutation)
{
    std::vector<I52Id> writes;
    for (const Pair& p : kMutationWrites) if (p.a == mutation) writes.push_back(p.b);
    return writes;
}

std::vector<I52Id> I52Auth::cleanupSteps(I52Id action)
{
    std::vector<I52Id> steps;
    for (const Pair& p : kCleanupSteps) if (p.a == action) steps.push_back(p.b);
    return steps;
}

int I52Auth::setupOrderIndex(I52Id setup)
{
    int index = 0;
    for (I52Id s : kSetupOrder) { if (s == setup) return index; ++index; }
    return -1;
}

std::vector<std::wstring> I52Auth::phases(I52Id driver)
{
    std::vector<std::wstring> result;
    for (const PhaseEntry& p : kPhases) if (p.driver == driver) result.emplace_back(p.phase);
    return result;
}

int I52Auth::finishGateTimeoutSeconds() { return kGate.timeout; }
int I52Auth::finishGateExternalDeadlineSeconds() { return kGate.external; }
int I52Auth::finishGatePostFinishDeadlineSeconds() { return kGate.postFinish; }
