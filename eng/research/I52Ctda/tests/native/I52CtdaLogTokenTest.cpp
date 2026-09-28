// D-2 regression (build machine only; no AutoCAD): the real LOG-SEQ-01 code of R-NATIVE-ARX (I52CtdaLog.cpp) appends
// every token record of a governed row through the I52Ctda_TokenSet export, and each record must carry the exact
// CommandIdentity that was current (I52CTDA_PROBE). Built with the Debug CRT, which overwrites freed heap blocks, so a
// CommandIdentity pointer into a destroyed temporary is corrupted deterministically instead of by chance.
//
//   I52CtdaNativeTests.exe write <ProbeId> <log.jsonl>   appends the row's token records and checks the log handle
//                                                        (readable while open, not inheritable)
//   I52CtdaNativeTests.exe check <ProbeId> <log.jsonl>   verifies every token record
//   I52CtdaNativeTests.exe lockrelease                   D-4 COMMAND-END-WINDOW classification (I52CtdaLockReleaseTest.cpp)
#include "I52CtdaAuthority.h"
#include "I52CtdaLog.h"

#include <Windows.h>

#include <crtdbg.h>
#include <cstdio>
#include <cstdlib>
#include <fstream>
#include <io.h>
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

void ignoreInvalidParameter(const wchar_t*, const wchar_t*, const wchar_t*, unsigned int, uintptr_t) {}

// Harness-defect regression: while R-NATIVE-ARX holds the log open, another process can read it (FILE_SHARE_READ), and
// the CRT handle behind the log is not inheritable, so no child process keeps it open after the PID exits.
void checkLogHandle(const std::wstring& path)
{
    HANDLE reader = CreateFileW(path.c_str(), GENERIC_READ, FILE_SHARE_READ | FILE_SHARE_WRITE | FILE_SHARE_DELETE, nullptr, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, nullptr);
    require(reader != INVALID_HANDLE_VALUE, "log readable while the writer holds it (error " + std::to_string(GetLastError()) + ")");
    if (reader != INVALID_HANDLE_VALUE)
    {
        char buffer[64]{};
        DWORD read = 0;
        require(ReadFile(reader, buffer, sizeof(buffer), &read, nullptr) && read > 0, "shared read returns the log bytes");
        CloseHandle(reader);
    }
    wchar_t wanted[MAX_PATH]{};
    GetFullPathNameW(path.c_str(), MAX_PATH, wanted, nullptr);
    _set_thread_local_invalid_parameter_handler(ignoreInvalidParameter);
    _CrtSetReportMode(_CRT_ASSERT, 0);
    int found = 0;
    for (int fd = 0; fd < 256; ++fd)
    {
        const intptr_t os = _get_osfhandle(fd);
        if (os == -1 || os == -2) continue;
        wchar_t name[MAX_PATH + 8]{};
        if (GetFinalPathNameByHandleW(reinterpret_cast<HANDLE>(os), name, MAX_PATH + 8, FILE_NAME_NORMALIZED) == 0) continue;
        const std::wstring final = name;
        if (final.size() < wcslen(wanted) || _wcsicmp(final.c_str() + final.size() - wcslen(wanted), wanted) != 0) continue;
        DWORD flags = 0;
        require(GetHandleInformation(reinterpret_cast<HANDLE>(os), &flags) != 0 && (flags & HANDLE_FLAG_INHERIT) == 0, "log handle is not inheritable");
        ++found;
    }
    require(found == 1, "exactly one CRT handle holds the log (" + std::to_string(found) + ")");
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

int runLockReleaseTests();

int wmain(int argc, wchar_t** argv)
{
    if (argc == 2 && std::wstring(argv[1]) == L"lockrelease") return runLockReleaseTests();
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
        checkLogHandle(path);
    }
    else
    {
        const size_t records = check(path);
        require(records == expected, "token record count " + std::to_string(records) + " of " + std::to_string(expected));
        std::printf("%s D-2 token CommandIdentity %ls: %zu token records\n", failures == 0 ? "PASS" : "FAIL", probe.c_str(), records);
    }
    return failures == 0 ? 0 : 1;
}
