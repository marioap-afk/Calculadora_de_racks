#pragma once

#include "I52CtdaPlan.h"

#include <string>
#include <vector>

// Read-only views over V35Authority.inc (generated from EXEC-CATALOG-V35-1 attributes).
namespace I52Auth
{
struct Trigger { I52Id id; I52Id target; const wchar_t* openMode; const wchar_t* targetClass; I52Id driver; bool modelSpaceWrite; };
struct Guard { I52Id id; const wchar_t* family; I52Id armTarget; const wchar_t* armTargetText; };
struct Registration { I52Id id; const wchar_t* reactorClass; const wchar_t* notifier; bool objectReactor; };

bool tokenStageAllowed(I52Id token, I52Id stage);
bool tokenIsBound(I52Id token);
std::wstring stageNamespace(I52Id stage);
bool markerStageAllowed(I52Id marker, I52Id stage);
std::wstring lockAnchor(I52Id stage);
const Guard* guard(I52Id id);
std::vector<std::wstring> guardKeys(I52Id guard, const std::wstring& family);
const Trigger* trigger(I52Id id);
std::vector<I52Id> chainSteps(I52Id chain);
I52Id execTransaction(I52Id exec);
std::vector<I52Id> execLocks(I52Id exec);
const Registration* registration(I52Id id);
std::vector<I52Id> mutationWrites(I52Id mutation);
std::vector<I52Id> cleanupSteps(I52Id action);
int setupOrderIndex(I52Id setup);
std::vector<std::wstring> phases(I52Id driver);
int finishGateTimeoutSeconds();
int finishGateExternalDeadlineSeconds();
int finishGatePostFinishDeadlineSeconds();
}
