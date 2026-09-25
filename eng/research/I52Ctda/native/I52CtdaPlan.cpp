#include "I52CtdaPlan.h"

#include <cwchar>

namespace
{
struct IdText { const wchar_t* text; const wchar_t* kind; };

const IdText kIdText[] = {
    { L"NONE", L"None" },
#define I52_ID(name, text, kind) { text, kind },
#include "V35Ids.inc"
#undef I52_ID
};
static_assert(sizeof(kIdText) / sizeof(kIdText[0]) == static_cast<size_t>(I52Id::Count), "identifier table");

#include "V35PlanTable.inc"
}

const wchar_t* I52Plan::text(I52Id id)
{
    const auto index = static_cast<size_t>(id);
    return index < static_cast<size_t>(I52Id::Count) ? kIdText[index].text : L"NONE";
}

const wchar_t* I52Plan::kind(I52Id id)
{
    const auto index = static_cast<size_t>(id);
    return index < static_cast<size_t>(I52Id::Count) ? kIdText[index].kind : L"None";
}

I52Id I52Plan::parse(const wchar_t* value)
{
    if (value == nullptr) return I52Id::None;
    for (size_t i = 1; i < static_cast<size_t>(I52Id::Count); ++i)
        if (std::wcscmp(kIdText[i].text, value) == 0) return static_cast<I52Id>(i);
    return I52Id::None;
}

// Exact ProbeId match only: an unknown ProbeId resolves to no plan and is rejected before any governed dispatch.
const I52RowPlan* I52Plan::find(const wchar_t* probeId)
{
    if (probeId == nullptr) return nullptr;
    for (const I52RowPlan& plan : kI52Plans)
        if (std::wcscmp(plan.probeId, probeId) == 0) return &plan;
    return nullptr;
}

size_t I52Plan::count() { return kI52PlanCount; }
const I52RowPlan& I52Plan::at(size_t index) { return kI52Plans[index]; }

bool I52Plan::contains(const I52Id* ids, size_t n, I52Id id)
{
    for (size_t i = 0; i < n; ++i) if (ids[i] == id) return true;
    return false;
}
