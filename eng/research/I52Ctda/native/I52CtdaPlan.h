#pragma once

// Generated V35 authority (V35Ids.inc, V35PlanTable.inc) as native types. The executor switches over I52Id values
// and interprets I52RowPlan; it never reads prose or selects a step at runtime.

#include <cstddef>
#include <cstdint>

enum class I52Id : uint16_t
{
    None = 0,
#define I52_ID(name, text, kind) name,
#include "V35Ids.inc"
#undef I52_ID
    Count
};

struct I52PlanMarker { bool mustBePresent; I52Id id; };
struct I52PlanBinding { I52Id marker; I52Id stage; };

// Field order is fixed by V35PlanCompiler.GenerateNativePlans (NPM-V35 row schema order).
struct I52RowPlan
{
    const wchar_t* probeId;
    const wchar_t* rowApprovalHash;
    I52Id primaryAuthority;
    I52Id scheduleOrigin;
    I52Id schedulerChain;
    const I52Id* header; size_t headerCount;
    I52Id driver;
    const I52Id* registrations; size_t registrationCount;
    I52Id bodyObserver;
    const I52Id* guards; size_t guardCount;
    const I52Id* setup; size_t setupCount;
    I52Id trigger;
    I52Id executionContext;
    I52Id primaryTransaction;
    I52Id executionTransaction;
    const I52Id* top; size_t topCount;
    const I52Id* locks; size_t lockCount;
    const I52Id* contexts; size_t contextCount;
    I52Id mutation;
    I52Id surface;
    I52Id schedulePoint;
    I52Id executionPoint;
    const I52Id* before; size_t beforeCount;
    const I52Id* after; size_t afterCount;
    const I52Id* verifiers; size_t verifierCount;
    const I52PlanMarker* markers; size_t markerCount;
    const I52PlanBinding* bindings; size_t bindingCount;
    const I52Id* observations; size_t observationCount;
    const I52Id* unknowns; size_t unknownCount;
    const I52Id* fails; size_t failCount;
    I52Id passClass;
    I52Id failClass;
    I52Id cleanup;
    const I52Id* cleanupSteps; size_t cleanupStepCount;
    I52Id fence;
    const I52Id* tokens; size_t tokenCount;
};

namespace I52Plan
{
const wchar_t* text(I52Id id);
const wchar_t* kind(I52Id id);
I52Id parse(const wchar_t* text);
const I52RowPlan* find(const wchar_t* probeId);
size_t count();
const I52RowPlan& at(size_t index);
bool contains(const I52Id* ids, size_t count, I52Id id);
inline bool has(const I52Id* ids, size_t n, I52Id id) { return contains(ids, n, id); }
}
