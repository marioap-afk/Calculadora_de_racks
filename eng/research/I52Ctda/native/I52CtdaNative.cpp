#include "aced.h"
#include "acdocman.h"
#include "rxregsvc.h"
#include "I52CtdaFixture.h"
#include "I52CtdaRuntime.h"

#include <Windows.h>

#include <chrono>
#include <cstdlib>
#include <filesystem>
#include <fstream>
#include <iomanip>
#include <sstream>
#include <string>
#include <system_error>
#include <vector>

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

constexpr const char* kSmokeIdentity = "I52CTDA_SMOKE";
constexpr const char* kCompletionToken = "I52CTDA_SMOKE_COMPLETE";

I52CtdaFixture& smokeFixture()
{
    static I52CtdaFixture fixture;
    return fixture;
}

std::string statusText(Acad::ErrorStatus status) { return std::to_string(static_cast<int>(status)); }

const char* flag(bool value) { return value ? "true" : "false"; }

// Raw pre-trigger SM facts; the managed SmRules own their classification.
std::string smFactsJson(const I52SmFacts& facts)
{
    std::ostringstream text;
    text << "{\"link\": {\"keyPresent\": " << flag(facts.link.keyPresent) << ", \"identityMatches\": " << flag(facts.link.identityMatches)
         << ", \"erased\": " << flag(facts.link.erased) << ", \"isXrecord\": " << flag(facts.link.isXrecord)
         << ", \"resbufCount\": " << facts.link.resbufCount << ", \"isText\": " << flag(facts.link.isText)
         << ", \"carriersFound\": " << facts.link.carriersFound << ", \"value\": \"" << jsonEscape(facts.link.value) << "\"}"
         << ", \"anchorALive\": " << flag(facts.anchorALive) << ", \"bRead\": " << flag(facts.bRead) << ", \"bLive\": " << flag(facts.bLive)
         << ", \"foreignReferenceInserts\": " << facts.foreignReferenceInserts << ", \"cBound\": " << flag(facts.cBound)
         << ", \"cFound\": " << flag(facts.cFound) << ", \"cLive\": " << flag(facts.cLive)
         << ", \"cAtCreationPosition\": " << flag(facts.cAtCreationPosition)
         << ", \"aEvidence\": \"" << jsonEscape(facts.aEvidence) << "\", \"cEvidence\": \"" << jsonEscape(facts.cEvidence) << "\"}";
    return text.str();
}

// Smoke only: materializes and binds the FEC-V34 fixture in its own bootstrap transaction, resolves the seven
// identities and evaluates cleanup from runtime state. It never dispatches a governed ProbeId.
void runSmoke()
{
    I52CtdaRuntime& runtime = I52CtdaRuntime::instance();
    const auto path = outputPath();
    if (path.empty())
    {
        acutPrintf(L"\nI52 CT-DA: I52_CTDA_OUTPUT is missing; smoke FAIL.\n");
        return;
    }

    runtime.setCommandIdentity(kSmokeIdentity);
    std::vector<std::string> failures;
    const bool loggerReady = runtime.loggerReady();
    if (!loggerReady) failures.push_back("LOGGER_UNAVAILABLE");
    runtime.mark("MARK-SMOKE-BEGIN", {});
    const unsigned long long firstSequence = runtime.lastSequence();

    AcApDocument* document = acDocManager == nullptr ? nullptr : acDocManager->curDocument();
    AcDbDatabase* database = document == nullptr ? nullptr : document->database();
    I52CtdaFixture& fixture = smokeFixture();
    I52FixtureResolution resolution{};
    resolution.declared = I52CtdaFixture::kDeclaredIdentities;
    std::string materializeStatus = "NOT_ATTEMPTED";
    std::string registerStatus = "NOT_ATTEMPTED";
    int activeTransactionsObserved = -1;
    I52SmFacts smFacts{};
    bool linkCarrierRemoved = false;

    if (!loggerReady) failures.push_back("BOOTSTRAP_SKIPPED_LOGGER_UNAVAILABLE");
    else if (database == nullptr) failures.push_back("NO_SCRATCH_DATABASE");
    else if (fixture.isBound()) failures.push_back("FIXTURE_ALREADY_BOUND");
    else
    {
        AcTransactionManager* manager = runtime.resolveTransactionManager(document);
        if (runtime.startTransaction(manager) == nullptr) failures.push_back("BOOTSTRAP_TRANSACTION_UNAVAILABLE");
        else
        {
            I52FixtureIds ids{};
            const Acad::ErrorStatus materialized = I52CtdaFixture::materialize(database, manager, ids);
            const Acad::ErrorStatus closed = materialized == Acad::eOk ? runtime.endTransaction(manager) : runtime.abortTransaction(manager);
            materializeStatus = statusText(materialized);
            runtime.mark("MARK-SMOKE-FIXTURE-MATERIALIZED", "materialize=" + materializeStatus + " transactionClose=" + statusText(closed));
            if (materialized != Acad::eOk) failures.push_back("FIXTURE_MATERIALIZATION_FAILED:" + materializeStatus);
            else if (closed != Acad::eOk) failures.push_back("BOOTSTRAP_TRANSACTION_END_FAILED:" + statusText(closed));
            else
            {
                fixture.bind(ids);
                runtime.setLinkCarrierOutstanding(true);
                const Acad::ErrorStatus registered = runtime.registerFixtureObjects(ids.triggerEraseObject, ids.siblingA);
                registerStatus = statusText(registered);
                runtime.mark("MARK-SMOKE-FIXTURE-BOUND", "registerFixtureObjects=" + registerStatus);
                if (registered != Acad::eOk) failures.push_back("FIXTURE_REACTOR_REGISTRATION_FAILED:" + registerStatus);
                resolution = fixture.resolve();
                smFacts = fixture.captureSm(true);
                runtime.mark("MARK-SMOKE-FIXTURE-RESOLVED", std::to_string(resolution.resolved) + "/" + std::to_string(resolution.declared)
                    + " binding=" + (resolution.bindingResolved ? "RESOLVED" : "MISMATCH"));
                if (resolution.resolved != I52CtdaFixture::kDeclaredIdentities) failures.push_back("FIXTURE_IDENTITIES_UNRESOLVED:" + std::to_string(resolution.resolved));
                if (!resolution.bindingResolved) failures.push_back("SM_LINK_BINDING_UNRESOLVED");

                // Cleanup of the SM-LINK binding in its own transaction; the obligation clears only after verification.
                if (runtime.startTransaction(manager) == nullptr) failures.push_back("CLEANUP_TRANSACTION_UNAVAILABLE");
                else
                {
                    const Acad::ErrorStatus removed = fixture.removeSmResources(manager);
                    const Acad::ErrorStatus ended = removed == Acad::eOk ? runtime.endTransaction(manager) : runtime.abortTransaction(manager);
                    linkCarrierRemoved = removed == Acad::eOk && ended == Acad::eOk && fixture.linkCarrierRemoved();
                    if (linkCarrierRemoved) runtime.setLinkCarrierOutstanding(false);
                    runtime.mark("MARK-SMOKE-LINK-CARRIER-REMOVED", "remove=" + statusText(removed) + " transactionClose=" + statusText(ended)
                        + " verified=" + (linkCarrierRemoved ? "true" : "false"));
                    if (!linkCarrierRemoved) failures.push_back("LINK_CARRIER_NOT_REMOVED");
                }
            }
        }
        activeTransactionsObserved = manager == nullptr ? -1 : manager->numActiveTransactions();
    }

    const Acad::ErrorStatus detached = runtime.unregisterFixtureObjects();
    if (detached != Acad::eOk) failures.push_back("FIXTURE_REACTOR_DETACH_FAILED:" + statusText(detached));
    if (!fixture.isBound()) failures.push_back("FIXTURE_NOT_BOUND");
    const bool cleanupComplete = runtime.cleanupComplete();
    const bool fixtureReactorsAttached = runtime.fixtureReactorsAttached();
    const int governed = runtime.governedDispatches();
    if (!cleanupComplete) failures.push_back("CLEANUP_INCOMPLETE");
    if (fixtureReactorsAttached) failures.push_back("FIXTURE_REACTORS_ATTACHED");
    if (governed != 0) failures.push_back("GOVERNED_PROBE_DISPATCHED");
    const bool pass = failures.empty();
    std::ostringstream cleanupState;
    cleanupState << "ownedTransactions=" << runtime.ownedTransactions() << " ownedLocks=" << runtime.ownedLocks()
                 << " queuedWork=" << runtime.queuedWork() << " activeGuards=" << runtime.activeGuards()
                 << " fixtureReactorsAttached=" << (fixtureReactorsAttached ? "true" : "false")
                 << " cleanupComplete=" << (cleanupComplete ? "true" : "false");
    runtime.mark("MARK-SMOKE-END", std::string(pass ? "PASS " : "FAIL ") + cleanupState.str());
    const unsigned long long lastSequence = runtime.lastSequence();

    std::error_code error;
    if (path.has_parent_path()) std::filesystem::create_directories(path.parent_path(), error);
    const bool applicationContext = acDocManager != nullptr && acDocManager->isApplicationContext();
    const auto micros = std::chrono::duration_cast<std::chrono::microseconds>(std::chrono::system_clock::now().time_since_epoch()).count();
    std::ostringstream failureList;
    for (size_t i = 0; i < failures.size(); ++i) failureList << (i == 0 ? "" : ", ") << '"' << jsonEscape(failures[i]) << '"';

    std::ofstream stream(path, std::ios::binary | std::ios::trunc);
    stream << "{\n"
           << "  \"schemaVersion\": 3,\n"
           << "  \"instrument\": \"I52CtdaNative\",\n"
           << "  \"stage\": \"A_SMOKE\",\n"
           << "  \"commandIdentity\": \"" << kSmokeIdentity << "\",\n"
           << "  \"completionToken\": \"" << kCompletionToken << "\",\n"
           << "  \"result\": \"" << (pass ? "PASS" : "FAIL") << "\",\n"
           << "  \"failures\": [" << failureList.str() << "],\n"
           << "  \"timestampUnixMicros\": " << micros << ",\n"
           << "  \"processId\": " << GetCurrentProcessId() << ",\n"
           << "  \"threadId\": " << GetCurrentThreadId() << ",\n"
           << "  \"applicationContext\": " << (applicationContext ? "true" : "false") << ",\n"
           << "  \"documentIdentity\": \"" << static_cast<const void*>(document) << "\",\n"
           << "  \"databaseIdentity\": \"" << static_cast<const void*>(database) << "\",\n"
           << "  \"documentName\": \"" << jsonEscape(narrow(document == nullptr ? L"" : document->fileName())) << "\",\n"
           << "  \"eventLog\": \"" << jsonEscape(runtime.eventLogPath().u8string()) << "\",\n"
           << "  \"loggerReady\": " << (loggerReady ? "true" : "false") << ",\n"
           << "  \"sequenceFirst\": " << firstSequence << ",\n"
           << "  \"sequenceLast\": " << lastSequence << ",\n"
           << "  \"fixture\": {\n"
           << "    \"materializeStatus\": \"" << materializeStatus << "\",\n"
           << "    \"registerFixtureObjectsStatus\": \"" << registerStatus << "\",\n"
           << "    \"bound\": " << (fixture.isBound() ? "true" : "false") << ",\n"
           << "    \"declaredIdentities\": " << resolution.declared << ",\n"
           << "    \"resolvedIdentities\": " << resolution.resolved << ",\n"
           << "    \"smLinkBinding\": \"" << (resolution.bindingResolved ? "RESOLVED" : "MISMATCH") << "\",\n"
           << "    \"smPrecondition\": " << smFactsJson(smFacts) << ",\n"
           << "    \"snapshot\": \"" << jsonEscape(resolution.snapshot) << "\"\n"
           << "  },\n"
           << "  \"cleanup\": {\n"
           << "    \"ownedTransactions\": " << runtime.ownedTransactions() << ",\n"
           << "    \"ownedLocks\": " << runtime.ownedLocks() << ",\n"
           << "    \"queuedWork\": " << runtime.queuedWork() << ",\n"
           << "    \"activeGuards\": " << runtime.activeGuards() << ",\n"
           << "    \"fixtureReactorsAttached\": " << (fixtureReactorsAttached ? "true" : "false") << ",\n"
           << "    \"activeTransactionsObserved\": " << activeTransactionsObserved << ",\n"
           << "    \"linkCarrierRemoved\": " << (linkCarrierRemoved ? "true" : "false") << ",\n"
           << "    \"cleanupComplete\": " << (cleanupComplete ? "true" : "false") << ",\n"
           << "    \"globalReactors\": \"RETAINED_UNTIL_ARX_UNLOAD\"\n"
           << "  },\n"
           << "  \"governedProbesDispatched\": " << governed << "\n"
           << "}\n";
    stream.flush();
    const bool written = stream.good();
    runtime.setCommandIdentity({});
    acutPrintf(written ? L"\nI52 CT-DA smoke report written.\n" : L"\nI52 CT-DA: smoke report could not be written; smoke FAIL.\n");
}
}

extern "C" AcRx::AppRetCode acrxEntryPoint(AcRx::AppMsgCode message, void* packet)
{
    switch (message)
    {
    case AcRx::kInitAppMsg:
        acrxDynamicLinker->unlockApplication(packet);
        acrxRegisterAppMDIAware(packet);
        acedRegCmds->addCommand(kCommandGroup, kSmokeCommand, kSmokeCommand, ACRX_CMD_MODAL, runSmoke);
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
