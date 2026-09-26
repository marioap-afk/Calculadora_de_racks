// D-2 regression (build machine only; no AutoCAD): the real LOG-SEQ-01 code of R-NATIVE-ARX (I52CtdaLog.cpp) appends
// every token record of a governed row through the I52Ctda_TokenSet export, and each record must carry the exact
// CommandIdentity that was current (I52CTDA_PROBE). Built with the Debug CRT, which overwrites freed heap blocks, so a
// CommandIdentity pointer into a destroyed temporary is corrupted deterministically instead of by chance.
//
//   I52CtdaNativeTests.exe write <ProbeId> <log.jsonl>   appends the row's token records (the log handle is not
//                                                        sharable, so it is read only after this process exits)
//   I52CtdaNativeTests.exe check <ProbeId> <log.jsonl>   verifies every token record
#include "I52CtdaAuthority.h"
#include "I52CtdaLog.h"

#include <Windows.h>

#include <cstdio>
#include <fstream>
#include <string>

namespace
{
int failures = 0;

void require(bool condition, const std::string& message)
{
    if (condition) return;
    ++failures;
    std::printf("FAIL %s\n", message.c_str());
}

std::string field(const std::string& line, const std::string& name)
{
    const std::string key = "\"" + name + "\":\"";
    const size_t start = line.find(key);
    if (start == std::string::npos) return "<missing>";
    const size_t end = line.find('"', start + key.size());
    return end == std::string::npos ? "<unterminated>" : line.substr(start + key.size(), end - start - key.size());
}

// Every token of the row, accepted in an activation of one of its stages, then again (DUPLICATE); plus one unknown
// token (rejected). All three paths append a token record. Returns the number of token records appended.
size_t write(const I52RowPlan& plan)
{
    I52Log& log = I52Log::instance();
    log.setPlan(&plan);
    log.setCommandIdentity(L"I52CTDA_PROBE");
    size_t records = 0, accepted = 0;
    for (size_t i = 0; i < plan.tokenCount; ++i)
    {
        const I52Id token = plan.tokens[i];
        for (uint16_t s = 1; s < static_cast<uint16_t>(I52Id::Count); ++s)
        {
            const I52Id stage = static_cast<I52Id>(s);
            if (std::wstring(I52Plan::kind(stage)) != L"Stage" || !I52Auth::tokenStageAllowed(token, stage)) continue;
            log.open(stage);
            break;
        }
        accepted += static_cast<size_t>(I52Ctda_TokenSet(I52Plan::text(token), I52CTDA_DELIVERY_CURRENT));
        I52Ctda_TokenSet(I52Plan::text(token), I52CTDA_DELIVERY_CURRENT);
        records += 2;
    }
    I52Ctda_TokenSet(L"TOK-NOT-A-TOKEN", I52CTDA_DELIVERY_CURRENT);
    require(accepted == plan.tokenCount, "every row token accepted once");
    return records + 1;
}

size_t check(const std::wstring& path)
{
    std::ifstream stream(path, std::ios::binary);
    require(stream.is_open(), "log readable");
    std::string line;
    size_t records = 0;
    while (std::getline(stream, line))
    {
        const std::string event = field(line, "EventOrMarkerId");
        if (event.rfind("TOK-", 0) != 0) continue;
        ++records;
        const std::string command = field(line, "CommandIdentity");
        require(command == "I52CTDA_PROBE", "token record " + event + " CommandIdentity '" + command + "'");
        // The unknown token has no activation (DeliveryId 0) and is malformed by design; row tokens never are.
        if (event != "TOK-NOT-A-TOKEN") require(line.find("\"Malformed\":true") == std::string::npos, "token record " + event + " flagged malformed");
    }
    return records;
}
}

int wmain(int argc, wchar_t** argv)
{
    if (argc != 4) { std::printf("usage: write|check <ProbeId> <log.jsonl>\n"); return 2; }
    const std::wstring mode = argv[1], probe = argv[2], path = argv[3];
    const I52RowPlan* plan = I52Plan::find(probe.c_str());
    require(plan != nullptr && plan->tokenCount > 0, "plan with completion tokens");
    if (plan == nullptr) return 1;
    const size_t expected = plan->tokenCount * 2 + 1;
    if (mode == L"write")
    {
        DeleteFileW(path.c_str());
        SetEnvironmentVariableW(L"I52_CTDA_PROBE_ID", probe.c_str());
        SetEnvironmentVariableW(L"I52_CTDA_EVENT_LOG", path.c_str());
        I52Log::instance().configure();
        require(I52Log::instance().ready(), "log ready");
        require(write(*plan) == expected, "token records appended");
    }
    else
    {
        const size_t records = check(path);
        require(records == expected, "token record count " + std::to_string(records) + " of " + std::to_string(expected));
        std::printf("%s D-2 token CommandIdentity %ls: %zu token records\n", failures == 0 ? "PASS" : "FAIL", probe.c_str(), records);
    }
    return failures == 0 ? 0 : 1;
}
