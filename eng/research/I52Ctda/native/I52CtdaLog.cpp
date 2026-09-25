#include "I52CtdaLog.h"
#include "I52CtdaAuthority.h"

#include <Windows.h>

#include <chrono>
#include <cstdio>
#include <cwchar>
#include <sstream>

namespace
{
std::wstring environment(const wchar_t* name)
{
    wchar_t buffer[32768]{};
    const DWORD length = GetEnvironmentVariableW(name, buffer, static_cast<DWORD>(_countof(buffer)));
    if (length == 0 || length >= _countof(buffer)) return {};
    return std::wstring(buffer, length);
}

std::string utf8(const std::wstring& value)
{
    if (value.empty()) return {};
    const int size = WideCharToMultiByte(CP_UTF8, 0, value.c_str(), static_cast<int>(value.size()), nullptr, 0, nullptr, nullptr);
    std::string result(static_cast<size_t>(size), '\0');
    WideCharToMultiByte(CP_UTF8, 0, value.c_str(), static_cast<int>(value.size()), result.data(), size, nullptr, nullptr);
    return result;
}

void jsonString(std::string& out, const std::wstring& value)
{
    out.push_back('"');
    for (const unsigned char ch : utf8(value))
    {
        if (ch == '\\' || ch == '"') { out.push_back('\\'); out.push_back(static_cast<char>(ch)); }
        else if (ch < 0x20) { char buffer[8]; std::snprintf(buffer, sizeof(buffer), "\\u%04x", ch); out += buffer; }
        else out.push_back(static_cast<char>(ch));
    }
    out.push_back('"');
}

// "k=v;k=v" -> {"k":"v",...}. Pairs without '=' are kept under the key "_" so nothing is silently dropped.
void jsonPayload(std::string& out, const std::wstring& payload)
{
    out.push_back('{');
    bool first = true;
    size_t start = 0;
    while (start <= payload.size() && !payload.empty())
    {
        size_t end = payload.find(L';', start);
        if (end == std::wstring::npos) end = payload.size();
        const std::wstring pair = payload.substr(start, end - start);
        if (!pair.empty())
        {
            const size_t eq = pair.find(L'=');
            if (!first) out.push_back(',');
            first = false;
            jsonString(out, eq == std::wstring::npos ? L"_" : pair.substr(0, eq));
            out.push_back(':');
            jsonString(out, eq == std::wstring::npos ? pair : pair.substr(eq + 1));
        }
        start = end + 1;
    }
    out.push_back('}');
}

const wchar_t* field(const wchar_t* value, bool& malformed)
{
    if (value == nullptr || *value == L'\0') { malformed = true; return L"<MISSING>"; }
    return value;
}
}

std::wstring I52Hex(uint64_t value)
{
    wchar_t buffer[24]{};
    std::swprintf(buffer, _countof(buffer), L"0x%llX", static_cast<unsigned long long>(value));
    return buffer;
}

std::wstring I52Narrow(const std::string& value) { return std::wstring(value.begin(), value.end()); }

I52Log& I52Log::instance() { static I52Log log; return log; }

I52Log::I52Log()
{
    auto* section = new CRITICAL_SECTION();
    InitializeCriticalSection(section);
    section_ = section;
}

void I52Log::configure()
{
    EnterCriticalSection(static_cast<CRITICAL_SECTION*>(section_));
    probeId_ = environment(L"I52_CTDA_PROBE_ID");
    path_ = environment(L"I52_CTDA_EVENT_LOG");
    if (!path_.empty() && file_ == nullptr)
    {
        FILE* stream = nullptr;
        if (_wfopen_s(&stream, path_.c_str(), L"ab") == 0) file_ = stream;
    }
    ready_ = file_ != nullptr && !probeId_.empty();
    LeaveCriticalSection(static_cast<CRITICAL_SECTION*>(section_));
}

// LOG-SEQ-01: Sequence is assigned with one InterlockedIncrement64 on the single process-global counter inside the
// log critical section, so the file order is the Sequence order. Malformed records are still written, flagged.
uint64_t I52Log::append(const I52CtdaRecord& record)
{
    if (record.size != sizeof(I52CtdaRecord)) return 0;
    auto* section = static_cast<CRITICAL_SECTION*>(section_);
    EnterCriticalSection(section);
    if (file_ == nullptr) { LeaveCriticalSection(section); return 0; }
    bool malformed = false;
    const wchar_t* probe = field(record.probeId, malformed);
    const wchar_t* stage = field(record.stageId, malformed);
    uint64_t delivery = record.deliveryId;
    I52Id providedStage = I52Id::None;
    if (std::wcscmp(stage, I52CTDA_STAGE_CURRENT) == 0 && stageProvider_ != nullptr)
    {
        stageProvider_(providedStage, delivery);
        stage = I52Plan::text(providedStage);
    }
    const wchar_t* driver = field(record.driverOrSchedulerId, malformed);
    const wchar_t* module = field(record.moduleId, malformed);
    const wchar_t* command = field(record.commandIdentity, malformed);
    if (std::wcscmp(command, I52CTDA_COMMAND_CURRENT) == 0) command = commandIdentity_.c_str();
    const wchar_t* event = field(record.eventOrMarkerId, malformed);
    const I52Id stageId = I52Plan::parse(stage);
    if (delivery == I52CTDA_DELIVERY_NEW)
    {
        delivery = 0;
        if (stageId != I52Id::None) { delivery = ++deliveryCounter_; currentActivation_[stageId] = delivery; activationStage_[delivery] = stageId; }
    }
    else if (delivery == I52CTDA_DELIVERY_CURRENT) { auto it = currentActivation_.find(stageId); delivery = it == currentActivation_.end() ? 0 : it->second; }
    if (delivery == 0) malformed = true;
    const uint64_t sequence = static_cast<uint64_t>(InterlockedIncrement64(&sequence_));
    const auto micros = std::chrono::duration_cast<std::chrono::microseconds>(std::chrono::system_clock::now().time_since_epoch()).count();
    std::string line = "{\"Sequence\":" + std::to_string(sequence) + ",\"ProbeId\":";
    jsonString(line, probe);
    line += ",\"StageId\":"; jsonString(line, stage);
    line += ",\"DeliveryId\":" + std::to_string(delivery);
    line += ",\"DriverOrSchedulerId\":"; jsonString(line, driver);
    line += ",\"ModuleId\":"; jsonString(line, module);
    line += ",\"PID\":" + std::to_string(GetCurrentProcessId()) + ",\"TID\":" + std::to_string(GetCurrentThreadId());
    line += ",\"DocumentId\":"; jsonString(line, I52Hex(record.documentId));
    line += ",\"DatabaseId\":"; jsonString(line, I52Hex(record.databaseId));
    line += ",\"CommandIdentity\":"; jsonString(line, command);
    line += ",\"EventOrMarkerId\":"; jsonString(line, event);
    line += ",\"Payload\":"; jsonPayload(line, record.payload == nullptr ? L"" : record.payload);
    line += ",\"TimestampUnixMicros\":" + std::to_string(micros);
    if (malformed) line += ",\"Malformed\":true";
    line += "}\n";
    ++moduleRecords_[module];
    ++moduleEventRecords_[std::wstring(module) + L"|" + event];
    auto* stream = static_cast<FILE*>(file_);
    std::fwrite(line.data(), 1, line.size(), stream);
    std::fflush(stream);
    LeaveCriticalSection(section);
    return sequence;
}

int I52Log::countFrom(const std::wstring& module) const
{
    auto* section = static_cast<CRITICAL_SECTION*>(section_);
    EnterCriticalSection(section);
    auto it = moduleRecords_.find(module);
    const int count = it == moduleRecords_.end() ? 0 : it->second;
    LeaveCriticalSection(section);
    return count;
}

int I52Log::countOf(const std::wstring& module, const std::wstring& event) const
{
    auto* section = static_cast<CRITICAL_SECTION*>(section_);
    EnterCriticalSection(section);
    auto it = moduleEventRecords_.find(module + L"|" + event);
    const int count = it == moduleEventRecords_.end() ? 0 : it->second;
    LeaveCriticalSection(section);
    return count;
}

uint64_t I52Log::lastSequence() const { return static_cast<uint64_t>(InterlockedCompareExchange64(const_cast<volatile int64_t*>(&sequence_), 0, 0)); }

uint64_t I52Log::open(I52Id stage)
{
    auto* section = static_cast<CRITICAL_SECTION*>(section_);
    EnterCriticalSection(section);
    const uint64_t id = ++deliveryCounter_;
    currentActivation_[stage] = id;
    activationStage_[id] = stage;
    LeaveCriticalSection(section);
    return id;
}

// Latest current activation among the stages the token may be set in (payload and managed modules pass
// I52CTDA_DELIVERY_CURRENT); 0 when none is active, which the token table rejects.
uint64_t I52Log::currentForToken(I52Id token) const
{
    auto* section = static_cast<CRITICAL_SECTION*>(section_);
    EnterCriticalSection(section);
    uint64_t best = 0;
    for (const auto& [stage, id] : currentActivation_) if (I52Auth::tokenStageAllowed(token, stage) && id > best) best = id;
    LeaveCriticalSection(section);
    return best;
}

void I52Log::setCommandIdentity(const std::wstring& command)
{
    auto* section = static_cast<CRITICAL_SECTION*>(section_);
    EnterCriticalSection(section);
    commandIdentity_ = command.empty() ? L"NONE" : command;
    LeaveCriticalSection(section);
}

std::wstring I52Log::commandIdentity() const
{
    auto* section = static_cast<CRITICAL_SECTION*>(section_);
    EnterCriticalSection(section);
    const std::wstring command = commandIdentity_;
    LeaveCriticalSection(section);
    return command;
}

uint64_t I52Log::current(I52Id stage) const
{
    auto* section = static_cast<CRITICAL_SECTION*>(section_);
    EnterCriticalSection(section);
    auto it = currentActivation_.find(stage);
    const uint64_t id = it == currentActivation_.end() ? 0 : it->second;
    LeaveCriticalSection(section);
    return id;
}

I52Id I52Log::stageOf(uint64_t deliveryId) const
{
    auto* section = static_cast<CRITICAL_SECTION*>(section_);
    EnterCriticalSection(section);
    auto it = activationStage_.find(deliveryId);
    const I52Id stage = it == activationStage_.end() ? I52Id::None : it->second;
    LeaveCriticalSection(section);
    return stage;
}

int32_t I52Log::setToken(I52Id token, uint64_t deliveryId, std::wstring& reason)
{
    auto* section = static_cast<CRITICAL_SECTION*>(section_);
    EnterCriticalSection(section);
    int32_t accepted = 0;
    auto it = activationStage_.find(deliveryId);
    if (std::wcscmp(I52Plan::kind(token), L"CompletionToken") != 0) reason = L"NOT-A-TOKEN";
    else if (plan_ == nullptr || !I52Plan::contains(plan_->tokens, plan_->tokenCount, token)) reason = L"NOT-IN-ROW";
    else if (it == activationStage_.end()) reason = L"UNKNOWN-DELIVERY";
    else if (!I52Auth::tokenStageAllowed(token, it->second)) reason = L"WRONG-STAGE";
    else if (tokens_.count(token) != 0) reason = L"DUPLICATE";
    else { tokens_[token] = deliveryId; accepted = 1; reason = L"ACCEPTED"; }
    LeaveCriticalSection(section);
    return accepted;
}

bool I52Log::tokenSet(I52Id token) const
{
    auto* section = static_cast<CRITICAL_SECTION*>(section_);
    EnterCriticalSection(section);
    const bool set = tokens_.count(token) != 0;
    LeaveCriticalSection(section);
    return set;
}

std::vector<I52Id> I52Log::missingTokens(const I52RowPlan& plan) const
{
    std::vector<I52Id> missing;
    for (size_t i = 0; i < plan.tokenCount; ++i) if (!tokenSet(plan.tokens[i])) missing.push_back(plan.tokens[i]);
    return missing;
}

void I52Log::setFence() { InterlockedExchange(&fence_, 1); }
int32_t I52Log::fenceIsSet() const { return InterlockedCompareExchange(const_cast<volatile long*>(&fence_), 0, 0) != 0 ? 1 : 0; }

void I52Log::setC15Arm(const C15Arm& arm)
{
    auto* section = static_cast<CRITICAL_SECTION*>(section_);
    EnterCriticalSection(section);
    c15_ = arm;
    LeaveCriticalSection(section);
}

int32_t I52Log::queryC15Arm(I52CtdaC15Arm* answer) const
{
    if (answer == nullptr || answer->size != sizeof(I52CtdaC15Arm)) return 0;
    auto* section = static_cast<CRITICAL_SECTION*>(section_);
    EnterCriticalSection(section);
    answer->databaseMatches = answer->databaseId != 0 && answer->databaseId == scratchDatabase_ ? 1 : 0;
    answer->armed = c15_.armed && answer->databaseMatches && c15_.databaseId == scratchDatabase_ ? 1 : 0;
    answer->triggerObjectOldId = answer->armed ? c15_.triggerObjectOldId : 0;
    answer->retainedDataId = answer->armed ? c15_.retainedDataId : 0;
    wcsncpy_s(answer->probeId, probeId_.c_str(), _TRUNCATE);
    LeaveCriticalSection(section);
    return 1;
}

// ------------------------------------------------------------------ LOG-SEQ-01 C ABI exports (frozen names)
extern "C" __declspec(dllexport) uint64_t I52Ctda_LogAppend(const I52CtdaRecord* record)
{
    return record == nullptr ? 0 : I52Log::instance().append(*record);
}

extern "C" __declspec(dllexport) int32_t I52Ctda_TokenSet(const wchar_t* tokenId, uint64_t deliveryId)
{
    I52Log& log = I52Log::instance();
    const I52Id token = I52Plan::parse(tokenId);
    const uint64_t delivery = deliveryId == I52CTDA_DELIVERY_CURRENT ? log.currentForToken(token) : deliveryId;
    std::wstring reason;
    const int32_t accepted = log.setToken(token, delivery, reason);
    const I52Id deliveryStage = log.stageOf(delivery);
    const std::wstring payload = L"status=" + reason;
    I52CtdaRecord record{ sizeof(I52CtdaRecord), log.probeId().c_str(), I52Plan::text(deliveryStage), delivery,
        log.plan() == nullptr ? L"NONE" : I52Plan::text(log.plan()->driver), L"R-NATIVE-ARX", log.scratchDocument(), log.scratchDatabase(),
        log.commandIdentity().c_str(), tokenId == nullptr ? L"" : tokenId, payload.c_str() };
    log.append(record);
    return accepted;
}

extern "C" __declspec(dllexport) int32_t I52Ctda_QueryC15Arm(I52CtdaC15Arm* arm) { return I52Log::instance().queryC15Arm(arm); }

extern "C" __declspec(dllexport) int32_t I52Ctda_FinishFenceIsSet(void) { return I52Log::instance().fenceIsSet(); }

int32_t I52Ctda_TokenSetNative(I52Id token, uint64_t deliveryId) { return I52Ctda_TokenSet(I52Plan::text(token), deliveryId); }

// Implementation helper, not part of LOG-SEQ-01: R-MANAGED-OBSERVER hands over its RR-MANAGED-CMD registration entry
// when it is loaded. DRIVER-CMD-01 calls the entry during REGISTER and CLN-BASE calls it again to unsubscribe.
extern "C" __declspec(dllexport) int32_t I52Ctda_BindManagedObserver(I52CtdaManagedEntryFn entry)
{
    if (entry == nullptr) return 0;
    I52Log::instance().setManagedEntry(entry);
    return 1;
}
