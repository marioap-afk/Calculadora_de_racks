#include "aced.h"
#include "acdocman.h"
#include "rxregsvc.h"
#include "I52CtdaRuntime.h"

#include <Windows.h>

#include <chrono>
#include <cstdlib>
#include <filesystem>
#include <fstream>
#include <iomanip>
#include <sstream>
#include <string>

namespace
{
constexpr const wchar_t* kCommandGroup = L"I52_CTDA_RESEARCH";
constexpr const wchar_t* kSmokeCommand = L"I52CTDA_SMOKE";

std::string jsonEscape(const std::string& value)
{
    std::ostringstream escaped;
    for (const unsigned char ch : value)
    {
        switch (ch)
        {
        case '\\': escaped << "\\\\"; break;
        case '"': escaped << "\\\""; break;
        case '\n': escaped << "\\n"; break;
        case '\r': escaped << "\\r"; break;
        case '\t': escaped << "\\t"; break;
        default:
            if (ch < 0x20)
            {
                escaped << "\\u" << std::hex << std::setw(4) << std::setfill('0') << static_cast<int>(ch);
            }
            else
            {
                escaped << ch;
            }
        }
    }
    return escaped.str();
}

std::string narrow(const wchar_t* value)
{
    if (value == nullptr)
    {
        return {};
    }

    const int size = WideCharToMultiByte(CP_UTF8, 0, value, -1, nullptr, 0, nullptr, nullptr);
    if (size <= 1)
    {
        return {};
    }

    std::string result(static_cast<size_t>(size), '\0');
    WideCharToMultiByte(CP_UTF8, 0, value, -1, result.data(), size, nullptr, nullptr);
    result.pop_back();
    return result;
}

std::filesystem::path outputPath()
{
    wchar_t buffer[32768]{};
    const DWORD length = GetEnvironmentVariableW(L"I52_CTDA_OUTPUT", buffer, static_cast<DWORD>(_countof(buffer)));
    if (length == 0 || length >= _countof(buffer))
    {
        return {};
    }
    return std::filesystem::path(buffer);
}

void writeSmokeReport()
{
    const auto path = outputPath();
    if (path.empty())
    {
        acutPrintf(L"\nI52 CT-DA: I52_CTDA_OUTPUT is missing.\n");
        return;
    }

    std::filesystem::create_directories(path.parent_path());
    AcApDocument* document = acDocManager == nullptr ? nullptr : acDocManager->curDocument();
    AcDbDatabase* database = document == nullptr ? nullptr : document->database();
    const bool applicationContext = acDocManager != nullptr && acDocManager->isApplicationContext();
    const auto now = std::chrono::system_clock::now().time_since_epoch();
    const auto micros = std::chrono::duration_cast<std::chrono::microseconds>(now).count();

    std::ofstream stream(path, std::ios::binary | std::ios::trunc);
    stream << "{\n"
           << "  \"schemaVersion\": 1,\n"
           << "  \"instrument\": \"I52CtdaNative\",\n"
           << "  \"stage\": \"A_SMOKE\",\n"
           << "  \"result\": \"PASS\",\n"
           << "  \"sequence\": 1,\n"
           << "  \"timestampUnixMicros\": " << micros << ",\n"
           << "  \"processId\": " << GetCurrentProcessId() << ",\n"
           << "  \"threadId\": " << GetCurrentThreadId() << ",\n"
           << "  \"applicationContext\": " << (applicationContext ? "true" : "false") << ",\n"
           << "  \"documentIdentity\": \"" << document << "\",\n"
           << "  \"databaseIdentity\": \"" << database << "\",\n"
           << "  \"documentName\": \"" << jsonEscape(narrow(document == nullptr ? L"" : document->fileName())) << "\",\n"
           << "  \"cleanup\": \"NO_REACTORS_OR_QUEUED_WORK\"\n"
           << "}\n";
    stream.flush();
    acutPrintf(L"\nI52 CT-DA smoke report written.\n");
}
}

extern "C" AcRx::AppRetCode acrxEntryPoint(AcRx::AppMsgCode message, void* packet)
{
    switch (message)
    {
    case AcRx::kInitAppMsg:
        acrxDynamicLinker->unlockApplication(packet);
        acrxRegisterAppMDIAware(packet);
        acedRegCmds->addCommand(kCommandGroup, kSmokeCommand, kSmokeCommand, ACRX_CMD_MODAL, writeSmokeReport);
        I52CtdaRuntime::instance().registerReactors();
        break;
    case AcRx::kUnloadAppMsg:
        I52CtdaRuntime::instance().unregisterReactors();
        acedRegCmds->removeGroup(kCommandGroup);
        break;
    default:
        break;
    }
    return AcRx::kRetOK;
}
